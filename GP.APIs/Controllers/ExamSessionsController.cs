using GP.Core.DTOs;
using GP.Core.Entities;
using GP.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GP.APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExamSessionsController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;

        public ExamSessionsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // الطالب يدخل الامتحان
        [HttpPost("join")]
        public async Task<IActionResult> Join(JoinExamDto dto)
        {
            // 1. لقي الامتحان
            var exam = await _unitOfWork.Repository<Exam>()
                .FindOneAsync(e => e.Code == dto.ExamCode && e.Status == "Active");

            if (exam == null) return NotFound("Exam not found or not active");

            // 2. لقي الطالب
            var student = await _unitOfWork.Repository<Student>()
                .FindOneAsync(s => s.StudentUniversityID == dto.StudentUniversityID);

            if (student == null) return NotFound("Student not found");

            // 3. تأكد مش عامل session قبل كده
            var existing = await _unitOfWork.Repository<ExamSession>()
                .FindOneAsync(s => s.ExamId == exam.Id
                               && s.StudentId == student.Id
                               && s.Status == "InProgress");

            if (existing != null) return BadRequest("Already in an active session");

            // 4. عمل Session جديد
            var session = new ExamSession
            {
                ExamId = exam.Id,
                StudentId = student.Id,
                StartTime = DateTime.UtcNow,
                Status = "InProgress",
                FinalRiskScore = 0,
                RiskLevel = "Low",
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<ExamSession>().AddAsync(session);
            await _unitOfWork.CompleteAsync();

            // 5. رجّع الأسئلة مع الـ Session
            var questions = await _unitOfWork.Repository<Question>()
                .FindAsync(q => q.ExamId == exam.Id);

            return Ok(new
            {
                SessionId = session.Id,
                ExamId = exam.Id,
                Subject = exam.Subject,
                DurationMinutes = exam.DurationMinutes,
                Questions = questions.Select(q => new
                {
                    q.Id,
                    q.QuestionText,
                    q.OptionA,
                    q.OptionB,
                    q.OptionC,
                    q.OptionD,
                    q.OrderIndex
                    // مش بنبعت CorrectAnswer للطالب ❌
                })
            });
        }

        // الـ AI بيبعت نتيجة
        [HttpPost("{sessionId}/ai-result")]
        public async Task<IActionResult> ReceiveAiResult(int sessionId, AiResultDto dto)
        {
            var session = await _unitOfWork.Repository<ExamSession>()
                .GetByIdAsync(sessionId);

            if (session == null) return NotFound("Session not found");
            if (session.Status != "InProgress") return BadRequest("Session is not active");

            // Parse EventType
            if (!Enum.TryParse<EventType>(dto.EventType, out var eventType))
                return BadRequest("Invalid event type");

            var suspiciousEvent = new SuspiciousEvent
            {
                SessionId = sessionId,
                EventType = eventType,
                RiskScoreContribution = dto.RiskScoreContribution,
                Confidence = dto.Confidence,
                Source = "AI",
                FrameUrl = dto.FrameUrl,
                MetadataJson = dto.MetadataJson,
                Timestamp = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<SuspiciousEvent>().AddAsync(suspiciousEvent);

            // حدّث الـ RiskScore
            session.FinalRiskScore += dto.RiskScoreContribution;
            session.RiskLevel = session.FinalRiskScore switch
            {
                <= 0.3m => "Low",
                <= 0.7m => "Medium",
                _ => "High"
            };
            session.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<ExamSession>().Update(session);
            await _unitOfWork.CompleteAsync();

            return Ok(new { RiskScore = session.FinalRiskScore, RiskLevel = session.RiskLevel });
        }

        // الـ Frontend بيبعت Browser Event
        [HttpPost("{sessionId}/browser-event")]
        public async Task<IActionResult> ReceiveBrowserEvent(int sessionId, BrowserEventDto dto)
        {
            var session = await _unitOfWork.Repository<ExamSession>()
                .GetByIdAsync(sessionId);

            if (session == null) return NotFound("Session not found");
            if (session.Status != "InProgress") return BadRequest("Session is not active");

            if (!Enum.TryParse<EventType>(dto.EventType, out var eventType))
                return BadRequest("Invalid event type");

            var browserEvent = new SuspiciousEvent
            {
                SessionId = sessionId,
                EventType = eventType,
                RiskScoreContribution = 0.1m, // Browser events بتاخد risk بسيط
                Confidence = 1.0,
                Source = "Browser",
                MetadataJson = dto.MetadataJson,
                Timestamp = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<SuspiciousEvent>().AddAsync(browserEvent);

            session.FinalRiskScore += 0.1m;
            session.RiskLevel = session.FinalRiskScore switch
            {
                <= 0.3m => "Low",
                <= 0.7m => "Medium",
                _ => "High"
            };
            session.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<ExamSession>().Update(session);
            await _unitOfWork.CompleteAsync();

            return Ok(new { RiskScore = session.FinalRiskScore, RiskLevel = session.RiskLevel });
        }

        // الطالب يسلم الامتحان
        [HttpPost("{sessionId}/submit")]
        public async Task<IActionResult> Submit(int sessionId, SubmitExamDto dto)
        {
            var session = await _unitOfWork.Repository<ExamSession>()
                .GetByIdAsync(sessionId);

            if (session == null) return NotFound("Session not found");
            if (session.Status != "InProgress") return BadRequest("Session already submitted");

            int correct = 0;
            int total = dto.Answers.Count;

            foreach (var answer in dto.Answers)
            {
                var question = await _unitOfWork.Repository<Question>()
                    .GetByIdAsync(answer.QuestionId);

                if (question == null) continue;

                var isCorrect = question.CorrectAnswer == answer.SelectedAnswer;
                if (isCorrect) correct++;

                var studentAnswer = new StudentAnswer
                {
                    SessionId = sessionId,
                    QuestionId = answer.QuestionId,
                    SelectedAnswer = answer.SelectedAnswer,
                    IsCorrect = isCorrect,
                    AnsweredAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Repository<StudentAnswer>().AddAsync(studentAnswer);
            }

            // اقفل الـ Session
            session.Status = "Completed";
            session.EndTime = DateTime.UtcNow;
            session.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Repository<ExamSession>().Update(session);

            // عمل الـ Report
            var exam = await _unitOfWork.Repository<Exam>()
                .GetByIdAsync(session.ExamId);

            var instructor = await _unitOfWork.Repository<Instructor>()
                .GetByIdAsync(exam.InstructorId);

            var report = new ExamReport
            {
                SessionId = sessionId,
                InstructorId = instructor.Id,
                FinalRiskScore = session.FinalRiskScore,
                RiskLevel = session.RiskLevel,
                TotalQuestions = total,
                CorrectAnswers = correct,
                ScorePercent = total > 0 ? Math.Round((decimal)correct / total * 100, 2) : 0,
                Status = "Pending",
                Notes = "",
                GeneratedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<ExamReport>().AddAsync(report);
            await _unitOfWork.CompleteAsync();

            return Ok(new
            {
                SessionId = sessionId,
                TotalQuestions = total,
                CorrectAnswers = correct,
                ScorePercent = report.ScorePercent,
                RiskScore = session.FinalRiskScore,
                RiskLevel = session.RiskLevel
            });
        }

        // الدكتور يشوف Sessions
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var sessions = await _unitOfWork.Repository<ExamSession>().GetAllAsync();
            return Ok(sessions);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var session = await _unitOfWork.Repository<ExamSession>().GetByIdAsync(id);
            if (session == null) return NotFound();
            return Ok(session);
        }
    }
}