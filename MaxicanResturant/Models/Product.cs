using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;

namespace MaxicanResturant.Models
{
    public class Product
    {
        public Product()
        {
            ProductIngredients = new List<ProductIngredient>(); // Initialize the ProductIngredients collection to an empty list to avoid null reference issues when adding ingredients to a product. This ensures that the collection is always ready to be used, even if no ingredients have been added yet.
        }
        public int ProductId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int CategoryId { get; set; }

        [NotMapped] // This property is not mapped to the database, it's used to handle the uploaded image file in the application logic.
        public IFormFile? ImageFile { get; set; } // This property is used to handle the uploaded image file in the application logic. It is marked with the [NotMapped] attribute to indicate that it should not be mapped to a database column, as it is only used for processing the uploaded file and does not need to be stored in the database.
        public string ImageUrl { get; set; } = "https://via.placeholder.com/150";

        [ValidateNever]
        public Category? Category { get; set; } //A product belongs to a category

        [ValidateNever]
        public ICollection<OrderItem>? OrderItems { get; set; } //A product can be in many order items

        [ValidateNever]
        public ICollection<ProductIngredient>? ProductIngredients { get; set; } //A product can have many ingredients
    }
}