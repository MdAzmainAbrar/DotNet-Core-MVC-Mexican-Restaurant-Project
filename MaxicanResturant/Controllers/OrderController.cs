using MaxicanResturant.Data;
using MaxicanResturant.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MaxicanResturant.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context; // This is the database context that allows us to interact with the database. It is injected into the controller through the constructor, enabling us to perform CRUD operations on our entities such as Products and Orders.
        private Repository<Product> _products; // This is a repository for the Product entity. It provides methods to perform CRUD operations on products in the database. The repository pattern helps to abstract away the data access logic and makes it easier to manage and test the code.
        private Repository<Order> _orders; // This is a repository for the Order entity. Similar to the Product repository, it provides methods to perform CRUD operations on orders in the database. It allows us to manage orders without directly interacting with the database context, promoting separation of concerns and improving code maintainability.
        private readonly UserManager<ApplicationUser> _userManager; // This is the UserManager from ASP.NET Core Identity, which provides methods for managing user accounts, including creating users, validating credentials, and retrieving user information. It is injected into the controller to allow us to associate orders with the currently logged-in user and to manage user-related functionality.

        public OrderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager) // This is the constructor for the OrderController. It takes in the ApplicationDbContext and UserManager as parameters, which are injected by the dependency injection framework. Inside the constructor, we initialize the private fields for the database context, user manager, and repositories for products and orders. This setup allows us to use these services throughout the controller to manage orders and interact with the database.
        {
            _context = context; // Initialize the database context
            _userManager = userManager; // Initialize the user manager
            _products = new Repository<Product>(context); // Initialize the product repository
            _orders = new Repository<Order>(context); // Initialize the order repository
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Create() // This action method is responsible for displaying the order creation page. It is decorated with the [Authorize] attribute, which means that only authenticated users can access this action. When a GET request is made to this action, it retrieves or creates an OrderViewModel from the session or other state management. The OrderViewModel contains a list of OrderItemViewModel and a list of products. The products are retrieved asynchronously from the product repository. Finally, the method returns the view with the OrderViewModel, allowing the user to see the available products and add them to their order.
        {
            //ViewBag.Products = await _products.GetAllAsync();

            //Retrieve or create an OrderViewModel from session or other state management
            var model = HttpContext.Session.Get<OrderViewModel>("OrderViewModel") ?? new OrderViewModel // If there is no OrderViewModel in the session, create a new one with an empty list of OrderItemViewModel and a list of products retrieved from the product repository.
            {
                OrderItems = new List<OrderItemViewModel>(), // Initialize the OrderItems list as an empty list to hold the items that the user will add to their order.
                Products = await _products.GetAllAsync() // Retrieve the list of products asynchronously from the product repository and assign it to the Products property of the OrderViewModel. This allows the view to display the available products for the user to choose from when creating their order.
            };


            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddItem(int prodId, int prodQty) // This action method is responsible for adding an item to the user's order. It is decorated with the [HttpPost] attribute, indicating that it handles POST requests, and the [Authorize] attribute, which means that only authenticated users can access this action. The method takes in two parameters: prodId (the ID of the product to be added) and prodQty (the quantity of the product to be added). Inside the method, it first retrieves the product from the database using the provided prodId. If the product is not found, it returns a NotFound result. Then, it retrieves or creates an OrderViewModel from the session or other state management. It checks if the product is already in the order; if it is, it updates the quantity. If not, it adds a new OrderItemViewModel to the OrderItems list in the OrderViewModel. Finally, it updates the total amount of the order and saves the updated OrderViewModel back to the session before redirecting back to the Create action to show the updated order items.
        {
            var product = await _context.Products.FindAsync(prodId); // Retrieve the product from the database using the provided prodId. This is done asynchronously to avoid blocking the thread while waiting for the database operation to complete. If the product is not found, it will return null.
            if (product == null)
            {
                return NotFound(); // If the product is not found in the database, return a NotFound result, which will typically result in a 404 error being sent to the client.
            }

            // Retrieve or create an OrderViewModel from session or other state management
            var model = HttpContext.Session.Get<OrderViewModel>("OrderViewModel") ?? new OrderViewModel // If there is no OrderViewModel in the session, create a new one with an empty list of OrderItemViewModel and a list of products retrieved from the product repository.
            {
                OrderItems = new List<OrderItemViewModel>(), // Initialize the OrderItems list as an empty list to hold the items that the user will add to their order.
                Products = await _products.GetAllAsync() // Retrieve the list of products asynchronously from the product repository and assign it to the Products property of the OrderViewModel. This allows the view to display the available products for the user to choose from when creating their order.
            };

            // Check if the product is already in the order
            var existingItem = model.OrderItems.FirstOrDefault(oi => oi.ProductId == prodId); // Check if there is already an OrderItemViewModel in the OrderItems list that has the same ProductId as the prodId being added. If such an item exists, it will be stored in the existingItem variable; otherwise, existingItem will be null.

            // If the product is already in the order, update the quantity
            if (existingItem != null) // If the product is already in the order, update the quantity of the existing item.
            {
                existingItem.Quantity += prodQty; // If the existing item is found, increment its Quantity property by the prodQty being added. This allows the user to add more of the same product to their order without creating a new entry for it.
            }
            else
            {
                model.OrderItems.Add(new OrderItemViewModel 
                {
                    ProductId = product.ProductId,
                    Price = product.Price,
                    Quantity = prodQty,
                    ProductName = product.Name
                }); // If the product is not already in the order, create a new OrderItemViewModel with the product's details and the specified quantity, and add it to the OrderItems list in the OrderViewModel. This allows the user to add a new product to their order. Here, we set the ProductId, Price, Quantity, and ProductName properties of the new OrderItemViewModel based on the retrieved product and the quantity specified by the user. This new item is then added to the OrderItems list, which will be displayed in the order summary for the user.
            }

            // Update the total amount
            model.TotalAmount = model.OrderItems.Sum(oi => oi.Price * oi.Quantity);

            // Save updated OrderViewModel to session
            HttpContext.Session.Set("OrderViewModel", model);

            // Redirect back to Create to show updated order items
            return RedirectToAction("Create", model);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Cart() // This action method is responsible for displaying the user's shopping cart. It is decorated with the [HttpGet] attribute, indicating that it handles GET requests, and the [Authorize] attribute, which means that only authenticated users can access this action. When a GET request is made to this action, it retrieves the OrderViewModel from the session or other state management. If the OrderViewModel is null or if there are no items in the order (i.e., OrderItems.Count == 0), it redirects the user back to the Create action to encourage them to add items to their cart. If there are items in the order, it returns the view with the OrderViewModel, allowing the user to see their shopping cart and proceed to checkout.
        {

            // Retrieve the OrderViewModel from session or other state management
            var model = HttpContext.Session.Get<OrderViewModel>("OrderViewModel"); // Retrieve the OrderViewModel from the session using the key "OrderViewModel". This will allow us to access the current state of the user's order, including the items they have added to their cart and the total amount.

            if (model == null || model.OrderItems.Count == 0)
            {
                return RedirectToAction("Create"); // If the OrderViewModel is null (i.e., there is no order in the session) or if there are no items in the order (i.e., OrderItems.Count == 0), redirect the user back to the Create action. This encourages the user to add items to their cart before they can view it, ensuring that they have a meaningful shopping cart experience.
            }

            return View(model); // If there are items in the order, return the view with the OrderViewModel. This will allow the user to see their shopping cart, including the items they have added and the total amount, and proceed to checkout if they wish.
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> PlaceOrder() // This action method is responsible for placing the user's order. It is decorated with the [HttpPost] attribute, indicating that it handles POST requests, and the [Authorize] attribute, which means that only authenticated users can access this action. When a POST request is made to this action, it retrieves the OrderViewModel from the session or other state management. If the OrderViewModel is null or if there are no items in the order (i.e., OrderItems.Count == 0), it redirects the user back to the Create action to encourage them to add items to their cart. If there are items in the order, it creates a new Order entity and populates it with the order details, including the order date, total amount, and user ID. It then adds OrderItem entities to the Order based on the items in the OrderViewModel. Finally, it saves the Order entity to the database using the order repository, clears the OrderViewModel from the session, and redirects the user to the ViewOrders action to see their placed orders.
        {
            var model = HttpContext.Session.Get<OrderViewModel>("OrderViewModel"); // Retrieve the OrderViewModel from the session using the key "OrderViewModel". This will allow us to access the current state of the user's order, including the items they have added to their cart and the total amount.
            if (model == null || model.OrderItems.Count == 0)
            {
                return RedirectToAction("Create"); // If the OrderViewModel is null (i.e., there is no order in the session) or if there are no items in the order (i.e., OrderItems.Count == 0), redirect the user back to the Create action. This encourages the user to add items to their cart before they can place an order, ensuring that they have a meaningful shopping cart experience.
            }

            // Create a new Order entity
            Order order = new Order
            {
                OrderDate = DateTime.Now,
                TotalAmount = model.TotalAmount,
                UserId = _userManager.GetUserId(User)
            }; // Create a new Order entity and populate its properties. The OrderDate is set to the current date and time, the TotalAmount is set to the total amount calculated in the OrderViewModel, and the UserId is set to the ID of the currently logged-in user, which is retrieved using the UserManager.

            // Add OrderItems to the Order entity
            foreach (var item in model.OrderItems)
            {
                order.OrderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Price
                }); // Iterate through each OrderItemViewModel in the OrderItems list of the OrderViewModel and create a corresponding OrderItem entity for each one. The ProductId, Quantity, and Price properties of the OrderItem are set based on the values from the OrderItemViewModel. These OrderItem entities are then added to the Order's OrderItems collection, which will be saved to the database when the Order is saved.
            }

            // Save the Order entity to the database
            await _orders.AddAsync(order);

            // Clear the OrderViewModel from session or other state management
            HttpContext.Session.Remove("OrderViewModel");

            // Redirect to the Order Confirmation page
            return RedirectToAction("ViewOrders");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ViewOrders()
        {
            var userId = _userManager.GetUserId(User); // Retrieve the ID of the currently logged-in user using the UserManager. This will allow us to query the database for orders that are associated with this user, ensuring that users can only see their own orders. Having the user ID is essential for maintaining data privacy and security, as it prevents users from accessing orders that belong to other users.

            var userOrders = await _orders.GetAllByIdAsync(userId, "UserId", new QueryOptions<Order> // Retrieve all orders from the database that are associated with the current user. The GetAllByIdAsync method is called on the order repository, passing in the userId, the name of the property to filter by ("UserId"), and a QueryOptions object that specifies any additional options for the query. In this case, we include the related OrderItems and their associated Product information by setting the Includes property of the QueryOptions to "OrderItems.Product". This allows us to retrieve all necessary information about the user's orders in a single query, improving performance and reducing the number of database calls. Having the user ID and including related entities ensures that we can display a comprehensive view of the user's orders, including the products they have ordered and the quantities, while also maintaining data security by only showing orders that belong to the current user.
            {
                Includes = "OrderItems.Product" 
            }); // The GetAllByIdAsync method will return a list of Order entities that match the specified user ID, along with their related OrderItems and Product information. This allows us to display the user's orders in the view with all relevant details.

            return View(userOrders); // Return the view with the list of user orders. This will allow the user to see all of their past orders, including the products they ordered, the quantities, and the total amounts. The view can be designed to display this information in a user-friendly format, such as a table or list, providing a clear overview of the user's order history.
        }



    }
}