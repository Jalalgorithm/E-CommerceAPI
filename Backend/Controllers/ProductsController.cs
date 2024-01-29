using AutoMapper;
using Backend.ApiModel.Base;
using Backend.ApiModel.Category;
using Backend.ApiModel.Product;
using Backend.Data;
using Backend.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ProductsController(ApplicationDbContext context , IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpGet("categories")]
        public async Task<ApiResponse> GetCategories()
        {
            string errorMessage = default;

            var result = default(ICollection<GetCategoryDto>);

            try
            {
                var categories = await _context.Categories
                    .Select( category => new GetCategoryDto
                    {
                        Id= category.Id,
                        Name= category.Name,
                    })
                    .ToListAsync();

                if ( categories is null)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;

                    return new ApiResponse
                    {
                        ErrorMessage = "categories missing",
                    };
                }

                result = categories;
            }
            catch (Exception ex)
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorMessage = ex.Message;
            }

            return new ApiResponse
            {
                ErrorMessage = errorMessage,
                Result = result,
            };
           


        }
        [HttpGet]
        public async Task<ApiResponse> GetProducts(string? search ,string? category , int? minPrice , int? maxPrice ,
            string? sort , string? order, int? page)
        {

            string errorMessage = default;

            var result = default(Pagination);

            try
            {
                IQueryable<Product> query = _context.Products.Include(c => c.Category);

                if (query is null)
                {
                    Response.StatusCode = (int) HttpStatusCode.BadRequest;

                    return new ApiResponse
                    {
                        ErrorMessage = "Incorrect data inquiry",
                    };
                }

                if (search is not null)
                {
                    query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search));
                }

                if (category is not null)
                {
                    query = query.Where(p => p.Category.Name.ToLower() == category.ToLower());
                }

                if (minPrice is not null)
                {
                    query = query.Where(p => p.Price >= minPrice);
                }

                if (maxPrice is not null)
                {
                    query = query.Where(p => p.Price <= maxPrice);
                }

                if (sort is null)
                    sort = "id";

                if (order is null || order != "asc")
                    order = "desc";

                if (sort.ToLower() == "name")
                {
                    if (order == "asc")
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
                totalPages = (int)Math.Ceiling(count / pageSize);

                query = query.Skip((int)(page - 1) * pageSize).Take(pageSize);


                var products = await query
                    .Select(product => new GetProductDto
                    {
                        Id = product.Id,
                        Name = product.Name,
                        Description = product.Description,
                        QuantityInStock = product.QuantityInStock,
                        DisplayImage = product.DisplayImage,
                        Brand = product.Brand,
                        Price = product.Price,
                        CategoryName = product.Category.Name
                    }).ToListAsync();


                var response = new Pagination
                {
                    Data = products,
                    TotalPages = totalPages,
                    PageSize = pageSize,
                    Page = page,
                };

                result = response;
            }
            catch (Exception ex)
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorMessage = ex.Message;
            }

            return new ApiResponse
            {
                ErrorMessage = errorMessage,
                Result = result
            };
        }

        [HttpGet("{id}")]
        public async Task<ApiResponse> GetProduct(int id)
        {
            string errorMessage = default;

            var result = default(GetProductDto);

            if (id <= 0 )
            {
                Response.StatusCode = (int) HttpStatusCode.BadRequest;

                return new ApiResponse
                {
                    ErrorMessage = "A valid id is required"
                };
            }

            try
            {

                var product = await _context.Products
                    .Include(c => c.Category)
                    .Select(product => new GetProductDto
                    {
                        Id = product.Id,
                        Name = product.Name,
                        Description = product.Description,
                        Price = product.Price,
                        CategoryName = product.Category.Name,
                        QuantityInStock = product.QuantityInStock,
                        DisplayImage = product.DisplayImage,
                        Brand = product.Brand
                    }).FirstOrDefaultAsync(x => x.Id == id);

                if(product is null)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;

                    return new ApiResponse
                    {
                        ErrorMessage = "Specific product cant be found",
                    };
                }

                result = product;

            }
            catch (Exception ex)
            {
                Response.StatusCode =(int) HttpStatusCode.InternalServerError;

                errorMessage = ex.Message;
            }

            return new ApiResponse
            {
                Result = result,
                ErrorMessage = errorMessage
            };

            
        }
        [Authorize(Roles ="Admin")]
        [HttpPost]
        public async Task<ApiResponse> CreateProduct ([FromForm]CreateProductDto createProductDto)
        {
            string errorMessage = default;
            
            var result = default(string);


            if(createProductDto.Image is null)
            {
                Response.StatusCode =(int) HttpStatusCode.BadRequest;

                return new ApiResponse
                {
                    ErrorMessage = "No image found",
                };
            }

            try
            {
                var fileName = DateTime.Now.ToString("yyyyMMddHHmmssffff");
                fileName += Path.GetExtension(createProductDto.Image.FileName);

                string imagesFolder = _environment.WebRootPath + "/images/products/";

                using (var stream = System.IO.File.Create(imagesFolder + fileName))
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
                    CategoryId = createProductDto.CategoryId,
                };
                _context.Products.Add(product);
                _context.SaveChanges();

                result = "Product has been created successfully";
            }
            catch (Exception ex)
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                errorMessage = ex.Message;
            }

            return new ApiResponse
            {
                ErrorMessage = errorMessage,
                Result = result,
            };

        }
        [Authorize(Roles ="Admin")]
        [HttpPut("{id}")]
        public async Task<ApiResponse> UpdateProduct (int id  , [FromForm]CreateProductDto createProductDto)
        {
            string errorMessage = default;
            var result = default(string);

            if (id <=0)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;

                return new ApiResponse
                {
                    ErrorMessage = "Invalid id is passed",
                };
            }

            try
            {
                var product = _context.Products.Find(id);

                if (product is null)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;

                    return new ApiResponse
                    {
                        ErrorMessage = "This product does not exist",
                    };
                }

                string fileName = product.DisplayImage;

                if (createProductDto.Image != null)
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
                product.Description = createProductDto.Description ?? "";
                product.QuantityInStock = createProductDto.QuantityInStock;
                product.Price = createProductDto.Price;
                product.Brand = createProductDto.Brand;
                product.DisplayImage = fileName;
                product.CategoryId = createProductDto.CategoryId;

                _context.SaveChanges();

                result = "Product has been successfully updated";

            }
            catch (Exception ex)
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorMessage = ex.Message;
            }
            return new ApiResponse
            {
                ErrorMessage = errorMessage,
                Result = result
            };
        }
        [Authorize(Roles ="Admin")]
        [HttpDelete("{id}")]
        public async Task<ApiResponse> DeleteProduct(int id)
        {
            string errorMessage = default;
            var result = default(string);

            if(id<=0)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;

                return new ApiResponse
                {
                    ErrorMessage = "Invalid id",
                };
            }

            try
            {

                var product = await _context.Products.FindAsync(id);

                if (product == null)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;

                    return new ApiResponse
                    {
                        ErrorMessage = "product doesnt exist",
                    };
                }

                string imagesFolder = _environment.WebRootPath + "/images/products/";
                System.IO.File.Delete(imagesFolder + product.DisplayImage);

                _context.Products.Remove(product);
                _context.SaveChanges();

                result = "Product deleted successfully";

            }
            catch (Exception ex)
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorMessage = ex.Message;
            }

            return new ApiResponse
            {
                ErrorMessage = errorMessage,
                Result = result,
            };

        }
    }
}
