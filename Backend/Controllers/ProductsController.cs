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
        private readonly IWebHostEnvironment _environment;

        public ProductsController(ApplicationDbContext context , IMapper mapper , IWebHostEnvironment environment)
        {
            _context = context;
            _mapper = mapper;
            _environment = environment;
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
            if(createProductDto.Image == null)
            {
                ModelState.AddModelError("Image", "The Image file is required");
                return BadRequest(ModelState);
            }

            var fileName = DateTime.Now.ToString("yyyyMMddHHmmssffff");
            fileName += Path.GetExtension(createProductDto.Image.FileName);

            string imagesFolder = _environment.WebRootPath + "/images/products";

            using(var stream =System.IO.File.Create(imagesFolder+fileName))
            {
                await createProductDto.Image.CopyToAsync(stream);
            }

            var AddProduct = _mapper.Map<Product>(createProductDto);

            await _context.Products.AddAsync(AddProduct);
            await _context.SaveChangesAsync();

            return Ok(_mapper.Map<CreateProductDto>(AddProduct));


        }
    }
}
