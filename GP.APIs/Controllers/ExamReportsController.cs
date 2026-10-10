using GP.Core.Entities;
using GP.Core.Interfaces;
using GP.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GP.APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExamReportsController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IExcelService _excelService;

        public ExamReportsController(IUnitOfWork unitOfWork, IExcelService excelService)
        {
            _unitOfWork = unitOfWork;
            _excelService = excelService;
        }

        // الدكتور يشوف كل الـ Reports بتاعته
        [HttpGet("my-reports")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> GetMyReports()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            var userId = int.Parse(userIdClaim.Value);
            var instructor = await _unitOfWork.Repository<Instructor>()
                .FindOneAsync(i => i.UserId == userId);

            if (instructor == null) return Unauthorized();

            var reports = await _unitOfWork.Repository<ExamReport>()
                .FindAsync(r => r.InstructorId == instructor.Id);

            var result = new List<object>();

            foreach (var report in reports)
            {
                var session = await _unitOfWork.Repository<ExamSession>()
                    .GetByIdAsync(report.SessionId);

                var student = session != null
                    ? await _unitOfWork.Repository<Student>().GetByIdAsync(session.StudentId)
                    : null;

                var studentUser = student != null
                    ? await _unitOfWork.Repository<User>().GetByIdAsync(student.UserId)
                    : null;

                var exam = session != null
                    ? await _unitOfWork.Repository<Exam>().GetByIdAsync(session.ExamId)
                    : null;

                result.Add(new
                {
                    report.Id,
                    report.TotalQuestions,
                    report.CorrectAnswers,
                    report.ScorePercent,
                    report.Status,
                    report.Notes,
                    report.GeneratedAt,
                    report.ReviewedAt,
                    StudentName = studentUser != null
                        ? $"{studentUser.FullName}"
                        : "Unknown",
                    StudentUniversityID = student?.StudentUniversityID,
                    ExamSubject = exam?.Subject,
                    ExamCode = exam?.Code
                });
            }

            return Ok(result);
        }

        // الدكتور يشوف Report بتاع Session معين
        [HttpGet("session/{sessionId}")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> GetBySession(int sessionId)
        {
            var report = await _unitOfWork.Repository<ExamReport>()
                .FindOneAsync(r => r.SessionId == sessionId);

            if (report == null) return NotFound("Report not found");

            var session = await _unitOfWork.Repository<ExamSession>()
                .GetByIdAsync(report.SessionId);

            var student = session != null
                ? await _unitOfWork.Repository<Student>().GetByIdAsync(session.StudentId)
                : null;

            var studentUser = student != null
                ? await _unitOfWork.Repository<User>().GetByIdAsync(student.UserId)
                : null;

            var answers = await _unitOfWork.Repository<StudentAnswer>()
                .FindAsync(a => a.SessionId == sessionId);

            var suspiciousEvent = await _unitOfWork.Repository<SuspiciousEvent>()
                                                   .FindOneAsync(e => e.SessionId == sessionId);

            return Ok(new
            {
                report.Id,
                report.TotalQuestions,
                report.CorrectAnswers,
                report.ScorePercent,
                report.CheatingCount,
                report.ScreenshotsUrl,
                report.Status,
                report.Notes,
                report.GeneratedAt,
                report.ReviewedAt,
                StudentName = studentUser?.FullName ?? "Unknown",
                StudentUniversityID = student?.StudentUniversityID,
                SuspiciousEvent = suspiciousEvent != null ? new
                {
                    suspiciousEvent.CheatingCount,
                    suspiciousEvent.LeavingExamCount,
                    suspiciousEvent.ScreenshotsUrl,
                    suspiciousEvent.Timestamp
                } : null,
                Answers = answers.Select(a => new
                {
                    a.QuestionId,
                    a.SelectedAnswer,
                    a.IsCorrect,
                    a.AnsweredAt
                })
            });
        }

        // الدكتور يضيف Notes على الـ Report
        [HttpPut("{id}/review")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Review(int id, [FromBody] string notes)
        {
            var report = await _unitOfWork.Repository<ExamReport>().GetByIdAsync(id);
            if (report == null) return NotFound();

            report.Notes = notes;
            report.Status = "Reviewed";
            report.ReviewedAt = DateTime.UtcNow;
            report.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<ExamReport>().Update(report);
            await _unitOfWork.CompleteAsync();

            return NoContent();
        }

        // ─── Basic CRUD ───────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var reports = await _unitOfWork.Repository<ExamReport>().GetAllAsync();
            return Ok(reports);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var report = await _unitOfWork.Repository<ExamReport>().GetByIdAsync(id);
            if (report == null) return NotFound();
            return Ok(report);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Delete(int id)
        {
            var report = await _unitOfWork.Repository<ExamReport>().GetByIdAsync(id);
            if (report == null) return NotFound();
            _unitOfWork.Repository<ExamReport>().Delete(report);
            await _unitOfWork.CompleteAsync();
            return NoContent();
        }

        // الدكتور يحمل الـ Excel Report
        [HttpGet("{id}/export")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> ExportToExcel(int id)
        {
            var report = await _unitOfWork.Repository<ExamReport>().GetByIdAsync(id);
            if (report == null) return NotFound();

            var session = await _unitOfWork.Repository<ExamSession>().GetByIdAsync(report.SessionId);
            var student = await _unitOfWork.Repository<Student>().GetByIdAsync(session.StudentId);
            var studentUser = await _unitOfWork.Repository<User>().GetByIdAsync(student.UserId);
            var exam = await _unitOfWork.Repository<Exam>().GetByIdAsync(session.ExamId);

            var events = await _unitOfWork.Repository<SuspiciousEvent>()
                .FindAsync(e => e.SessionId == session.Id);

            var answers = await _unitOfWork.Repository<StudentAnswer>()
                .FindAsync(a => a.SessionId == session.Id);

            var fileBytes = await _excelService.GenerateExamReportAsync(
                report, session, studentUser, student, exam, events, answers);

            var fileName = $"Report_{student.StudentUniversityID}_{exam.Code}_{DateTime.UtcNow:yyyyMMdd}.xlsx";

            return File(fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        //[HttpGet("exam/{examId}/export")]
        //[Authorize(Roles = "Instructor")]
        //public async Task<IActionResult> ExportExamReports(int examId)
        //{
        //    var exam = await _unitOfWork.Repository<Exam>().GetByIdAsync(examId);
        //    if (exam == null) return NotFound("Exam not found");

        //    var sessions = await _unitOfWork.Repository<ExamSession>()
        //        .FindAsync(s => s.ExamId == examId);

        //    var result = new List<object>();

        //    foreach (var session in sessions)
        //    {
        //        var report = await _unitOfWork.Repository<ExamReport>()
        //            .FindOneAsync(r => r.SessionId == session.Id);

        //        var student = await _unitOfWork.Repository<Student>()
        //            .GetByIdAsync(session.StudentId);

        //        var studentUser = student != null
        //            ? await _unitOfWork.Repository<User>().GetByIdAsync(student.UserId)
        //            : null;

        //        result.Add(new
        //        {
        //            StudentName = studentUser?.FullName,
        //            StudentUniversityID = student?.StudentUniversityID,
        //            ExamCode = exam.Code,
        //            ExamSubject = exam.Subject,

        //            ScorePercent = report?.ScorePercent,
        //            CorrectAnswers = report?.CorrectAnswers,
        //            TotalQuestions = report?.TotalQuestions,
        //            RiskLevel = report?.RiskLevel,
        //            FinalRiskScore = report?.FinalRiskScore,
        //            Status = report?.Status
        //        });
        //    }

        //    var fileBytes = await _excelService.GenerateExamReportsAsync(result, exam);

        //    var fileName = $"ExamReport_{exam.Code}_{DateTime.UtcNow:yyyyMMdd}.xlsx";

        //    return File(fileBytes,
        //        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        //        fileName);
        //}


        [HttpGet("exam/{examId}/export")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> ExportExamReports(int examId)
        {
            var exam = await _unitOfWork.Repository<Exam>().GetByIdAsync(examId);
            if (exam == null) return NotFound("Exam not found");

            var sessions = await _unitOfWork.Repository<ExamSession>()
                .FindAsync(s => s.ExamId == examId);

            var result = new List<object>();

            foreach (var session in sessions)
            {
                var report = await _unitOfWork.Repository<ExamReport>()
                    .FindOneAsync(r => r.SessionId == session.Id);

                if (report != null && report.Status == "Pending")
                {
                    report.Status = "Completed"; // تحويل الحالة تلقائياً عند التصدير
                    _unitOfWork.Repository<ExamReport>().Update(report);
                }

                var student = await _unitOfWork.Repository<Student>()
                    .GetByIdAsync(session.StudentId);

                var studentUser = student != null
                    ? await _unitOfWork.Repository<User>().GetByIdAsync(student.UserId)
                    : null;

                result.Add(new
                {
                    StudentName = studentUser?.FullName,
                    StudentUniversityID = student?.StudentUniversityID,
                    ExamCode = exam.Code,
                    ExamSubject = exam.Subject,
                    ScorePercent = report?.ScorePercent,
                    CorrectAnswers = report?.CorrectAnswers,
                    TotalQuestions = report?.TotalQuestions,
                    CheatingCount = report?.CheatingCount ?? 0,
                    ScreenshotsUrl = report?.ScreenshotsUrl ?? "-",
                    Notes = report?.Notes ?? "-",
                    Status = report?.Status
                });
            }
            await _unitOfWork.CompleteAsync();

            var fileBytes = await _excelService.GenerateExamReportsAsync(result, exam);
            var fileName = $"ExamReport_{exam.Code}_{DateTime.UtcNow:yyyyMMdd}.xlsx";

            return File(fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

    }
}