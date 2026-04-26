using Microsoft.AspNetCore.Identity;

namespace MaxicanResturant.Models
{
    public class ApplicationUser : IdentityUser
    {
       public ICollection<Order>? Orders { get; set; } //if we want to get all the orders of a user we can use this property to do that. this is a navigation property that allows us to access the orders associated with a user. it is defined as a collection of Order objects, and it is nullable (indicated by the ?), meaning that a user may not have any orders. this property can be used in our application to retrieve and manage the orders placed by a specific user.
    }
}
