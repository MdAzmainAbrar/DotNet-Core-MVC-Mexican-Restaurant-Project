using MaxicanResturant.Data;
using MaxicanResturant.Models;
using Microsoft.AspNetCore.Mvc;

namespace MaxicanResturant.Controllers
{
    public class ProductController : Controller
    {
        private Repository<Product> products; // Repository for managing Product entities. This allows for separation of concerns and easier testing.
        private Repository<Ingredient> ingredients; // Repository for managing Ingredient entities.
        private Repository<Category> categories; // Repository for managing Category entities.
        private readonly IWebHostEnvironment _webHostEnvironment; // Provides information about the web hosting environment. Used for handling file uploads.

        public ProductController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment) // Constructor that initializes the repositories and the web hosting environment.
        {
            products = new Repository<Product>(context); // Initializes the products repository with the provided database context.
            ingredients = new Repository<Ingredient>(context); // Initializes the ingredients repository with the provided database context.
            categories = new Repository<Category>(context); // Initializes the categories repository with the provided database context. 
            _webHostEnvironment = webHostEnvironment; // Assigns the web hosting environment to the private field for later use in file handling.
        }

        public async Task<IActionResult> Index() // Action method for displaying the list of products. It retrieves all products asynchronously and passes them to the view.
        {
            return View(await products.GetAllAsync()); // Retrieves all products from the repository asynchronously and returns the view with the list of products.
        }

