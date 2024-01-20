using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Backend.ApiModel.Product
{
    public class CreateProductDto
    {
        [Required]
        [StringLength(255)]
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        public int QuantityInStock { get; set; }
        public IFormFile Image { get; set; }
        [MaxLength(100)]
        public string Brand { get; set; } = "";

        public int CategoryId { get; set; }
    }
}
