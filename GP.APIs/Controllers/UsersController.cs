using GP.Core.DTOs;
using GP.Core.Entities;
using GP.Core.Interfaces;
using GP.Core.Repositories.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GP.APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;

        public UsersController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        //[HttpGet]
        //public async Task<IActionResult> GetAll()
        //{
        //    var users = await _unitOfWork.Repository<User>().GetAllAsync();
        //    return Ok(users);
        //}

        [HttpGet]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> GetAll()
        {
            var users = await _unitOfWork.Repository<User>()
                .GetAllAsync(u => u.Instructor);
            
            var userDtos = users.Select(user => new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                InstructorCode = user.Instructor?.InstructorCode
            }).ToList();

            return Ok(userDtos);
        }


        [HttpGet("{id}")]
        //[Authorize(Roles = "Instructor")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(id);
            if (user == null) return NotFound();

            return Ok(new
            {
                user.Id,
                user.FullName,
                user.Email,
                user.Role,
                user.UniversityID
            });
            //var user = await _unitOfWork.Repository<User>().GetByIdAsync(id);
            //if (user == null) return NotFound();
            //return Ok(user);
        }

        #region Create
        //[HttpPost]
        //public async Task<IActionResult> Create(User user)
        //{
        //    await _unitOfWork.Repository<User>().AddAsync(user);
        //    await _unitOfWork.CompleteAsync();
        //    return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        //} 
        #endregion

        #region Update
        //[HttpPut("{id}")]
        //public async Task<IActionResult> Update(int id, User user)
        //{
        //    if (id != user.Id) return BadRequest();
        //    _unitOfWork.Repository<User>().Update(user);
        //    await _unitOfWork.CompleteAsync();
        //    return NoContent();
        //} 
        #endregion

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(id);
            if (user == null) return NotFound();
            _unitOfWork.Repository<User>().Delete(user);
            await _unitOfWork.CompleteAsync();
            return NoContent();
        }


        [HttpGet("{id}/profile")]
        //[Authorize(Roles = "Instructor")]
        public async Task<IActionResult> GetProfile(int id)
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(id);
            if (user == null) return NotFound();

            if (user.Role == "Instructor")
            {
                var instructor = await _unitOfWork.Repository<Instructor>()
                    .FindOneAsync(i => i.UserId == id);

                return Ok(new
                {
                    user.Id,
                    user.FullName,
                    user.Email,
                    user.Role,
                    instructor?.InstructorCode,
                    instructor?.Department,
                    instructor?.University,
                    instructor?.College
                });
            }

            return Ok(new
            {
                user.Id,
                user.FullName,
                user.Email,
                user.Role
            });
        }
    }
}