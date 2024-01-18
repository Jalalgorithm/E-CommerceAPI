using AutoMapper;
using Backend.ApiModel.Product;
using Backend.Model;

namespace Backend.Data
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Product , GetProductDto>();
            CreateMap<CreateProductDto, Product>();

        }
    }
}
