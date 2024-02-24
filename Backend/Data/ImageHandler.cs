using Backend.ApiModel.Products;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace Backend.Data
{
    public class ImageHandler
    {

        private readonly IConfiguration _configuration;
        private CloudinarySettings _cloudinarySettings;
        private Cloudinary _cloudinary;

        public ImageHandler(IConfiguration configuration)
        {
            _configuration = configuration;
            _cloudinarySettings = configuration.GetSection("CloudinarySettings").Get<CloudinarySettings>()!;
            Account account = new Account()
            {
                ApiKey = _cloudinarySettings.ApiKey,
                Cloud = _cloudinarySettings.CloudName,
                ApiSecret = _cloudinarySettings.ApiSecret,
            };
            _cloudinary = new Cloudinary(account);
        }

        public string UploadImage (IFormFile displayImage)
        {
            var file = displayImage;

            var uploadResult = new ImageUploadResult();

            if(file.Length<=0)
            {

            }

            using(var stream  = file.OpenReadStream())
            {
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(file.Name , stream),
                    UseFilename = true,
                };
                uploadResult = _cloudinary.Upload(uploadParams);
            }

            var result = uploadResult.Url.ToString();
            if (result is null)
            {
                return "";
            }

            return result;
        }

        public List<string> UploadManyImages (List<IFormFile> ProductsImage)
        {
            var file = ProductsImage;

            var uploadResult = new ImageUploadResult();

            var result = new List<string>();   
            

            foreach (var image in ProductsImage)
            {
                using (var stream = image.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(image.Name, stream),
                        UseFilename = true,
                    };

                    uploadResult =_cloudinary.Upload(uploadParams);

                }
                result.Add(uploadResult.Url.ToString());
            }

            var filteredList = result.Where(str => !string.IsNullOrEmpty(str.Trim())).ToList();

            return filteredList;
        }

        public bool DeleteAnImage (string FileName)
        {
            var publicId = GetPublicIdFromImageUrl(FileName);

            if (publicId is null)
            {
                return false;
            }

            var deleteFile = new DeletionParams(publicId)
            {
                ResourceType = ResourceType.Image
            };

            var deleteResult = _cloudinary.Destroy(deleteFile);

            if (deleteResult.Result.ToLower() == "ok")
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public bool DeleteManyImages (List<string> filenames)
        {
            if (filenames.Count <1)
            {

            }

            var result= false;

            foreach (var filename in filenames)
            {
                var deleteFile = new DeletionParams(filename);

                var deleteResult = _cloudinary.Destroy(deleteFile);

                if (deleteResult.Result.ToLower() == "ok")
                {
                    result = true;
                }
                else
                {
                    result = false;
                }
            }

           return result;
        }

        private string GetPublicIdFromImageUrl(string imageUrl)
        {
            var url = new Uri(imageUrl);

            return Path.GetFileNameWithoutExtension(url.Segments.Last());
        }
        
    }
}
