using Backend.ApiModel.Base;
using Backend.ApiModel.User;
using Backend.Data;
using Backend.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public AccountController(IConfiguration configuration , ApplicationDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        [HttpPost("Register")]
        public async Task<ApiResponse> Register(UserDto userDto)
        {
            var emailCount = await _context.Users.CountAsync(u => u.Email == userDto.Email);
            if (emailCount > 0)
            {
                return new ApiResponse
                {
                    ErrorMessage = "Email already exists"
                };

               
            }

            var passwordHasher = new PasswordHasher<User>();

            var encryptedPassword = passwordHasher.HashPassword(new User(), userDto.Password);

            User user = new User()
            {
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                Email = userDto.Email,
                Password = encryptedPassword,
                Address = userDto.Address,
                Phone = userDto.Phone ?? "",
                Role = "Client",
                CreatedAt = DateTime.Now,

            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var jwt = CreateJwt(user);

            var userProfileInfo = new UserProfileDto()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                Role = user.Role,
                CreatedAt = DateTime.Now
            };

            var responses = new
            {
                Token = jwt,
                userDetails = userProfileInfo
            };

            return new ApiResponse
            {
                Result = responses,
            };

        }
        [HttpPost("Login")]
        public async Task<ApiResponse>Login (string email, string password)
        {
            var user  = await _context.Users.FirstOrDefaultAsync(u=>u.Email==email);
            if(user is null)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;

                return new ApiResponse
                {
                    ErrorMessage = "Email does not exist try signing up"
                };
            }

            var passwordHasher = new PasswordHasher<User>();
            var getPasssword = passwordHasher.VerifyHashedPassword(new User(), user.Password, password);

            if(getPasssword ==PasswordVerificationResult.Failed)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return new ApiResponse
                {
                    ErrorMessage = "Email or password is incorrect"
                };

            }
            var jwt = CreateJwt(user);

            var userProfileInfo = new UserProfileDto()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                Role = user.Role,
                CreatedAt = DateTime.Now
            };

            var responses = new
            {
                Token = jwt,
                userDetails = userProfileInfo
            };

            return new ApiResponse
            {
                Result = responses,
            };

        }

        [HttpPost("ForgotPassword")]
        public async Task<ApiResponse> ForgotPassword (string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user is null)
            {
                Response.StatusCode = (int) HttpStatusCode.BadRequest;

                return new ApiResponse
                {
                    ErrorMessage = "Email does not exist"
                };

            }

            var oldPasswordReset = await _context.PasswordResets.FirstOrDefaultAsync(u => u.Email == email);

            if (oldPasswordReset is not null)
            {
                _context.Remove(oldPasswordReset);
            }

            string token =Guid.NewGuid().ToString() + "/" +Guid.NewGuid().ToString();

            var newPassword = new PasswordReset()
            {
                Email = email,
                Token = token,
                CreatedAt = DateTime.Now
            };

            await _context.PasswordResets.AddAsync(newPassword);
            await _context.SaveChangesAsync();

            // send the password token as email to the user using sendgrid

            return new ApiResponse
            {

            };
        }

        [HttpPost("ResetPassword")]
        public async Task<ApiResponse> ResetPassword (string token , string password)
        {
            var passwordReset = await _context.PasswordResets.FirstOrDefaultAsync(r => r.Token == token);

            if (passwordReset is null)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;

                return new ApiResponse
                {
                    ErrorMessage = "Authentication not found"
                };
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == passwordReset.Email);
            if (user is null)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;

                return new ApiResponse
                {
                    ErrorMessage = "Email does not exist"
                };

            }

            var retrievePassword = new PasswordHasher<User>();
            var encryptPassword = retrievePassword.HashPassword(new User(), password);

            _context.PasswordResets.Remove(passwordReset);

            await _context.SaveChangesAsync();

            return new ApiResponse
            {

            };
        }

        [Authorize]
        [HttpGet("Profile")]
        public async Task<ApiResponse> GetProfile()
        {
            var id = JwtReader.GetUserId(User);

            var user = await _context.Users.FindAsync(id);

            if(user is null)
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;

                return new ApiResponse
                {
                    ErrorMessage = "User details unavailable"
                };
            }

            var DisplayUser = new UserProfileDto()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            };

            return new ApiResponse
            {
                Result = DisplayUser
            };

        }

        [Authorize]
        [HttpPut("UpdateProfile")]
        public async Task<ApiResponse> UpdateProfile(UserProfileUpdateDto userUpdateDto)
        {
            var id = JwtReader.GetUserId(User);

            var user = await _context.Users.FindAsync(id);

            if (user is null)
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;

                return new ApiResponse
                {
                    ErrorMessage = "User not found"
                };
            }

            user.FirstName = userUpdateDto.FirstName;
            user.LastName = userUpdateDto.LastName;
            user.Phone = userUpdateDto.Phone ?? "";
            user.Email = userUpdateDto.Email;
            user.Address = userUpdateDto.Address;

            await _context.SaveChangesAsync();

            var DisplayUser = new UserProfileDto()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone ?? "",
                Address = user.Address,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            };

            return new ApiResponse
            {
                Result = DisplayUser
            };

        }

        [Authorize]
        [HttpPut("UpdatePassword")]
        public async Task<ApiResponse> UpdatePassword([Required , MinLength(8)]string password)
        {
            var id = JwtReader.GetUserId(User);

            var user = await _context.Users.FindAsync(id);

            if (user is null)
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;

                return new ApiResponse
                {
                    ErrorMessage = "User not found"
                };
            }

            var passwordHasher = new PasswordHasher<User>();
            var encryptedPassword = passwordHasher.HashPassword(new User(), password);

            user.Password = encryptedPassword;

            await _context.SaveChangesAsync();

            return new ApiResponse
            {
                Result = "Password has been Successfully updated"
            };

        }


        private string CreateJwt (User user )
        {
            List<Claim> claims = new List<Claim>()
            {
                new Claim("id" ,"" + user.Id),
                new Claim("role" , user.Role),

            };

            string strKey = _configuration["JwtSettings:Key"]!;

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(strKey));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var token =  new JwtSecurityToken(

                issuer: _configuration["JwtSettings:Issuer"],
                audience:_configuration["JwtSettings:Audience"],
                claims:claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;

        }
    }
}
