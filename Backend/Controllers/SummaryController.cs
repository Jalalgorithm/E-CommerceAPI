using Backend.ApiModel.Base;
using Backend.ApiModel.OrderDtoModel;
using Backend.Data;
using Backend.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SummaryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SummaryController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ApiResponse> GetSummary ()
        {
            string errorMessage = default;
            var result = default(SummaryModel);
            try
            {
                var totalSales = await _context.Orders.SelectMany(o => o.OrderItems).SumAsync(oi => oi.Quantity * oi.UnitPrice);

                var totalProducts = await _context.Products.CountAsync();
                var totalOrders = await _context.Orders.CountAsync();

                var summary = new SummaryModel()
                {
                    TotalSales = totalSales,
                    TotalProducts = totalProducts,
                    TotalOrders = totalOrders
                };

                result = summary;

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

        [HttpGet("GetLatestOrders")]
        public async Task<ApiResponse> GetLatestOrders()
        {
            string errorMessage = default;
            var result = default(List<LatestOrderDto>);

            try
            {
                var latestOrders = await _context.Orders.OrderByDescending(o => o.CreatedAt)
                .Include(u => u.User)
                .Take(6)
                .Select(order => new LatestOrderDto
                {
                    CreatedAt = order.CreatedAt,
                    FirstName = order.User.FirstName + " " + order.User.LastName,
                    UniqueId = order.UniqueOrderId,
                    Email = order.User.Email
                })
                .ToListAsync();

                result = latestOrders;
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
    }
}
