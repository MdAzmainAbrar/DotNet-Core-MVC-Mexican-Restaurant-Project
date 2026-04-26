using MaxicanResturant.Data;
using MaxicanResturant.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found."); // Get the connection string from the configuration. If the connection string is not found, throw an exception to indicate the issue. This ensures that the application fails gracefully if the required configuration is missing, providing a clear error message about the missing connection string. 
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString)); // Configure the application to use SQL Server as the database provider, using the connection string retrieved from the configuration. This sets up the Entity Framework Core context to connect to the specified SQL Server database, allowing the application to perform database operations such as querying and saving data. some of the operations that we can perform with this configuration include creating, reading, updating, and deleting records in the database, as well as managing relationships between entities and executing complex queries.
builder.Services.AddDatabaseDeveloperPageExceptionFilter(); // Add a developer exception page filter for database-related exceptions. This provides detailed error information when a database error occurs during development, making it easier to diagnose and fix issues related to database operations. When an exception is thrown during a database operation, this filter will catch the exception and display a detailed error page with information about the error, including the stack trace and any relevant database context. This is particularly useful for developers to identify and resolve issues during the development phase of the application.

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>(); // Configure the application to use ASP.NET Core Identity for authentication and user management. This sets up the default identity system with the ApplicationUser class as the user entity, and it requires users to have a confirmed account before they can sign in. The AddEntityFrameworkStores method specifies that the identity system should use Entity Framework Core to store user information in the database, using the ApplicationDbContext as the context for managing user data. This configuration allows the application to handle user registration, login, and other identity-related features while ensuring that only users with confirmed accounts can access certain parts of the application. This setup is essential for securing the application and managing user authentication effectively.
builder.Services.AddControllersWithViews(); // Add support for controllers and views to the application. This enables the MVC (Model-View-Controller) pattern, allowing the application to handle HTTP requests using controllers and return views as responses. With this configuration, you can create controllers to manage the application's logic and views to render the user interface, facilitating a structured approach to building web applications with separation of concerns between data, presentation, and user interactions.

builder.Services.AddMemoryCache(); // Add support for in-memory caching. This allows the application to store frequently accessed data in memory, improving performance by reducing the need to repeatedly fetch data from the database or other external sources.
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set your timeout here
}); // Add support for session state management. This allows the application to store user-specific data across multiple requests, enabling features like user authentication, shopping carts, and other personalized experiences. The IdleTimeout option specifies the duration for which a session can remain idle before it is abandoned, in this case, 30 minutes. This means that if a user does not interact with the application for 30 minutes, their session will expire, and they may need to log in again or lose any unsaved data stored in the session. Here, we are configuring the session to ensure that user data is managed effectively while also providing a reasonable timeout to balance user convenience and security.

var app = builder.Build(); // Build the application. This compiles the configured services and middleware into a runnable application instance that can handle incoming HTTP requests and generate responses based on the defined routes, controllers, and views. The Build method finalizes the application's configuration and prepares it for execution, allowing it to start listening for requests and processing them according to the defined logic and behavior.

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint(); // In development mode, use the migrations endpoint to allow developers to apply database migrations directly from the application. This provides a convenient way to manage database schema changes during development without needing to use command-line tools or other external methods. The migrations endpoint can be accessed through a specific URL, and it allows developers to apply pending migrations to the database, ensuring that the database schema is up-to-date with the application's data model during the development process.
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts(); // Use HTTP Strict Transport Security (HSTS) to add a response header that instructs browsers to only use HTTPS for future requests. This helps to protect the application from certain types of man-in-the-middle attacks by ensuring that all communication between the client and server is encrypted.
}

app.UseHttpsRedirection(); // Redirect HTTP requests to HTTPS to ensure secure communication between the client and server.
app.UseRouting(); // Enable routing to define how the application responds to different URL patterns. This allows the application to map incoming requests to specific controllers and actions based on the defined routes, enabling a structured way to handle various endpoints and user interactions.

app.UseAuthorization(); // Enable authorization to restrict access to certain parts of the application based on user roles or permissions. This allows you to control who can access specific resources or perform certain actions within the application, enhancing security and ensuring that only authorized users can access sensitive information or functionality.

app.MapStaticAssets(); // Map static assets to serve files such as images, CSS, and JavaScript directly from the wwwroot folder. This allows the application to efficiently serve static content without needing to go through the MVC pipeline, improving performance for static resources.

app.UseSession(); // Enable session state management in the HTTP request pipeline. This allows the application to maintain user-specific data across multiple requests, enabling features like user authentication, shopping carts, and other personalized experiences. By calling UseSession, the application can read and write session data for each user, allowing for a more interactive and personalized user experience.

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Ingredient}/{action=Index}/{id?}")
    .WithStaticAssets(); // Map the default controller route to the Ingredient controller and its Index action. This means that when a user navigates to the root URL of the application, they will be directed to the Ingredient controller's Index action by default. The {id?} part of the route pattern indicates that an optional id parameter can be included in the URL, allowing for more flexible routing based on specific resource identifiers. By calling WithStaticAssets, we ensure that static files can be served correctly even when using this routing configuration.

app.MapRazorPages()
   .WithStaticAssets(); // Map Razor Pages routes and ensure that static files can be served correctly when using Razor Pages.

app.Run(); // Run the application. This starts the web server and begins listening for incoming HTTP requests, allowing the application to respond to user interactions and serve content based on the defined routes, controllers, and views. The Run method is the entry point for the application's execution, and it will keep the application running until it is stopped or shut down.
