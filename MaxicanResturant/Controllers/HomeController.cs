using MaxicanResturant.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace MaxicanResturant.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)] // This attribute is used to prevent caching of the error page, ensuring that users always see the most up-to-date error information. It sets the cache duration to 0, specifies that the cache location is none, and indicates that the response should not be stored.
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }); // This line creates a new instance of the ErrorViewModel class and sets its RequestId property. The value of RequestId is determined by checking if there is a current activity (which represents the current operation or request) and using its Id. If there is no current activity, it falls back to using the TraceIdentifier from the HttpContext, which is a unique identifier for the current HTTP request. This allows the error view to display relevant information about the request that caused the error. If an error occurs, the user will see the error page with the RequestId, which can be helpful for debugging and tracking issues in the application.
        }
    }
}
