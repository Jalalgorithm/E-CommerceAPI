using AutoMapper;
using Backend.ApiModel.Category;
using Backend.ApiModel.Product;
using Backend.Model;

namespace Backend.Data
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Product , GetProductDto>()
                .ForMember(dest=>dest.CategoryName , opt=>opt.MapFrom(src=>src.Category.Name));

            CreateMap<Category, GetCategoryDto>();
            CreateMap<CreateProductDto, Product>()
                .ForMember(dest=>dest.DisplayImage , opt=>opt.MapFrom(src=>ConvertIFormFileToString(src.Image)));

        }

        private string ConvertIFormFileToString (IFormFile file)
        {
            if (file == null)
                return string.Empty;

            using (var memoryStream = new MemoryStream())
            {
                file.CopyTo(memoryStream);
                byte [] bytes = memoryStream.ToArray();
                return Convert.ToBase64String(bytes);

            }
        }
    }
}
