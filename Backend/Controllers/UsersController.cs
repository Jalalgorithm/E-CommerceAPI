using Backend.ApiModel.Base;
using Backend.ApiModel.User;
using Backend.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Backend.Controllers
{
    [Authorize(Roles ="Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ApiResponse> GetUsers (int? page)
        {
            string errorMessage = default;

            var result = default(Pagination);

            if (page is null || page <1 )
            {
                page = 1;
            }

            int pageSize = 5;
            int totalPages = 0;

            decimal count = await _context.Users.CountAsync();
            totalPages = (int)Math.Ceiling(count / pageSize);

            try
            {
                var users = await _context.Users
                .Select(user => new UserProfileDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Phone = user.Phone,
                    Address = user.Address,
                    Role = user.Role,
                    Email = user.Email,
                })
                .OrderByDescending(x => x.Id)
                .Skip((int)(page-1)*pageSize)
                .Take(pageSize)
                .ToListAsync();

                if (users.Count < 0 || users is null )
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;

                    return new ApiResponse
                    {
                        ErrorMessage = "no user exist"
                    };
                }

                var response = new Pagination
                {
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    Data = users,
                    Page = page
                    
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
                Result = result,
                ErrorMessage = errorMessage ?? "",
            };

            
        }

        [HttpGet("{id}")]
        public async Task<ApiResponse> GetUserById(int id)
        {
            string errorMessage = default;
            var result = default(UserProfileDto);

            try
            {
                var user = await _context.Users
                .Select(user => new UserProfileDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Phone = user.Phone,
                    Address = user.Address,
                    Role = user.Role,
                    Email = user.Email,
                }).FirstOrDefaultAsync(x => x.Id == id);

                if (user is null)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return new ApiResponse
                    {
                        ErrorMessage = "Cannot find desired user"
                    };
                }

                result = user;



            }
            catch (Exception ex)
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorMessage = ex.Message;
            }

            return new ApiResponse
            {
                ErrorMessage = errorMessage ?? string.Empty,
                Result = result,
            };

             
        }
    }
}
