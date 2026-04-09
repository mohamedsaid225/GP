using GP.Core.DTOs;
using GP.Core.Entities;
using GP.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GP.APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExamsController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;

        public ExamsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // الطالب يشوف الامتحانات المتاحة
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailable()
        {
            var exams = await _unitOfWork.Repository<Exam>()
                .FindAsync(e => e.Status == "Active");

            var result = new List<ExamDto>();

            foreach (var exam in exams)
            {
                var instructor = await _unitOfWork.Repository<Instructor>()
                    .GetByIdAsync(exam.InstructorId);

                var user = instructor != null
                    ? await _unitOfWork.Repository<User>().GetByIdAsync(instructor.UserId)
                    : null;

                result.Add(new ExamDto
                {
                    Id = exam.Id,
                    Code = exam.Code,
                    Subject = exam.Subject,
                    Department = exam.Department,
                    University = exam.University,
                    College = exam.College,
                    Level = exam.Level,
                    DurationMinutes = exam.DurationMinutes,
                    Status = exam.Status,
                    InstructorName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown"
                });
            }

            return Ok(result);
        }

        // الدكتور يضيف امتحان
        [HttpPost]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Create(CreateExamDto dto)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            var userId = int.Parse(userIdClaim.Value);
            var instructor = await _unitOfWork.Repository<Instructor>()
                .FindOneAsync(i => i.UserId == userId);

            if (instructor == null) return Unauthorized("Not an instructor");

            var exam = new Exam
            {
                InstructorId = instructor.Id,
                Code = dto.Code,
                Subject = dto.Subject,
                Department = dto.Department,
                University = dto.University,
                College = dto.College,
                Level = dto.Level,
                DurationMinutes = dto.DurationMinutes,
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<Exam>().AddAsync(exam);
            await _unitOfWork.CompleteAsync();

            // رجّع DTO بس مش الـ Entity
            return CreatedAtAction(nameof(GetById), new { id = exam.Id }, new ExamDto
            {
                Id = exam.Id,
                Code = exam.Code,
                Subject = exam.Subject,
                Department = exam.Department,
                University = exam.University,
                College = exam.College,
                Level = exam.Level,
                DurationMinutes = exam.DurationMinutes,
                Status = exam.Status,
                InstructorName = $"{User.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value}"
            });
        }

        // الدكتور يشوف امتحاناته
        [HttpGet("my-exams")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> GetMyExams()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            var userId = int.Parse(userIdClaim.Value);
            var instructor = await _unitOfWork.Repository<Instructor>()
                .FindOneAsync(i => i.UserId == userId);

            if (instructor == null) return Unauthorized();

            var exams = await _unitOfWork.Repository<Exam>()
                .FindAsync(e => e.InstructorId == instructor.Id);

            return Ok(exams);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var exam = await _unitOfWork.Repository<Exam>().GetByIdAsync(id);
            if (exam == null) return NotFound();
            return Ok(exam);
        }

        // الدكتور يمسح امتحان
        [HttpDelete("{id}")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Delete(int id)
        {
            var exam = await _unitOfWork.Repository<Exam>().GetByIdAsync(id);
            if (exam == null) return NotFound();

            _unitOfWork.Repository<Exam>().Delete(exam);
            await _unitOfWork.CompleteAsync();

            return NoContent();
        }

        // الدكتور يعدل امتحان
        [HttpPut("{id}")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Update(int id, CreateExamDto dto)
        {
            var exam = await _unitOfWork.Repository<Exam>().GetByIdAsync(id);
            if (exam == null) return NotFound();

            exam.Code = dto.Code;
            exam.Subject = dto.Subject;
            exam.Department = dto.Department;
            exam.University = dto.University;
            exam.College = dto.College;
            exam.Level = dto.Level;
            exam.DurationMinutes = dto.DurationMinutes;
            exam.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<Exam>().Update(exam);
            await _unitOfWork.CompleteAsync();

            return NoContent();
        }
    }
}