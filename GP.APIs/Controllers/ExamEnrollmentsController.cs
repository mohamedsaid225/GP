using GP.Core.Entities;
using GP.Core.Interfaces;
using GP.Core.Repositories.Contract;
using Microsoft.AspNetCore.Mvc;

namespace GP.APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExamEnrollmentsController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;

        public ExamEnrollmentsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var enrollments = await _unitOfWork.Repository<ExamEnrollment>().GetAllAsync();
            return Ok(enrollments);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var enrollment = await _unitOfWork.Repository<ExamEnrollment>().GetByIdAsync(id);
            if (enrollment == null) return NotFound();
            return Ok(enrollment);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ExamEnrollment enrollment)
        {
            await _unitOfWork.Repository<ExamEnrollment>().AddAsync(enrollment);
            await _unitOfWork.CompleteAsync();
            return CreatedAtAction(nameof(GetById), new { id = enrollment.Id }, enrollment);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ExamEnrollment enrollment)
        {
            if (id != enrollment.Id) return BadRequest();
            _unitOfWork.Repository<ExamEnrollment>().Update(enrollment);
            await _unitOfWork.CompleteAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var enrollment = await _unitOfWork.Repository<ExamEnrollment>().GetByIdAsync(id);
            if (enrollment == null) return NotFound();
            _unitOfWork.Repository<ExamEnrollment>().Delete(enrollment);
            await _unitOfWork.CompleteAsync();
            return NoContent();
        }
    }
}