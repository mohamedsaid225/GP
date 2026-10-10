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

        private static DateTime GetScheduleNow()
        {
            // ActiveFrom / ActiveTo are stored as the local exam schedule selected by the doctor.
            // Do not compare them with UtcNow, otherwise the exam appears/starts hours late or early.
            return DateTime.Now;
        }

        private static DateTime? NormalizeScheduleTime(DateTime? value)
        {
            if (!value.HasValue) return null;

            var dateTime = value.Value;

            // If an older frontend sends a UTC ISO value, convert it back to local time once.
            if (dateTime.Kind == DateTimeKind.Utc)
            {
                dateTime = dateTime.ToLocalTime();
            }

            return DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
        }

        private static bool IsInsideAvailabilityWindow(Exam exam)
        {
            var now = GetScheduleNow();

            if (exam.ActiveFrom.HasValue && now < exam.ActiveFrom.Value) return false;
            if (exam.ActiveTo.HasValue && now > exam.ActiveTo.Value) return false;

            return true;
        }

        private IActionResult? ValidateAvailabilityWindow(DateTime? activeFrom, DateTime? activeTo)
        {
            // Allow old exams/old clients to omit both values, but never allow only one side.
            if (activeFrom.HasValue != activeTo.HasValue)
            {
                return BadRequest(new
                {
                    message = "Both activeFrom and activeTo are required when setting exam availability."
                });
            }

            if (activeFrom.HasValue && activeTo.HasValue && activeTo.Value <= activeFrom.Value)
            {
                return BadRequest(new
                {
                    message = "activeTo must be after activeFrom."
                });
            }

            return null;
        }

        private static ExamDto ToExamDto(Exam exam, string instructorName)
        {
            return new ExamDto
            {
                Id = exam.Id,
                Code = exam.Code,
                Subject = exam.Subject,
                Department = exam.Department,
                University = exam.University,
                College = exam.College,
                Level = exam.Level,
                DurationMinutes = exam.DurationMinutes,
                ActiveFrom = exam.ActiveFrom,
                ActiveTo = exam.ActiveTo,
                Status = exam.Status,
                InstructorName = instructorName
            };
        }

        // الطالب يشوف الامتحانات المتاحة أو المجدولة.
        // تظهر قبل ActiveFrom كـ Scheduled، وتختفي فقط بعد ActiveTo أو لو الدكتور عملها Inactive.
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailable()
        {
            var now = GetScheduleNow();

            var exams = await _unitOfWork.Repository<Exam>()
                .FindAsync(e => e.Status == "Active"
                    && (!e.ActiveTo.HasValue || e.ActiveTo.Value >= now));

            var result = new List<ExamDto>();

            foreach (var exam in exams)
            {
                var instructor = await _unitOfWork.Repository<Instructor>()
                    .GetByIdAsync(exam.InstructorId);

                var user = instructor != null
                    ? await _unitOfWork.Repository<User>().GetByIdAsync(instructor.UserId)
                    : null;

                result.Add(ToExamDto(exam, user != null ? user.FullName : "Unknown"));
            }

            return Ok(result);
        }

        // الدكتور يضيف امتحان
        [HttpPost]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Create(CreateExamDto dto)
        {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var activeFrom = NormalizeScheduleTime(dto.ActiveFrom);
            var activeTo = NormalizeScheduleTime(dto.ActiveTo);

            var availabilityError = ValidateAvailabilityWindow(activeFrom, activeTo);
            if (availabilityError != null) return availabilityError;

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            var userId = int.Parse(userIdClaim.Value);
            var instructor = await _unitOfWork.Repository<Instructor>()
                .FindOneAsync(i => i.UserId == userId);

            if (instructor == null) return Unauthorized("Not an instructor");

            if (!dto.University.Trim().Equals(instructor.University.Trim(), StringComparison.OrdinalIgnoreCase) ||
                !dto.College.Trim().Equals(instructor.College.Trim(), StringComparison.OrdinalIgnoreCase) ||
                !dto.Department.Trim().Equals(instructor.Department.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    message = "Exam university, college, and department must match your profile",
                    yourUniversity = instructor.University,
                    yourCollege = instructor.College,
                    yourDepartment = instructor.Department
                });
            }

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
                ActiveFrom = activeFrom,
                ActiveTo = activeTo,
                // Active means the schedule can control visibility. Outside the window it still will not appear to students.
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<Exam>().AddAsync(exam);
            await _unitOfWork.CompleteAsync();

            var auditLog = new AuditLog
            {
                UserId = userId,
                Action = "CreateExam",
                IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Details = $"Exam '{exam.Code}' created with active window from {exam.ActiveFrom:u} to {exam.ActiveTo:u}",
                Timestamp = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Repository<AuditLog>().AddAsync(auditLog);
            await _unitOfWork.CompleteAsync();

            return CreatedAtAction(nameof(GetById), new { id = exam.Id }, ToExamDto(exam, User.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value ?? "Unknown"));
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

            var result = new List<object>();

            foreach (var exam in exams)
            {
                var questions = await _unitOfWork.Repository<Question>()
                    .FindAsync(q => q.ExamId == exam.Id);

                result.Add(new
                {
                    exam.Id,
                    exam.Code,
                    exam.Subject,
                    exam.Department,
                    exam.University,
                    exam.College,
                    exam.Level,
                    exam.DurationMinutes,
                    exam.ActiveFrom,
                    exam.ActiveTo,
                    exam.Status,
                    IsCurrentlyAvailable = exam.Status == "Active" && IsInsideAvailabilityWindow(exam),
                    exam.CreatedAt,
                    Questions = questions.Select(q => new
                    {
                        q.Id,
                        q.QuestionText,
                        q.OptionA,
                        q.OptionB,
                        q.OptionC,
                        q.OptionD,
                        q.CorrectAnswer,
                        q.OrderIndex
                    })
                });
            }

            return Ok(result);
        }

        // الدكتور يعدل امتحان
        [HttpPut("{id}")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Update(int id, CreateExamDto dto)
        {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var activeFrom = NormalizeScheduleTime(dto.ActiveFrom);
            var activeTo = NormalizeScheduleTime(dto.ActiveTo);

            var availabilityError = ValidateAvailabilityWindow(activeFrom, activeTo);
            if (availabilityError != null) return availabilityError;

            var exam = await _unitOfWork.Repository<Exam>().GetByIdAsync(id);
            if (exam == null) return NotFound();

            // تأكد إن الدكتور صاحب الامتحان
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            var userId = int.Parse(userIdClaim.Value);
            var instructor = await _unitOfWork.Repository<Instructor>()
                .FindOneAsync(i => i.UserId == userId);

            if (instructor == null || exam.InstructorId != instructor.Id)
                return Unauthorized("You don't own this exam");

            if (!dto.University.Trim().Equals(instructor.University.Trim(), StringComparison.OrdinalIgnoreCase) ||
                !dto.College.Trim().Equals(instructor.College.Trim(), StringComparison.OrdinalIgnoreCase) ||
                !dto.Department.Trim().Equals(instructor.Department.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    message = "Exam university, college, and department must match your profile",
                    yourUniversity = instructor.University,
                    yourCollege = instructor.College,
                    yourDepartment = instructor.Department
                });
            }

            // عدّل بيانات الامتحان الأساسية
            exam.Code = dto.Code;
            exam.Subject = dto.Subject;
            exam.Department = dto.Department;
            exam.University = dto.University;
            exam.College = dto.College;
            exam.Level = dto.Level;
            exam.DurationMinutes = dto.DurationMinutes;
            exam.ActiveFrom = activeFrom;
            exam.ActiveTo = activeTo;
            exam.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<Exam>().Update(exam);

            if (dto.Questions != null && dto.Questions.Any())
            {
                var existingQuestions = (await _unitOfWork.Repository<Question>()
                    .FindAsync(q => q.ExamId == id))
                    .OrderBy(q => q.OrderIndex)
                    .ThenBy(q => q.Id)
                    .ToList();

                var incomingQuestions = dto.Questions
                    .OrderBy(q => q.OrderIndex)
                    .ToList();

                for (int i = 0; i < incomingQuestions.Count; i++)
                {
                    var incoming = incomingQuestions[i];

                    if (i < existingQuestions.Count)
                    {
                        // عدّل السؤال الموجود مكانه بدل ما تمسحه
                        var existing = existingQuestions[i];

                        existing.QuestionText = incoming.QuestionText;
                        existing.OptionA = incoming.OptionA;
                        existing.OptionB = incoming.OptionB;
                        existing.OptionC = incoming.OptionC;
                        existing.OptionD = incoming.OptionD;
                        existing.CorrectAnswer = (incoming.CorrectAnswer ?? string.Empty).Trim().ToUpper();
                        existing.OrderIndex = incoming.OrderIndex;
                        existing.UpdatedAt = DateTime.UtcNow;

                        _unitOfWork.Repository<Question>().Update(existing);
                    }
                    else
                    {
                        // ضيف الأسئلة الجديدة فقط
                        var newQuestion = new Question
                        {
                            ExamId = id,
                            QuestionText = incoming.QuestionText,
                            OptionA = incoming.OptionA,
                            OptionB = incoming.OptionB,
                            OptionC = incoming.OptionC,
                            OptionD = incoming.OptionD,
                            CorrectAnswer = (incoming.CorrectAnswer ?? string.Empty).Trim().ToUpper(),
                            OrderIndex = incoming.OrderIndex,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        await _unitOfWork.Repository<Question>().AddAsync(newQuestion);
                    }
                }

                // لو الدكتور قلّل عدد الأسئلة:
                // احذف السؤال الزائد فقط لو مفيش عليه إجابات طلاب.
                // لو عليه StudentAnswers، سيبه عشان SQL Server مايرجعش FK error.
                var removedQuestions = existingQuestions
                    .Skip(incomingQuestions.Count)
                    .ToList();

                foreach (var removedQuestion in removedQuestions)
                {
                    var hasStudentAnswers = (await _unitOfWork.Repository<StudentAnswer>()
                        .FindAsync(a => a.QuestionId == removedQuestion.Id))
                        .Any();

                    if (!hasStudentAnswers)
                    {
                        _unitOfWork.Repository<Question>().Delete(removedQuestion);
                    }
                }
            }

            await _unitOfWork.CompleteAsync();
            return NoContent();
        }

        // الدكتور يغير Status الامتحان
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> ChangeStatus(int id, [FromBody] string status)
        {
            var validStatuses = new[] { "Active", "Inactive", "Draft" };
            if (!validStatuses.Contains(status))
                return BadRequest("Status must be Active, Inactive, or Draft");

            var exam = await _unitOfWork.Repository<Exam>().GetByIdAsync(id);
            if (exam == null) return NotFound();

            // تأكد إن الدكتور ده هو صاحب الامتحان
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            var userId = int.Parse(userIdClaim.Value);
            var instructor = await _unitOfWork.Repository<Instructor>()
                .FindOneAsync(i => i.UserId == userId);

            if (instructor == null || exam.InstructorId != instructor.Id)
                return Unauthorized("You don't own this exam");

            exam.Status = status;
            exam.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<Exam>().Update(exam);
            await _unitOfWork.CompleteAsync();

            return Ok(new
            {
                Message = $"Exam status changed to {status}",
                ExamId = id,
                exam.ActiveFrom,
                exam.ActiveTo,
                IsCurrentlyAvailable = exam.Status == "Active" && IsInsideAvailabilityWindow(exam)
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {

            //var sessionIdHeader = Request.Headers["Session-Id"].FirstOrDefault();

            //if (string.IsNullOrEmpty(sessionIdHeader))
            //{
            //    return Unauthorized("Session ID is required");
            //}

            //if (!int.TryParse(sessionIdHeader, out int sessionId))
            //{
            //    return Unauthorized("Invalid Session ID");
            //}

            //var examsession = await _unitOfWork.Repository<ExamSession>()
            //    .FindOneAsync(s => s.Id == sessionId && s.ExamId == id);

            //if (examsession == null)
            //{
            //    return Unauthorized("Invalid exam session");
            //}

            var exam = await _unitOfWork.Repository<Exam>().GetByIdAsync(
                id,
                e => e.Questions,
                e => e.Instructor.User,
                e => e.Sessions
            );

            if (exam == null) return NotFound();

            var sessionsWithStudents = new List<object>();

            if (exam.Sessions != null)
            {
                foreach (var session in exam.Sessions)
                {
                    var student = await _unitOfWork.Repository<Student>()
                        .FindOneAsync(s => s.Id == session.StudentId, s => s.User);

                    sessionsWithStudents.Add(new
                    {
                        session.Id,
                        session.Status,
                        session.StartTime,
                        session.EndTime,
                        StudentInfo = new
                        {
                            Id = student?.Id,
                            UniversityId = student?.StudentUniversityID,
                            FullName = student?.User != null
                                ? student.User.FullName
                                : "Unknown"
                        }
                    });
                }
            }

            var result = new
            {
                exam.Id,
                exam.Code,
                exam.Subject,
                exam.Department,
                exam.College,
                exam.University,
                exam.Level,
                exam.DurationMinutes,
                exam.ActiveFrom,
                exam.ActiveTo,
                exam.Status,
                IsCurrentlyAvailable = exam.Status == "Active" && IsInsideAvailabilityWindow(exam),
                Instructor = new
                {
                    Id = exam.InstructorId,
                    Name = exam.Instructor?.User != null
                        ? exam.Instructor.User.FullName
                        : "Unknown"
                },
                Questions = exam.Questions.Select(q => new
                {
                    q.Id,
                    q.QuestionText,
                    q.OptionA,
                    q.OptionB,
                    q.OptionC,
                    q.OptionD,
                    //q.CorrectAnswer,
                    q.OrderIndex
                }),
                Sessions = sessionsWithStudents
            };

            return Ok(result);
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

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            var userId = int.Parse(userIdClaim.Value);

            var auditLog = new AuditLog
            {
                UserId = userId,
                Action = "DeleteExam",
                IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Details = $"Exam '{exam.Code}' deleted",
                Timestamp = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Repository<AuditLog>().AddAsync(auditLog);
            await _unitOfWork.CompleteAsync();

            return NoContent();
        }
    }
}
