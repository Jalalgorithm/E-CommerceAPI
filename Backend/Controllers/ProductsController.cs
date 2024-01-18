using AutoMapper;
using Backend.ApiModel.Product;
using Backend.Data;
using Backend.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ProductsController(ApplicationDbContext context , IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _context.Products.ToListAsync();

            if (products is null)
                return NotFound();

            return Ok(_mapper.Map<ICollection<GetProductDto>>(products));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product is null)
                return NotFound();

            return Ok(_mapper.Map<Product>(product));
        }
        [HttpPost]
        public async Task<IActionResult> CreateProduct (CreateProductDto createProductDto)
        {
            var AddProdcut = _mapper.Map<Product>(createProductDto);

            return Ok(_mapper.Map<CreateProductDto>(AddProdcut));


        }
    }
}
