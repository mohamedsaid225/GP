using GP.Core.Entities;
using GP.Core.Interfaces;
using GP.Core.Repositories.Contract;
using Microsoft.AspNetCore.Mvc;

namespace GP.APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentAnswersController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;

        public StudentAnswersController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var answers = await _unitOfWork.Repository<StudentAnswer>().GetAllAsync();
            return Ok(answers);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var answer = await _unitOfWork.Repository<StudentAnswer>().GetByIdAsync(id);
            if (answer == null) return NotFound();
            return Ok(answer);
        }

        [HttpPost]
        public async Task<IActionResult> Create(StudentAnswer answer)
        {
            await _unitOfWork.Repository<StudentAnswer>().AddAsync(answer);
            await _unitOfWork.CompleteAsync();
            return CreatedAtAction(nameof(GetById), new { id = answer.Id }, answer);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, StudentAnswer answer)
        {
            if (id != answer.Id) return BadRequest();
            _unitOfWork.Repository<StudentAnswer>().Update(answer);
            await _unitOfWork.CompleteAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var answer = await _unitOfWork.Repository<StudentAnswer>().GetByIdAsync(id);
            if (answer == null) return NotFound();
            _unitOfWork.Repository<StudentAnswer>().Delete(answer);
            await _unitOfWork.CompleteAsync();
            return NoContent();
        }
    }
}