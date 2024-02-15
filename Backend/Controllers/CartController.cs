using Backend.ApiModel.Base;
using Backend.ApiModel.Cart;
using Backend.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<ApiResponse> GetCartItem(string productIdenitfiers)
        {
            CartDto cartDto = new CartDto();
            cartDto.CartItems = new List<CartItemDto>();
            cartDto.SubTotal = 0;
            cartDto.ShippingFee = OrderHelper.ShippingFee;
            cartDto.TotalPrice = 0;
            var productDictionary = OrderHelper.GetProductDictionary(productIdenitfiers);

            foreach (var pair in productDictionary)
            {
                int productId = pair.Key;
                var product = _context.Products.Find(productId);
                if (product == null)
                {
                    continue;
                }

                var cartItemDto =  new CartItemDto();
                cartItemDto.Product = product;
                cartItemDto.Quantity = pair.Value;

                cartDto.CartItems.Add(cartItemDto);
                cartDto.SubTotal += product.Price* pair.Value;
                cartDto.TotalPrice = cartDto.SubTotal + cartDto.ShippingFee;

            }

            return new ApiResponse
            {
                Result = cartDto
            };
        }
    }
}
