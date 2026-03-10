using Microsoft.AspNetCore.Identity;

namespace MaxicanResturant.Models
{
    public class ApplicationUser : IdentityUser
    {
       public ICollection<Order>? Orders { get; set; }
    }
}
