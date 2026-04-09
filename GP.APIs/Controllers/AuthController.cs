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

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            // 1. Find user by email
            var user = await _unitOfWork.Repository<User>()
                .FindOneAsync(u => u.Email == dto.Email);

            if (user == null) return Unauthorized("Invalid email or password");

            // 2. Verify password hash
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes("GPSecretSalt"));
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dto.Password));
            var computedHashString = Convert.ToBase64String(computedHash);

            if (user.PasswordHash != computedHashString)
                return Unauthorized("Invalid email or password");

            // 3. Return token
            return Ok(new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                Token = _tokenService.CreateToken(user)
            });
        }
    }
}
