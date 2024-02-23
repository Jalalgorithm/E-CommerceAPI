using Backend.ApiModel.Base;
using Backend.ApiModel.ThumbnailImages;
using Backend.Data;
using Backend.Model;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ThumbnailController : ControllerBase
    {
        private readonly ImageHandler _imageHandler;
        private readonly ApplicationDbContext _context;

        public ThumbnailController(ImageHandler imageHandler , ApplicationDbContext context)
        {
            _imageHandler = imageHandler;
            _context = context;
        }

        [Authorize(Roles ="Admin")]
        [HttpPost("Add-thumbnail")]
        public async Task<ApiResponse> AddImage(List<IFormFile> pictures)
        {
            if (pictures.Count<1)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;

                return new ApiResponse
                {
                    ErrorMessage = "Kindly add an image"
                };


            }

            var imagesPath = _imageHandler.UploadManyImages(pictures);


            foreach (var image in imagesPath)
            {
                var imageUrl = new ThumbnailPicture();
                imageUrl.Path = image;
                await _context.ThumbnailPictures.AddAsync(imageUrl);
                await _context.SaveChangesAsync();
            }




            return new ApiResponse
            {
                Result = "All images successfully uploaded"
            };
        }

        [HttpGet("AllImages")]
        public async Task<ApiResponse> GetImages ()
        {
            var images =  _context.ThumbnailPictures
                .Select(image => new GetThumbnailDto
                {
                    Path = image.Path,
                }).ToList();

            if (images.Count < 1)
            {
                Response.StatusCode = (int)HttpStatusCode.NotFound;

                return new ApiResponse
                {
                    ErrorMessage = "no image found"
                };
            }

            return new ApiResponse
            {
                Result = images
            };
        }
    }
}
