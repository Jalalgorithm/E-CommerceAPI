using Backend.ApiModel.Base;
using Backend.ApiModel.OrderDtoModel;
using Backend.Data;
using Backend.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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

        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public async Task<ApiResponse> GetOrders (int page)
        {
            string errorMessage = default;

            var result = default(Pagination);

            try
            {
                if (page == 0 || page < 1)
                {
                    page = 1;
                }

                int pageSize = 5;
                int totalPages = 0;

                var TotalItems = _context.Orders.Count();
                totalPages = (int)Math.Ceiling((double)TotalItems/ pageSize);

                var orders = _context.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .OrderByDescending(o => o.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(order => new OrderViewDto
                    {
                        FullName = order.Name,
                        Id = order.Id,
                        UniqueId = order.UniqueOrderId,
                        PaymentStatus = order.PaymentStatus,
                        DeliveryStatus = order.OrderStatus,
                        DateCreated = order.CreatedAt,
                        TotalAmount = order.OrderItems.Sum(oi => oi.Quantity * oi.Product.Price),
                    }).ToList();

                if (orders is null || orders.Count == 0)
                {
                    Response.StatusCode = (int)HttpStatusCode.NotFound;

                    return new ApiResponse
                    {
                        ErrorMessage = "No order exist"
                    };
                }

                var response = new Pagination
                {
                    Data = orders,
                    TotalPages = totalPages,
                    Page = page,
                    PageSize = pageSize
                };

                result = response;
            }
            catch (Exception ex)
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorMessage = ex.Message;
            }
            return new ApiResponse
            {
                Result = result,
                ErrorMessage = errorMessage
            };
        }


        [Authorize]
        [HttpGet("user")]
        public async Task<ApiResponse> GetOrdersById()
        {
            var userId = JwtReader.GetUserId(User);
            string errorMessage = default;

            var result = default(List<OrderViewDto>);

            try
            {

                var orders = _context.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .OrderByDescending(o => o.CreatedAt)
                    .Where(o=>o.UserId==userId)
                    .Select(order => new OrderViewDto
                    {
                        FullName = order.Name,
                        Id = order.Id,
                        UniqueId = order.UniqueOrderId,
                        PaymentStatus = order.PaymentStatus,
                        DeliveryStatus = order.OrderStatus,
                        DateCreated = order.CreatedAt,
                        TotalAmount = order.OrderItems.Sum(oi => oi.Quantity * oi.Product.Price),
                    }).ToList();

                if (orders is null || orders.Count==0)
                {
                    Response.StatusCode = (int)HttpStatusCode.NotFound;

                    return new ApiResponse
                    {
                        ErrorMessage = "No order exist"
                    };
                }

                result = orders;
            }
            catch (Exception ex)
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorMessage = ex.Message;
            }
            return new ApiResponse
            {
                Result = result,
                ErrorMessage = errorMessage
            };
        }


        [Authorize(Roles ="Admin")]
        [HttpGet("{uniqueId}")]
        public async Task<ApiResponse> GetOrderByUniqueId(string uniqueId)
        {
            string errorMessage = default;

            var result = default(List<OrderViewDto>);

            try
            {

                if (string.IsNullOrEmpty(uniqueId))
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;

                    return new ApiResponse
                    {
                        ErrorMessage = "Kindly input a value"
                    };
                }
                uniqueId.ToUpper();

                var order = await _context.Orders
                     .Include(oi => oi.OrderItems)
                     .ThenInclude(p => p.Product)
                     .Where(o => o.UniqueOrderId == uniqueId)
                     .Select(order => new OrderViewDto
                     {
                         FullName = order.Name,
                         Id = order.Id,
                         UniqueId = order.UniqueOrderId,
                         PaymentStatus = order.PaymentStatus,
                         DeliveryStatus = order.OrderStatus,
                         DateCreated = order.CreatedAt,
                         TotalAmount = order.OrderItems.Sum(oi => oi.Quantity * oi.Product.Price),
                     }).ToListAsync();

                if (order is null || order.Count==0 )
                {
                    Response.StatusCode = (int)HttpStatusCode.NotFound;

                    return new ApiResponse
                    {
                        ErrorMessage = "Specific order cannot be found , kindly check the order id again"
                    };
                }

                result = order;
            }
            catch (Exception ex)
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorMessage = ex.Message;
            }

            return new ApiResponse
            {
                Result = result,
                ErrorMessage = errorMessage
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
            var newOrderId =  GenerateUniqueAlphanumericId();

            while(await _context.Orders.AnyAsync(o=>o.UniqueOrderId == newOrderId)) 
            {
                newOrderId =  GenerateUniqueAlphanumericId();
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

        [Authorize]
        [HttpPost("process/{orderUniqueId}")]
        public async Task<ApiResponse> ProcessOrderPayment(string orderUniqueId)
        {

            var userId = JwtReader.GetUserId(User);
            if (userId <= 0)
            {
                Response.StatusCode = (int)HttpStatusCode.NotFound;

                return new ApiResponse
                {
                    ErrorMessage = "kindly log in"
                };

            }

            //Order? order = null;
            var order = await _context.Orders
                    .Include(oi => oi.OrderItems)
                    .ThenInclude(p => p.Product)
                    .FirstOrDefaultAsync(uid => uid.UniqueOrderId.ToLower() == orderUniqueId.ToLower() && uid.UserId == userId);

            if (order is null)
            {
                Response.StatusCode = (int)HttpStatusCode.NotFound;

                return new ApiResponse
                {
                    ErrorMessage = "no order found"
                };

            }

           
           // var orderItems = _context.OrderItems.Where(oi=>oi.OrderId == order.Id).ToList();

            decimal totalPrice =  order.OrderItems.Sum(item => item.UnitPrice * item.Quantity);

            if (order.PaymentStatus.ToLower() == "accepted")
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;

                return new ApiResponse
                {
                    ErrorMessage = "order has already been paid"
                };
            }

            foreach (var item in order.OrderItems)
            {

                if (item.Product.QuantityInStock < item.Quantity)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;

                    return new ApiResponse
                    {
                        ErrorMessage = "Not enough quantity available for product"
                    };
                }
            }
            



                //process payment here


                order.PaymentStatus = OrderHelper.PaymentStatuses[1];


            foreach (var item in order.OrderItems)
            {
                item.Product.QuantityInStock -=item.Quantity;
                _context.Entry(item).State = EntityState.Modified; 
            }


            await _context.SaveChangesAsync();

            return new ApiResponse
            {
                Result = "Payment Successful"
            };
        }

        

        private string GenerateUniqueAlphanumericId()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new  string(Enumerable.Repeat(chars , 9).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
