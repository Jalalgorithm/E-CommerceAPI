

using Backend.Model;

namespace Backend.ApiModel.Cart
{
    public class CartItemDto
    {
        public Product Product { get; set; } = new Product();
        public int Quantity { get; set; }
    }
}
