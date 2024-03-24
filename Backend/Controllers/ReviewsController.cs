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
            string errorMessage = default;
            var result = string.Empty;

            try
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

                result = "Review Added";
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

        [HttpGet("average-rating/{productId}")]
        public async Task<ApiResponse> GetAverageRatingForProduct(int productId)
        {
            string errorMessage = default;
            var result = default(double);

            try
            {
                var productReviews = await _context.Reviews.Where(r => r.ProductId == productId).ToListAsync();

                if (productReviews == null || productReviews.Count <= 0)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;

                    return new ApiResponse
                    {
                        ErrorMessage = "product review not found"
                    };
                }

                double totalRating = (double)productReviews.Sum(r => r.Rating);
                double averageRating = totalRating / productReviews.Count;

                result = averageRating;
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
        [HttpDelete("DeleteReview/{id}")]
        public async Task<ApiResponse> DeleteReview (int id)
        {
            string errorMessage = default;

            var result = default(string);

            try
            {
                var productReview = await _context.Reviews.FindAsync(id);

                if (productReview is null)
                {
                    Response.StatusCode = (int)HttpStatusCode.NotFound;

                    return new ApiResponse
                    {
                        ErrorMessage = "This review doesnt not exist for this program"
                    };
                }

                _context.Reviews.Remove(productReview);
                await _context.SaveChangesAsync();

                result = "Review Deleted";
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
