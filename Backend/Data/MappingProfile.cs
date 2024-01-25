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
           

        }

      
    }
}
