using Backend.ApiModel.Base;
using Backend.ApiModel.OrderDtoModel;
using Backend.Data;
using Backend.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet]
        public async Task<ApiResponse> GetOrders (int? page)
        {
            var userId = JwtReader.GetUserId(User);
            string role =  _context.Users.Find(userId)?.Role ?? "";

            IQueryable<Order> query = _context.Orders
                .Include(u => u.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product);

            if(role != "Admin")
            {
                query = _context.Orders.Where(u => u.UserId == userId);
            }

            query = query.OrderByDescending(o => o.Id);


            if (page is null || page <1 )
            {
                page = 1;
            }

            int pageSize = 5;
            int totalPages = 0;

            decimal count = query.Count();
            totalPages = (int) Math.Ceiling(count / pageSize);

            query = query.Skip((int)(page - 1) * pageSize)
                .Take(pageSize);


           


            var orders = query.ToList();


            foreach (var order in orders)
            {
                foreach (var item in order.OrderItems)
                {
                    item.Order = null;
                }

                order.User.Password = "";
            }

            var response = new
            {
                Orders = orders,
                totalPages = totalPages,
                Page = page,
                PageSize = pageSize
            };

            return new ApiResponse
            {
                Result = response
            };
        }

        [Authorize]
        [HttpGet("{UniqueId}")]
        public async Task<ApiResponse> GetOrderByUniqueId(string uniqueId)
        {
            var userId = JwtReader.GetUserId(User);
            var role = _context.Users.Find(userId)?.Role ?? "";

            Order? order = null;
            if (role == "Admin")
            {
                order = await _context.Orders
                    .Include(u=>u.User)
                    .Include(oi=>oi.OrderItems)
                    .ThenInclude(p=>p.Product)
                    .FirstOrDefaultAsync(uid => uid.UniqueOrderId.ToLower() == uniqueId.ToLower());
            }
            else
            {
                order = await _context.Orders
                    .Include(u => u.User)
                    .Include(oi => oi.OrderItems)
                    .ThenInclude(p => p.Product)
                    .FirstOrDefaultAsync(uid => uid.UniqueOrderId.ToLower() == uniqueId.ToLower() && uid.UserId == userId);

            }

            if (order is null)
            {
                Response.StatusCode = (int)HttpStatusCode.NotFound;

                return new ApiResponse
                {
                    ErrorMessage = "Specific order cannot be found , kindly check the order id again"
                };
            }

            foreach (var item in order.OrderItems)
            {
                item.Order = null;
            }

            order.User.Password = "";

            return new ApiResponse
            {
                Result = order
            };
        }


        [Authorize]
        [HttpPost]
        public async Task<ApiResponse> CreateOrder(OrderDto orderDto)
        {
            if (!OrderHelper.PaymentMethods.ContainsKey(orderDto.PaymentMethod))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;

                return new ApiResponse
                {
                    ErrorMessage = "Input a valid payment method"
                };

            }

            int userId = JwtReader.GetUserId(User);
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                Response.StatusCode = (int)HttpStatusCode.NotFound;

                return new ApiResponse
                {
                    ErrorMessage = "Error finding user"
                };
            }
            var newOrderId = "ORD" +  GenerateUniqueAlphanumericId();

            while(await _context.Orders.AnyAsync(o=>o.UniqueOrderId == newOrderId)) 
            {
                newOrderId = "ORD" + GenerateUniqueAlphanumericId();
            }

            var productDictionary = OrderHelper.GetProductDictionary(orderDto.ProductIdentifiers);

            var order = new Order();
            order.UserId = userId;
            order.UniqueOrderId = newOrderId;
            order.CreatedAt = DateTime.Now;
            order.ShippingFee = OrderHelper.ShippingFee;
            order.DeliveryAddress = orderDto.DeliveryAddress;
            order.PaymentMethod = orderDto.PaymentMethod;
            
            order.PaymentStatus = OrderHelper.PaymentStatuses[0];
            order.OrderStatus = OrderHelper.OrderStatuses[0];

            foreach (var pair in productDictionary)
            {
                int productId = pair.Key;
                var product = await _context.Products.FindAsync(productId);
                if (product == null)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;

                    return new ApiResponse
                    {
                        ErrorMessage = "No product found, Product is Unavailable"
                    };

                }

                var orderItem = new OrderItem();
                orderItem.ProductId = productId;
                orderItem.Quantity = pair.Value;
                orderItem.UnitPrice = product.Price;

                order.OrderItems.Add(orderItem);


            }

            if (order.OrderItems.Count <1)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;

                return new ApiResponse
                {
                    ErrorMessage = "Unable to create order"
                };
            }
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            foreach (var item in order.OrderItems)
            {
                item.Order = null;
            }

            order.User.Password = "";

            return new ApiResponse
            {
                Result = order
            };
        }

        [Authorize(Roles ="Admin")]
        [HttpPut("{id}")]
        public async Task<ApiResponse> UpdateOrder  (int id , string? paymentStatus , string? orderStatus)
        {
            if (paymentStatus is null && orderStatus is null)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;

                return new ApiResponse
                {
                    ErrorMessage = "Kindly provide details in appropraite field"
                };

           
            }
            if (paymentStatus is not null && !OrderHelper.PaymentStatuses.Contains(paymentStatus))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;

                return new ApiResponse
                {
                    ErrorMessage = "Payment Status is not valid"
                };
            }

            if (orderStatus is not null && !OrderHelper.OrderStatuses.Contains(orderStatus))
            {
                Response.StatusCode = (int) HttpStatusCode.BadRequest;

                return new ApiResponse
                {
                    ErrorMessage = " The order status is not valid"
                };

            }

            var order = await _context.Orders.FindAsync(id);

            if (order is null)
            {
                Response.StatusCode = (int)HttpStatusCode.NotFound;

                return new ApiResponse
                {
                    ErrorMessage = "Order not found"
                };
            }

            if (paymentStatus is not null)
            {
                order.PaymentStatus = paymentStatus;
            }

            if (orderStatus is not null)
            {
                order.OrderStatus = orderStatus;
            }

            await _context.SaveChangesAsync();

            return new ApiResponse
            {
                Result = order
            };
        }

        [Authorize(Roles ="Admin")]
        [HttpDelete("{id}")]
        public async Task<ApiResponse> DeleteOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order is null)
            {
                Response.StatusCode = (int)HttpStatusCode.NotFound;

                return new ApiResponse
                {
                    ErrorMessage = "Order not found"
                };

            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return new ApiResponse
            {
                Result = "Order has been deleted successfully"
            };
        }
        

        private string GenerateUniqueAlphanumericId()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new  string(Enumerable.Repeat(chars , 5).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
