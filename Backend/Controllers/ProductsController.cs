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
        public async Task<IActionResult> GetProducts(string? search ,string? category , int? minPrice , int? maxPrice ,
            string? sort , string? order, int? page)
        {
            IQueryable<Product> query =  _context.Products;

            if (search is not null)
            {
                query = query.Where(p=>p.Name.Contains(search)|| p.Description.Contains(search));
            }

            if (category is not null)
            {
                query = query.Where(p=>p.Category.Name.ToLower() == category.ToLower());
            }

            if (minPrice is not null)
            {
                query = query.Where(p => p.Price >= minPrice);
            }

            if (maxPrice is not null)
            {
                query= query.Where(p=>p.Price <= maxPrice);
            }

            if (sort is null)
                sort = "id";

            if (order is null || order != "asc")
                order = "desc";

            if (sort.ToLower()=="name")
            {
                if (order=="asc")
                {
                    query = query.OrderBy(p => p.Name);
                }
                else
                {
                    query = query.OrderByDescending(p => p.Name);
                }
            }

            else if (sort.ToLower() == "brand")
            {
                if (order == "asc")
                {
                    query = query.OrderBy(p => p.Brand);
                }
                else
                {
                    query = query.OrderByDescending(p => p.Brand);
                }
            }
            else if (sort.ToLower() == "category")
            {
                if (order == "asc")
                {
                    query = query.OrderBy(p => p.Category.Name);
                }
                else
                {
                    query = query.OrderByDescending(p => p.Category.Name);
                }
            }
            else if (sort.ToLower() == "price")
            {
                if (order == "asc")
                {
                    query = query.OrderBy(p => p.Price);
                }
                else
                {
                    query = query.OrderByDescending(p => p.Price);
                }
            }

            else if (sort.ToLower() == "date")
            {
                if (order == "asc")
                {
                    query = query.OrderBy(p => p.CreatedAt);
                }
                else
                {
                    query = query.OrderByDescending(p => p.CreatedAt);
                }
            }
            else
            {
                if (order == "asc")
                {
                    query = query.OrderBy(p => p.Id);
                }
                else
                {
                    query = query.OrderByDescending(p => p.Id);
                }
            }

            if (page is null || page < 1)
                page = 1;

            int pageSize = 5;
            int totalPages = 0;

            decimal count = query.Count();
            totalPages = (int)Math.Ceiling(count/pageSize);

            query = query.Skip((int)(page - 1) * pageSize).Take(pageSize);


            var products = await query.ToListAsync();


            var response = new
            {
                Products = products,
                TotalPages = totalPages,
                PageSize = pageSize,
                Page = page,
            };


            return Ok(response);
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