        [HttpGet]
        public async Task<IActionResult> AddEdit(int id) // Action method for displaying the add/edit form for a product. It retrieves the necessary data (ingredients and categories) and determines whether to display an empty form for adding a new product or a populated form for editing an existing product based on the provided id.
        {
            ViewBag.Ingredients = await ingredients.GetAllAsync(); // Retrieves all ingredients from the repository asynchronously and stores them in the ViewBag for use in the view (e.g., to populate a dropdown or checklist).
            ViewBag.Categories = await categories.GetAllAsync(); // Retrieves all categories from the repository asynchronously and stores them in the ViewBag for use in the view (e.g., to populate a dropdown).
            if (id == 0)
            {
                ViewBag.Operation = "Add"; // Sets the operation type to "Add" in the ViewBag, which can be used in the view to determine whether to display an add or edit form.
                return View(new Product()); // Returns the view with a new, empty Product object for adding a new product.
            }
            else
            {
                Product product = await products.GetByIdAsync(id, new QueryOptions<Product> // Retrieves the product with the specified id from the repository asynchronously, including related entities (ProductIngredients and Category) based on the provided QueryOptions. This allows for eager loading of related data, which can be used in the view for editing the product.
                {
                    Includes = "ProductIngredients.Ingredient, Category"
                });
                ViewBag.Operation = "Edit"; // Sets the operation type to "Edit" in the ViewBag, which can be used in the view to determine whether to display an add or edit form.
                return View(product); // Returns the view with the retrieved Product object for editing.
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddEdit(Product product, int[] ingredientIds, int catId) // Action method for handling the form submission for adding or editing a product. It processes the submitted data, handles file uploads if an image is provided, and updates the product in the repository accordingly. It also manages the relationships between products and ingredients based on the selected ingredient IDs.
        {
            ViewBag.Ingredients = await ingredients.GetAllAsync(); // Retrieves all ingredients from the repository asynchronously and stores them in the ViewBag for use in the view (e.g., to populate a dropdown or checklist).
            ViewBag.Categories = await categories.GetAllAsync(); // Retrieves all categories from the repository asynchronously and stores them in the ViewBag for use in the view (e.g., to populate a dropdown).
            if (ModelState.IsValid) // Checks if the submitted model is valid based on the defined validation rules. If it is valid, the method proceeds to handle the product data and file upload.
            {

                if (product.ImageFile != null) // Checks if an image file has been uploaded. If so, it processes the file upload by saving the file to the server and updating the product's ImageUrl property with the unique file name.
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images"); // Combines the web root path with the "images" folder to determine the directory where uploaded images will be stored.
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + product.ImageFile.FileName; // Generates a unique file name by combining a GUID with the original file name to avoid conflicts.
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName); // Combines the uploads folder path with the unique file name to get the full file path.
                    using (var fileStream = new FileStream(filePath, FileMode.Create)) // Creates a new file stream to write the uploaded file to the specified file path. The FileMode.Create option ensures that a new file is created or an existing file is overwritten.
                    {
                        await product.ImageFile.CopyToAsync(fileStream); // Asynchronously copies the uploaded file to the file stream, effectively saving it to the server.
                    }
                    product.ImageUrl = uniqueFileName; // Updates the product's ImageUrl property with the unique file name, which can be used to display the image in the view.
                }

                if (product.ProductId == 0) // Checks if the product is new (ProductId is 0). If it is new, it sets the category ID and adds the selected ingredients.
                {

                    product.CategoryId = catId; // Sets the product's CategoryId property to the selected category ID from the form submission.

                    //add ingredients
                    foreach (int id in ingredientIds) // Iterates through the selected ingredient IDs and adds them to the product's ProductIngredients collection by creating new ProductIngredient objects that link the product with each selected ingredient.
                    {
                        product.ProductIngredients?.Add(new ProductIngredient { IngredientId = id, ProductId = product.ProductId }); // Adds a new ProductIngredient object to the product's ProductIngredients collection, linking the product with the ingredient specified by the current id in the loop. The null-conditional operator (?.) is used to ensure that the collection is not null before attempting to add items to it.
                    }

                    await products.AddAsync(product); // Asynchronously adds the new product to the repository, which will save it to the database. After adding the product, it redirects to the Index action to display the updated list of products.
                    return RedirectToAction("Index", "Product"); // Redirects to the Index action of the Product controller after successfully adding a new product.
                }
                else
                {
                    var existingProduct = await products.GetByIdAsync(product.ProductId, new QueryOptions<Product> { Includes = "ProductIngredients" }); // If the product is not new (ProductId is not 0), it retrieves the existing product from the repository asynchronously, including its related ProductIngredients. This allows for updating the existing product's properties and managing its relationships with ingredients.

                    if (existingProduct == null)
                    {
                        ModelState.AddModelError("", "Product not found."); // If the existing product is not found in the repository, it adds a model error indicating that the product was not found and returns the view with the current product data, allowing the user to correct any issues.
                        ViewBag.Ingredients = await ingredients.GetAllAsync(); // Retrieves all ingredients from the repository asynchronously and stores them in the ViewBag for use in the view (e.g., to populate a dropdown or checklist) when returning the view with the current product data.
                        ViewBag.Categories = await categories.GetAllAsync(); // Retrieves all categories from the repository asynchronously and stores them in the ViewBag for use in the view (e.g., to populate a dropdown) when returning the view with the current product data.
                        return View(product); // Returns the view with the current product data, allowing the user to correct any issues (e.g., if the product was not found).
                    }

                    existingProduct.Name = product.Name; // Updates the existing product's Name property with the value from the submitted product data.
                    existingProduct.Description = product.Description; // Updates the existing product's Description property with the value from the submitted product data.
                    existingProduct.Price = product.Price; // Updates the existing product's Price property with the value from the submitted product data.
                    existingProduct.Stock = product.Stock; // Updates the existing product's Stock property with the value from the submitted product data.
                    existingProduct.CategoryId = catId; // Updates the existing product's CategoryId property with the value from the submitted product data.

                    // Update product ingredients
                    existingProduct.ProductIngredients?.Clear(); // Clears the existing product's ProductIngredients collection to remove any previously associated ingredients. This is necessary to ensure that the updated list of ingredients reflects the current selection from the form submission.
                    foreach (int id in ingredientIds) // Iterates through the selected ingredient IDs and adds them to the existing product's ProductIngredients collection by creating new ProductIngredient objects that link the product with each selected ingredient. This updates the product's relationships with ingredients based on the current selection from the form submission.
                    {
                        existingProduct.ProductIngredients?.Add(new ProductIngredient { IngredientId = id, ProductId = product.ProductId }); // Adds a new ProductIngredient object to the existing product's ProductIngredients collection, linking the product with the ingredient specified by the current id in the loop. The null-conditional operator (?.) is used to ensure that the collection is not null before attempting to add items to it.
                    }

                    try
                    {
                        await products.UpdateAsync(existingProduct); // Asynchronously updates the existing product in the repository, which will save the changes to the database. After updating the product, it redirects to the Index action to display the updated list of products.
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", $"Error: {ex.GetBaseException().Message}"); // If an exception occurs during the update process, it adds a model error with the exception message and returns the view with the current product data, allowing the user to correct any issues.
                        ViewBag.Ingredients = await ingredients.GetAllAsync(); // Retrieves all ingredients from the repository asynchronously and stores them in the ViewBag for use in the view (e.g., to populate a dropdown or checklist) when returning the view with the current product data after an error occurs during the update process.
                        ViewBag.Categories = await categories.GetAllAsync(); // Retrieves all categories from the repository asynchronously and stores them in the ViewBag for use in the view (e.g., to populate a dropdown) when returning the view with the current product data after an error occurs during the update process.
                        return View(product); // Returns the view with the current product data, allowing the user to correct any issues (e.g., if an error occurred during the update process).
                    }
                }
            }
            return RedirectToAction("Index", "Product"); // If the model state is not valid, it redirects to the Index action of the Product controller. This could be improved by returning the view with the current product data and validation errors instead of redirecting, allowing the user to correct any issues without losing their input.
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id) // Action method for handling the deletion of a product. It attempts to delete the product with the specified id from the repository asynchronously. If the deletion is successful, it redirects to the Index action to display the updated list of products. If an exception occurs (e.g., if the product is not found), it adds a model error indicating that the product was not found and redirects to the Index action.
        {
            try
            {
                await products.DeleteAsync(id); // Asynchronously deletes the product with the specified id from the repository, which will remove it from the database. After deleting the product, it redirects to the Index action to display the updated list of products.
                return RedirectToAction("Index"); // Redirects to the Index action of the Product controller after successfully deleting a product.
            }
            catch
            {
                ModelState.AddModelError("", "Product not found."); // If an exception occurs during the deletion process (e.g., if the product is not found), it adds a model error indicating that the product was not found. However, since this method redirects to the Index action regardless of success or failure, the model error will not be displayed in the view. This could be improved by returning the view with the current product data and validation errors instead of redirecting, allowing the user to correct any issues without losing their input.
                return RedirectToAction("Index"); // Redirects to the Index action of the Product controller after an error occurs during the deletion process. This could be improved by returning the view with the current product data and validation errors instead of redirecting, allowing the user to correct any issues without losing their input.
            }
        }
    }
}
