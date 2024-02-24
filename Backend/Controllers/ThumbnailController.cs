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
            string errorMessage = default;
            var result = default(string);

            try
            {
                if (pictures.Count < 1)
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

                result = "Image upload successful";
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

        [HttpGet("AllImages")]
        public async Task<ApiResponse> GetImages ()
        {
            string errorMessage = default;

            var result = default(List<GetThumbnailDto>);

            try
            {
                var images = _context.ThumbnailPictures
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

                result = images;
            }
            catch (Exception ex)
            {
                Response.StatusCode= (int)HttpStatusCode.InternalServerError;
                errorMessage = ex.Message;
            }
           
            return new ApiResponse
            {
                Result = result,
                ErrorMessage = errorMessage
            };
        }

        [Authorize(Roles ="Admin")]
        [HttpPut("Update-Thumbnail")]
        public async Task<ApiResponse> UpdateThumbnail (List<IFormFile> welcomePictures)
        {
            string errorMessage = default;
            var result = default(string);

            try
            {
                var images = _context.ThumbnailPictures.ToList();

                foreach (var image in images)
                {
                    _imageHandler.DeleteAnImage(image.Path);
                }

                _context.ThumbnailPictures.RemoveRange(images);

                var newImagesUrl = _imageHandler.UploadManyImages(welcomePictures);

                foreach (var image in newImagesUrl)
                {
                    var picture = new ThumbnailPicture();
                    picture.Path = image;
                    await _context.ThumbnailPictures.AddAsync(picture);

                }

                await _context.SaveChangesAsync();

                result = "Pictures updated successfully";
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
        [HttpDelete("Delete-Thumbnail")]
        public async Task<ApiResponse> DeleteThumbnail()
        {
            string errorMessage = default;
            var result = default(string);

            try
            {
                var pictures = _context.ThumbnailPictures.ToList();

                foreach (var image in pictures)
                {
                    _imageHandler.DeleteAnImage(image.Path);
                }

                _context.ThumbnailPictures.RemoveRange(pictures);
                await _context.SaveChangesAsync();

                result = "Thumbnails deleted";
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
