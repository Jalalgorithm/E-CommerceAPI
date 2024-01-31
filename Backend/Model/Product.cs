using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Backend.Model
{
    public class Product
    {
        public int Id { get; set; }

        [StringLength(255)]
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        [Precision(16,2)]
        public decimal Price { get; set; }
        public int QuantityInStock { get; set; }
        [MaxLength(100)]
        public string DisplayImage { get; set; } = "";
        [MaxLength(100)]
        public string Brand { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public OtherImagesForProduct Otherimages { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }

    }
}
