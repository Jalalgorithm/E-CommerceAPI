using AutoMapper;
using Backend.ApiModel.Base;
using Backend.ApiModel.Category;
using Backend.ApiModel.Products;
using Backend.ApiModel.Reviews;
using Backend.Data;
using Backend.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics.Eventing.Reader;
using System.Net;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ImageHandler _imageHandler;

        public ProductsController(ApplicationDbContext context  , ImageHandler imageHandler)
        {
            _context = context;
            _imageHandler = imageHandler;
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

        [HttpGet("GetProductByCategoriesId/{categoryId}")]
        public async Task<ApiResponse> GetProductByCategoryId (int categoryId)
        {

            string errorMessage = default;

            var result = default(ICollection<GetProductDtoList>);

            try
            {
                var product = await _context.Products
               .Include(c => c.Category)
               .Where(p => p.CategoryId == categoryId)
               .Select(mainProduct => new GetProductDtoList
               {
                   Id = mainProduct.Id,
                   Name = mainProduct.Name,
                   Description = mainProduct.Description,
                   Brand = mainProduct.Brand,
                   Price = mainProduct.Price,
                   DisplayImage = mainProduct.DisplayImage,
                   CategoryName = mainProduct.Category.Name,
                   QuantityInStock = mainProduct.QuantityInStock,
               })
               .ToListAsync();


                if (product is null)
                {
                    Response.StatusCode = (int)HttpStatusCode.NotFound;

                    return new ApiResponse
                    {
                        ErrorMessage = "No product exist in current category"
                    };
                }

                result = product;

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

        [HttpGet]
        public async Task<ApiResponse> GetProducts(string? search ,string? category , int? categoryId ,  int? minPrice , int? maxPrice ,
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
                if(categoryId is not null)
                {
                    query = query.Where(p => p.CategoryId == categoryId);
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

                int pageSize = 10;
                int totalPages = 0;

                decimal count = query.Count();
                totalPages = (int)Math.Ceiling(count / pageSize);

                query = query.Skip((int)(page - 1) * pageSize).Take(pageSize);


                var products = await query
                    .Select(product => new GetProductDtoList
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
                    .Include(i => i.Otherimages)
                    .Include(r=>r.Reviews)
                    .Select(product => new GetProductDto
                    {
                        Id = product.Id,
                        Name = product.Name,
                        Description = product.Description,
                        Price = product.Price,
                        CategoryName = product.Category.Name,
                        QuantityInStock = product.QuantityInStock,
                        DisplayImage = product.DisplayImage,
                        Brand = product.Brand,
                        Images = product.Otherimages.Select(image => image.FilePath).ToList(),
                        Reviews = product.Reviews.Select(review => new ReviewAddDto
                        {
                            Comment = review.Comment,
                            Email = review.Email,
                            Rating = review.Rating
                        }).ToList(),
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
        [HttpPost("Create")]
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
                var imageurl = _imageHandler.UploadImage(createProductDto.Image);
                var otherImagesUrl = _imageHandler.UploadManyImages(createProductDto.OtherImages);

                if(string.IsNullOrEmpty(imageurl))
                {
                    return new ApiResponse
                    {
                        ErrorMessage = "Couldnt upload image"
                    };
                }

                var prodImages = new List<ProductImage>();

                foreach (var image in otherImagesUrl)
                {
                    var imageUrl = new ProductImage();
                    imageUrl.FilePath = image;
                    prodImages.Add(imageUrl);
                }

                var product = new Product()
                {
                    Name = createProductDto.Name,
                    Description = createProductDto.Description ?? "",
                    Brand = createProductDto.Brand,
                    DisplayImage = imageurl,
                    Price = createProductDto.Price,
                    QuantityInStock = createProductDto.QuantityInStock,
                    CategoryId = createProductDto.CategoryId,
                    Otherimages = prodImages.ToList(),
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
        [Authorize(Roles = "Admin")]
        [HttpPut("Update/{id}")]
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
                var product = await _context.Products
                    .Include(po => po.Otherimages)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product is null)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;

                    return new ApiResponse
                    {
                        ErrorMessage = "This product does not exist",
                    };
                }

                string fileName = product.DisplayImage;
                var response  = _imageHandler.DeleteAnImage(fileName);

                if (!response)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;

                    return new ApiResponse
                    {
                        ErrorMessage = "Couldnt delete image"
                    };
                }

                var newFile = _imageHandler.UploadImage(createProductDto.Image);

                if (string.IsNullOrEmpty(newFile))
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;

                    return new ApiResponse
                    {
                        ErrorMessage = "Couldnt upload image"
                    };
                }



                product.Name = createProductDto.Name;
                product.Description = createProductDto.Description ?? "";
                product.QuantityInStock = createProductDto.QuantityInStock;
                product.Price = createProductDto.Price;
                product.Brand = createProductDto.Brand;
                product.DisplayImage = newFile;
                product.CategoryId = createProductDto.CategoryId;

                var oldImages = product.Otherimages.ToList();


                foreach ( var image in oldImages )
                {
                    _imageHandler.DeleteAnImage(image.FilePath);
                }

                _context.ProductImages.RemoveRange(product.Otherimages);

                var newImages = _imageHandler.UploadManyImages(createProductDto.OtherImages);

                var productImages = new List<ProductImage>();

                foreach( var image in newImages )
                {
                    var imageUrls = new ProductImage();
                    imageUrls.FilePath = image;
                    productImages.Add(imageUrls);
                }



                product.Otherimages = productImages;
                  
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
        [HttpDelete("Delete/{id}")]
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

                var product = await _context.Products
                    .Include(p=>p.Otherimages)
                    .Include(r=>r.Reviews)
                    .FirstOrDefaultAsync(x=>x.Id  == id);

                if (product == null)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;

                    return new ApiResponse
                    {
                        ErrorMessage = "product doesnt exist",
                    };
                }

                var imagesinDb = product.Otherimages.ToList();
                var response = _imageHandler.DeleteAnImage(product.DisplayImage);

                if (response == false)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;

                    return new ApiResponse
                    {
                        ErrorMessage = "image couldnt be deleted"
                    };
                }

                foreach (var image in imagesinDb)
                {
                    _imageHandler.DeleteAnImage(image.FilePath);
                }



                _context.ProductImages.RemoveRange(product.Otherimages);
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

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
