using GP.Core.Entities;
using GP.Core.Interfaces;
using GP.Core.Repositories.Contract;
using Microsoft.AspNetCore.Mvc;

namespace GP.APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InstructorsController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;

        public InstructorsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        //public async Task<IActionResult> GetAll()
        //{
        //    var instructors = await _unitOfWork
        //        .Repository<Instructor>()
        //        .GetAllAsync(i => i.User);
        //    return Ok(instructors);
        //}
        public async Task<IActionResult> GetAll()
        {
            var instructors = await _unitOfWork
    .Repository<Instructor>()
    .GetAllAsync(i => i.User, i => i.Exams);

            var result = instructors.Select(i => new
            {
                i.Id,
                i.InstructorCode,
                i.Department,
                i.University,
                i.College,

                Name = i.User.FullName,
                Email = i.User.Email,

                Exams = i.Exams.Select(e => new
                {
                    e.Id,
                    e.Code,
                    e.Subject,
                    e.Level,
                    e.DurationMinutes,
                    e.Status
                })
            });

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var instructor = await _unitOfWork.Repository<Instructor>().GetByIdAsync(id);
            if (instructor == null) return NotFound();
            return Ok(instructor);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Instructor instructor)
        {
            await _unitOfWork.Repository<Instructor>().AddAsync(instructor);
            await _unitOfWork.CompleteAsync();
            return CreatedAtAction(nameof(GetById), new { id = instructor.Id }, instructor);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Instructor instructor)
        {
            if (id != instructor.Id) return BadRequest();
            _unitOfWork.Repository<Instructor>().Update(instructor);
            await _unitOfWork.CompleteAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var instructor = await _unitOfWork.Repository<Instructor>().GetByIdAsync(id);
            if (instructor == null) return NotFound();
            _unitOfWork.Repository<Instructor>().Delete(instructor);
            await _unitOfWork.CompleteAsync();
            return NoContent();
        }
    }
}