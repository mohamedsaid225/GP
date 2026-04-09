using GP.Core.DTOs;
using GP.Core.Entities;
using GP.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GP.APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionsController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;

        public QuestionsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // الدكتور يضيف سؤال
        [HttpPost]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Create(CreateQuestionDto dto)
        {
            // تأكد إن الامتحان موجود
            var exam = await _unitOfWork.Repository<Exam>().GetByIdAsync(dto.ExamId);
            if (exam == null) return NotFound("Exam not found");

            // تأكد إن الدكتور ده هو صاحب الامتحان
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            var userId = int.Parse(userIdClaim.Value);
            var instructor = await _unitOfWork.Repository<Instructor>()
                .FindOneAsync(i => i.UserId == userId);

            if (instructor == null || exam.InstructorId != instructor.Id)
                return Unauthorized("You don't own this exam");

            // تأكد إن الـ CorrectAnswer صح
            var validAnswers = new[] { "A", "B", "C", "D" };
            if (!validAnswers.Contains(dto.CorrectAnswer.ToUpper()))
                return BadRequest("CorrectAnswer must be A, B, C, or D");

            var question = new Question
            {
                ExamId = dto.ExamId,
                QuestionText = dto.QuestionText,
                OptionA = dto.OptionA,
                OptionB = dto.OptionB,
                OptionC = dto.OptionC,
                OptionD = dto.OptionD,
                CorrectAnswer = dto.CorrectAnswer.ToUpper(),
                OrderIndex = dto.OrderIndex,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<Question>().AddAsync(question);
            await _unitOfWork.CompleteAsync();

            return CreatedAtAction(nameof(GetById), new { id = question.Id }, new
            {
                question.Id,
                question.ExamId,
                question.QuestionText,
                question.OptionA,
                question.OptionB,
                question.OptionC,
                question.OptionD,
                question.OrderIndex
                // مش بنرجع CorrectAnswer في الـ Response ❌
            });
        }

        // الدكتور يضيف أسئلة كتير دفعة واحدة
        [HttpPost("bulk")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> CreateBulk(List<CreateQuestionDto> dtos)
        {
            if (!dtos.Any()) return BadRequest("No questions provided");

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            var userId = int.Parse(userIdClaim.Value);
            var instructor = await _unitOfWork.Repository<Instructor>()
                .FindOneAsync(i => i.UserId == userId);

            if (instructor == null) return Unauthorized();

            var validAnswers = new[] { "A", "B", "C", "D" };
            var questions = new List<Question>();

            foreach (var dto in dtos)
            {
                var exam = await _unitOfWork.Repository<Exam>().GetByIdAsync(dto.ExamId);
                if (exam == null || exam.InstructorId != instructor.Id)
                    return Unauthorized($"You don't own exam {dto.ExamId}");

                if (!validAnswers.Contains(dto.CorrectAnswer.ToUpper()))
                    return BadRequest($"CorrectAnswer must be A, B, C, or D");

                questions.Add(new Question
                {
                    ExamId = dto.ExamId,
                    QuestionText = dto.QuestionText,
                    OptionA = dto.OptionA,
                    OptionB = dto.OptionB,
                    OptionC = dto.OptionC,
                    OptionD = dto.OptionD,
                    CorrectAnswer = dto.CorrectAnswer.ToUpper(),
                    OrderIndex = dto.OrderIndex,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _unitOfWork.Repository<Question>().AddRangeAsync(questions);
            await _unitOfWork.CompleteAsync();

            return Ok(new { Message = $"{questions.Count} questions added successfully" });
        }

        // الدكتور يشوف أسئلة امتحان معين
        [HttpGet("exam/{examId}")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> GetByExam(int examId)
        {
            var questions = await _unitOfWork.Repository<Question>()
                .FindAsync(q => q.ExamId == examId);

            return Ok(questions.Select(q => new
            {
                q.Id,
                q.ExamId,
                q.QuestionText,
                q.OptionA,
                q.OptionB,
                q.OptionC,
                q.OptionD,
                q.CorrectAnswer, // الدكتور يشوف الإجابة الصح ✅
                q.OrderIndex
            }));
        }

        // الدكتور يمسح سؤال
        [HttpDelete("{id}")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Delete(int id)
        {
            var question = await _unitOfWork.Repository<Question>().GetByIdAsync(id);
            if (question == null) return NotFound();

            _unitOfWork.Repository<Question>().Delete(question);
            await _unitOfWork.CompleteAsync();

            return NoContent();
        }

        // الدكتور يعدل سؤال
        [HttpPut("{id}")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Update(int id, CreateQuestionDto dto)
        {
            var question = await _unitOfWork.Repository<Question>().GetByIdAsync(id);
            if (question == null) return NotFound();

            question.QuestionText = dto.QuestionText;
            question.OptionA = dto.OptionA;
            question.OptionB = dto.OptionB;
            question.OptionC = dto.OptionC;
            question.OptionD = dto.OptionD;
            question.CorrectAnswer = dto.CorrectAnswer.ToUpper();
            question.OrderIndex = dto.OrderIndex;
            question.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<Question>().Update(question);
            await _unitOfWork.CompleteAsync();

            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var question = await _unitOfWork.Repository<Question>().GetByIdAsync(id);
            if (question == null) return NotFound();
            return Ok(question);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var questions = await _unitOfWork.Repository<Question>().GetAllAsync();
            return Ok(questions);
        }
    }
}