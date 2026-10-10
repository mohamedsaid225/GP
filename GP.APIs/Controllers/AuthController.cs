using System.Security.Cryptography;
using System.Text;
using GP.Core.DTOs;
using GP.Core.Entities;
using GP.Core.Interfaces;
using GP.Core.Repositories.Contract;
using Microsoft.AspNetCore.Mvc;

namespace GP.APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;

        public AuthController(IUnitOfWork unitOfWork, ITokenService tokenService)
        {
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
        }


        //[HttpPost("login")]
        //public async Task<IActionResult> Login(LoginDto dto)
        //{
        //    // 1. Find user by email
        //    var user = await _unitOfWork.Repository<User>()
        //        .FindOneAsync(u => u.Email == dto.Email);

        //    if (user == null) return Unauthorized("Invalid email or password");

        //    // 2. Verify password hash
        //    using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes("GPSecretSalt"));
        //    var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dto.Password));
        //    var computedHashString = Convert.ToBase64String(computedHash);

        //    if (user.PasswordHash != computedHashString)
        //        return Unauthorized("Invalid email or password");

        //    // 3. Return token
        //    return Ok(new UserDto
        //    {
        //        Id = user.Id,
        //        Email = user.Email,
        //        FirstName = user.FirstName,
        //        LastName = user.LastName,
        //        Role = user.Role,
        //        Token = _tokenService.CreateToken(user)
        //    });
        //}

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            // 1. البحث عن المستخدم بالإيميل
            var user = await _unitOfWork.Repository<User>()
                .FindOneAsync(u => u.Email == dto.Email);

            if (user == null) return Unauthorized("Invalid email or password");

            // 2. التحقق من الباسورد
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes("GPSecretSalt"));
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dto.Password));
            var computedHashString = Convert.ToBase64String(computedHash);

            if (user.PasswordHash != computedHashString)
                return Unauthorized("Invalid email or password");

            string instructorCode = null;

            if (user.Role == "Instructor")
            {
                var instructor = await _unitOfWork.Repository<Instructor>()
                    .FindOneAsync(i => i.UserId == user.Id);

                instructorCode = instructor?.InstructorCode; // ✅
            }

            var auditLog = new AuditLog
            {
                UserId = user.Id,
                Action = "Login",
                IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Details = $"{user.FullName} logged in as {user.Role}",
                Timestamp = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Repository<AuditLog>().AddAsync(auditLog);
            await _unitOfWork.CompleteAsync();

            return Ok(new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                Token = _tokenService.CreateToken(user),
                InstructorCode = instructorCode // ✅
            });

            
        }
    }
}
