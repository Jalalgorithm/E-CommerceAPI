using AutoMapper;
using Backend.ApiModel.Category;
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

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories.ToListAsync();


            return Ok(_mapper.Map<ICollection<GetCategoryDto>>(categories));
        }
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _context.Products.ToListAsync();
                //.Include(c => c.Category)
                //.Select(product => new GetProductDto
                //{
                //    Id=product.Id,
                //    Name = product.Name,


                //}).ToListAsync();

            if (products is null)
                return NotFound();

            return Ok(products);
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
        public async Task<IActionResult> CreateProduct ([FromForm]CreateProductDto createProductDto)
        {
            if(createProductDto.Image == null)
            {
                ModelState.AddModelError("Image", "The Image file is required");
                return BadRequest(ModelState);
            }

            var fileName = DateTime.Now.ToString("yyyyMMddHHmmssffff");
            fileName += Path.GetExtension(createProductDto.Image.FileName);

            string imagesFolder = _environment.WebRootPath + "/images/products/";

            using(var stream =System.IO.File.Create(imagesFolder+fileName))
            {
                await createProductDto.Image.CopyToAsync(stream);
            }

            var product = new Product()
            {
                Name = createProductDto.Name,
                Description = createProductDto.Description ?? "",
                Brand = createProductDto.Brand,
                DisplayImage = fileName,
                Price = createProductDto.Price,
                QuantityInStock = createProductDto.QuantityInStock,
                CategoryId= createProductDto.CategoryId,
            };
            _context.Products.Add(product);
            _context.SaveChanges();

            return Ok("Product Successfully uploaded");


        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id  , [FromForm]CreateProductDto createProductDto)
        {
            var product = _context.Products.Find(id);

            if (product is null)
                return NotFound();

            string fileName = product.DisplayImage;

            if (createProductDto.Image !=null)
            {
                fileName = DateTime.Now.ToString("yyyyMMddHHmmssffff");
                fileName += Path.GetExtension(createProductDto.Image.FileName);

                string imagesFolder = _environment.WebRootPath + "/images/products/";

                using (var stream = System.IO.File.Create(imagesFolder + fileName))
                {
                    await createProductDto.Image.CopyToAsync(stream);
                }


                System.IO.File.Delete(imagesFolder + product.DisplayImage);
            }

            product.Name = createProductDto.Name;
            product.Description = createProductDto.Description;
            product.QuantityInStock = createProductDto.QuantityInStock;
            product.Price = createProductDto.Price;
            product.Brand = createProductDto.Brand;
            product.DisplayImage = fileName;
            product.CategoryId = createProductDto.CategoryId;

            _context.SaveChanges();

            return Ok(product);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product ==null)
            {
                return NotFound();
            }

            string imagesFolder = _environment.WebRootPath + "/images/products/";
            System.IO.File.Delete(imagesFolder + product.DisplayImage);

            _context.Products.Remove(product);
            _context.SaveChanges();

            return Ok("Product has been deleted successfully");

        }
    }
}
