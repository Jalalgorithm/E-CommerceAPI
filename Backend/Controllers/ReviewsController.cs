using Backend.ApiModel.Base;
using Backend.ApiModel.Reviews;
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
    public class ReviewsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReviewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("AddReview/{productId}")]
        public async Task<ApiResponse> AddReview(int productId, [FromBody] ReviewAddDto reviewAdd)
        {
            var review = new Review()
            {
                Comment = reviewAdd.Comment,
                Email = reviewAdd.Email,
                ProductId = productId,
                Rating = reviewAdd.Rating
            };

            await _context.Reviews.AddAsync(review);
            await _context.SaveChangesAsync();

            return new ApiResponse
            {

            };
        }

        [HttpGet("GetReview/{productId}")]
        public async Task<ApiResponse> GetReviewForAProduct(int productId)
        {
            var reviews = await _context.Reviews.Where(r => r.ProductId == productId).ToListAsync();

            if (reviews == null)
            {
                Response.StatusCode = (int)HttpStatusCode.NotFound;

                return new ApiResponse
                {
                    ErrorMessage = "no review available for this product"
                };


            }

            return new ApiResponse
            {
                Result = reviews
            };
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("average-rating/{productId}")]
        public async Task<ApiResponse> GetAverageRatingForProduct(int productId)
        {
            var productReviews = await _context.Reviews.Where(r=>r.ProductId==productId).ToListAsync();

            if(productReviews == null || productReviews.Count <=0)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;

                return new ApiResponse
                {
                    ErrorMessage = "product review not found"
                };
            }

            double totalRating = (double)productReviews.Sum(r => r.Rating);
            double averageRating = totalRating / productReviews.Count;

            return new ApiResponse
            {
                Result = averageRating
            };

        }

    }
}
