using System.ComponentModel.DataAnnotations;

namespace Backend.ApiModel.Product
{
    public class GetProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int QuantityInStock { get; set; }
        public string DisplayImage { get; set; }
        public string Brand { get; set; }

        public string CategoryName { get; set; }
    }
}
