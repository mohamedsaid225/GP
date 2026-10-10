using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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


        // فحص قبل فتح صفحة الامتحان أو طلب الكاميرا
        // لا ينشئ Session. يستخدم فقط لمعرفة هل الطالب مسموح له يدخل أم امتحن قبل كده.
        [HttpPost("precheck")]
        public async Task<IActionResult> Precheck(JoinExamDto dto)
        {
            var exam = await _unitOfWork.Repository<Exam>()
                .FindOneAsync(e => e.Code == dto.ExamCode && e.Status == "Active");

            if (exam == null)
            {
                return NotFound(new
                {
                    canJoin = false,
                    message = "Exam not found or not active"
                });
            }

            var now = DateTime.Now;

            if (exam.ActiveFrom.HasValue && now < exam.ActiveFrom.Value)
            {
                return BadRequest(new
                {
                    canJoin = false,
                    message = "Exam is not active yet",
                    activeFrom = exam.ActiveFrom
                });
            }

            if (exam.ActiveTo.HasValue && now > exam.ActiveTo.Value)
            {
                return BadRequest(new
                {
                    canJoin = false,
                    message = "Exam availability window has ended",
                    activeTo = exam.ActiveTo
                });
            }

            var student = await _unitOfWork.Repository<Student>()
                .FindOneAsync(s => s.StudentUniversityID == dto.StudentUniversityID, s => s.User);

            if (student == null)
            {
                return NotFound(new
                {
                    canJoin = false,
                    message = "Student not found"
                });
            }

            if (!student.Department.Equals(exam.Department, StringComparison.OrdinalIgnoreCase) ||
                !student.College.Equals(exam.College, StringComparison.OrdinalIgnoreCase) ||
                !student.University.Equals(exam.University, StringComparison.OrdinalIgnoreCase) ||
                student.Level != exam.Level)
            {
                return Unauthorized(new
                {
                    canJoin = false,
                    message = "You are not eligible for this exam"
                });
            }

            var existing = await _unitOfWork.Repository<ExamSession>()
                .FindOneAsync(s => s.ExamId == exam.Id && s.StudentId == student.Id);

            if (existing != null)
            {
                return BadRequest(new
                {
                    canJoin = false,
                    alreadyTaken = true,
                    message = "You have already taken this exam"
                });
            }

            return Ok(new
            {
                canJoin = true,
                examId = exam.Id,
                studentId = student.Id,
                studentName = student.User?.FullName,
                message = "Student can start this exam"
            });
        }

        // الطالب يدخل الامتحان
        [HttpPost("join")]
        public async Task<IActionResult> Join(JoinExamDto dto)
        {
            var exam = await _unitOfWork.Repository<Exam>()
                .FindOneAsync(e => e.Code == dto.ExamCode && e.Status == "Active");

            if (exam == null)
            {
                return NotFound("Exam not found or not active");
            }

            var now = DateTime.Now;

            if (exam.ActiveFrom.HasValue && now < exam.ActiveFrom.Value)
            {
                return BadRequest(new
                {
                    message = "Exam is not active yet",
                    activeFrom = exam.ActiveFrom
                });
            }

            if (exam.ActiveTo.HasValue && now > exam.ActiveTo.Value)
            {
                return BadRequest(new
                {
                    message = "Exam availability window has ended",
                    activeTo = exam.ActiveTo
                });
            }

            var student = await _unitOfWork.Repository<Student>()
                .FindOneAsync(s => s.StudentUniversityID == dto.StudentUniversityID, s => s.User);

            if (student == null)
            {
                return NotFound("Student not found");
            }

            //if (!student.Department.Equals(exam.Department, StringComparison.OrdinalIgnoreCase) ||
            //    !student.College.Equals(exam.College, StringComparison.OrdinalIgnoreCase) ||
            //    !student.University.Equals(exam.University, StringComparison.OrdinalIgnoreCase) ||
            //    student.Level != exam.Level)
            //{
            //    return Unauthorized("You are not eligible for this exam");
            //}

            string Normalize(string value)
            {
                return (value ?? "").Trim().ToLower();
            }

            if (Normalize(student.Department) != Normalize(exam.Department) ||
                Normalize(student.College) != Normalize(exam.College) ||
                Normalize(student.University) != Normalize(exam.University) ||
                student.Level != exam.Level)
            {
                return Unauthorized("You are not eligible for this exam");
            }

            var existing = await _unitOfWork.Repository<ExamSession>()
                .FindOneAsync(s => s.ExamId == exam.Id && s.StudentId == student.Id);

            if (existing != null)
            {
                return BadRequest(new
                {
                    message = "You have already taken this exam"
                });
            }

            var session = new ExamSession
            {
                ExamId = exam.Id,
                StudentId = student.Id,
                StartTime = DateTime.UtcNow,
                Status = "InProgress",
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<ExamSession>().AddAsync(session);
            await _unitOfWork.CompleteAsync();

            var auditLog = new AuditLog
            {
                UserId = student.UserId,
                Action = "JoinExam",
                IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Details = $"Student '{student.StudentUniversityID}' joined exam '{exam.Code}'",
                Timestamp = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<AuditLog>().AddAsync(auditLog);
            await _unitOfWork.CompleteAsync();

            var questions = await _unitOfWork.Repository<Question>()
                .FindAsync(q => q.ExamId == exam.Id);

            return Ok(new
            {
                SessionId = session.Id,
                ExamId = exam.Id,
                Subject = exam.Subject,
                DurationMinutes = exam.DurationMinutes,
                ActiveFrom = exam.ActiveFrom,
                ActiveTo = exam.ActiveTo,
                StudentName = student.User.FullName,
                StudentID = student.StudentUniversityID,
                Questions = questions
                    .OrderBy(q => q.OrderIndex)
                    .Select(q => new
                    {
                        q.Id,
                        q.QuestionText,
                        q.OptionA,
                        q.OptionB,
                        q.OptionC,
                        q.OptionD,
                        q.OrderIndex
                    })
            });
        }

        //[HttpPost("{sessionId}/ai-result")]
        //public async Task<IActionResult> ReceiveAiResult(int sessionId, AiResultDto dto)
        //{
        //    var session = await _unitOfWork.Repository<ExamSession>()
        //        .GetByIdAsync(sessionId);

        //    if (session == null)
        //    {
        //        return NotFound("Session not found");
        //    }

        //    var student = await _unitOfWork.Repository<Student>()
        //        .FindOneAsync(s => s.StudentUniversityID == dto.StudentUniversityID);

        //    if (student == null)
        //    {
        //        return NotFound("Student not found");
        //    }

        //    var studentUser = await _unitOfWork.Repository<User>()
        //        .GetByIdAsync(student.UserId);

        //    var screenshotsUrl = NormalizeScreenshotsUrl(dto.ScreenshotsUrl);

        //    var suspiciousEvent = new SuspiciousEvent
        //    {
        //        SessionId = sessionId,
        //        StudentUniversityID = dto.StudentUniversityID,
        //        StudentName = studentUser?.FullName ?? "Unknown",
        //        CheatingCount = dto.CheatingCount,
        //        ScreenshotsUrl = screenshotsUrl,
        //        Timestamp = DateTime.UtcNow,
        //        CreatedAt = DateTime.UtcNow
        //    };

        //    await _unitOfWork.Repository<SuspiciousEvent>().AddAsync(suspiciousEvent);

        //    // لو التقرير اتعمل قبل نتيجة الـ AI ما توصل، حدّث التقرير بنفس بيانات الـ AI.
        //    var report = await _unitOfWork.Repository<ExamReport>()
        //        .FindOneAsync(r => r.SessionId == sessionId);

        //    if (report != null)
        //    {
        //        // متقللش الرقم لو وصلت نتيجة قديمة/ناقصة، وخلي اللينك القديم لو الجديد فاضي.
        //        report.CheatingCount = Math.Max(report.CheatingCount, dto.CheatingCount);

        //        if (HasRealScreenshotsUrl(screenshotsUrl))
        //        {
        //            report.ScreenshotsUrl = screenshotsUrl;
        //        }
        //        else if (string.IsNullOrWhiteSpace(report.ScreenshotsUrl))
        //        {
        //            report.ScreenshotsUrl = "Not Available";
        //        }

        //        report.UpdatedAt = DateTime.UtcNow;
        //        _unitOfWork.Repository<ExamReport>().Update(report);
        //    }

        //    await _unitOfWork.CompleteAsync();

        //    return Ok(new
        //    {
        //        Message = "AI results saved successfully",
        //        SessionId = sessionId,
        //        CheatingCount = dto.CheatingCount,
        //        ScreenshotsUrl = screenshotsUrl
        //    });
        //}

        // الطالب يسلم الامتحان

        [HttpPost("{sessionId}/ai-result")]
        public async Task<IActionResult> ReceiveAiResult(int sessionId, AiResultDto dto)
        {
            var session = await _unitOfWork.Repository<ExamSession>()
                .GetByIdAsync(sessionId);

            if (session == null) return NotFound("Session not found");

            var student = await _unitOfWork.Repository<Student>()
                .FindOneAsync(s => s.StudentUniversityID == dto.StudentUniversityID);

            if (student == null) return NotFound("Student not found");

            var studentUser = await _unitOfWork.Repository<User>()
                .GetByIdAsync(student.UserId);

            var screenshotsUrl = NormalizeScreenshotsUrl(dto.ScreenshotsUrl);

            // شوف لو في event موجود لنفس الـ Session
            var existingEvent = await _unitOfWork.Repository<SuspiciousEvent>()
                .FindOneAsync(s => s.SessionId == sessionId);

            if (existingEvent != null)
            {
                // عدّل الموجود بدل ما تضيف جديد
                existingEvent.CheatingCount = Math.Max(existingEvent.CheatingCount, dto.CheatingCount);
                if (HasRealScreenshotsUrl(screenshotsUrl))
                    existingEvent.ScreenshotsUrl = screenshotsUrl;
                existingEvent.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Repository<SuspiciousEvent>().Update(existingEvent);
            }
            else
            {
                // ضيف جديد لو مفيش
                var suspiciousEvent = new SuspiciousEvent
                {
                    SessionId = sessionId,
                    StudentUniversityID = dto.StudentUniversityID,
                    StudentName = studentUser?.FullName ?? "Unknown",
                    CheatingCount = dto.CheatingCount,
                    ScreenshotsUrl = screenshotsUrl,
                    Timestamp = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.Repository<SuspiciousEvent>().AddAsync(suspiciousEvent);
            }

            // حدّث الـ Report لو موجود
            var report = await _unitOfWork.Repository<ExamReport>()
                .FindOneAsync(r => r.SessionId == sessionId);

            if (report != null)
            {
                report.CheatingCount = Math.Max(report.CheatingCount, dto.CheatingCount);
                if (HasRealScreenshotsUrl(screenshotsUrl))
                    report.ScreenshotsUrl = screenshotsUrl;
                report.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Repository<ExamReport>().Update(report);
            }

            await _unitOfWork.CompleteAsync();

            return Ok(new
            {
                Message = "AI results saved successfully",
                SessionId = sessionId,
                CheatingCount = dto.CheatingCount,
                ScreenshotsUrl = screenshotsUrl
            });
        }

        [HttpPost("{sessionId}/submit")]
        public async Task<IActionResult> Submit(int sessionId, SubmitExamDto dto)
        {
            var session = await _unitOfWork.Repository<ExamSession>()
                .GetByIdAsync(sessionId);

            if (session == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Session not found"
                });
            }

            if (session.Status != "InProgress")
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Session already submitted",
                    status = session.Status
                });
            }

            var examQuestions = (await _unitOfWork.Repository<Question>()
                .FindAsync(q => q.ExamId == session.ExamId))
                .OrderBy(q => q.OrderIndex)
                .ToList();

            if (!examQuestions.Any())
            {
                return BadRequest(new
                {
                    success = false,
                    message = "No questions found for this exam"
                });
            }

            var submittedAnswers = (dto.Answers ?? new List<SubmitAnswerDto>())
                .Where(a => a.QuestionId > 0)
                .GroupBy(a => a.QuestionId)
                .ToDictionary(g => g.Key, g => g.Last());

            var oldAnswers = await _unitOfWork.Repository<StudentAnswer>()
                .FindAsync(a => a.SessionId == sessionId);

            var oldAnswerQuestionIds = oldAnswers
                .Select(a => a.QuestionId)
                .ToHashSet();

            int correct = 0;
            int total = examQuestions.Count;

            foreach (var question in examQuestions)
            {
                submittedAnswers.TryGetValue(question.Id, out var submittedAnswer);

                var selectedAnswer = submittedAnswer?.SelectedAnswer;

                if (string.IsNullOrWhiteSpace(selectedAnswer))
                {
                    selectedAnswer = "Not Answered";
                }

                var isNotAnswered = selectedAnswer.Equals("Not Answered", StringComparison.OrdinalIgnoreCase);
                var isCorrect = !isNotAnswered &&
                                !string.IsNullOrWhiteSpace(question.CorrectAnswer) &&
                                question.CorrectAnswer.Equals(selectedAnswer, StringComparison.OrdinalIgnoreCase);

                if (isCorrect)
                {
                    correct++;
                }

                if (!oldAnswerQuestionIds.Contains(question.Id))
                {
                    var studentAnswer = new StudentAnswer
                    {
                        SessionId = sessionId,
                        QuestionId = question.Id,
                        SelectedAnswer = selectedAnswer,
                        IsCorrect = isCorrect,
                        AnsweredAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.Repository<StudentAnswer>().AddAsync(studentAnswer);
                }
            }

            session.Status = "Completed";
            session.EndTime = DateTime.UtcNow;
            session.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Repository<ExamSession>().Update(session);

            var exam = await _unitOfWork.Repository<Exam>().GetByIdAsync(session.ExamId);
            if (exam == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Exam not found for this session"
                });
            }

            var instructor = await _unitOfWork.Repository<Instructor>().GetByIdAsync(exam.InstructorId);
            var student = await _unitOfWork.Repository<Student>().GetByIdAsync(session.StudentId);

            if (instructor == null || student == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Instructor or student data was not found"
                });
            }

            var studentUser = await _unitOfWork.Repository<User>().GetByIdAsync(student.UserId);
            var instructorUser = await _unitOfWork.Repository<User>().GetByIdAsync(instructor.UserId);

            var sessionEvents = (await _unitOfWork.Repository<SuspiciousEvent>()
                .FindAsync(e => e.SessionId == sessionId))
                .OrderByDescending(e => e.Timestamp)
                .ToList();

            var latestAiEvent = sessionEvents.FirstOrDefault();
            var latestScreenshotsEvent = sessionEvents
                .FirstOrDefault(e => HasRealScreenshotsUrl(e.ScreenshotsUrl));

            // استخدم أكبر CheatingCount محفوظ للـ session عشان نتيجة متأخرة أو ناقصة متصفرش التقرير.
            var cheatingCount = sessionEvents.Any() ? sessionEvents.Max(e => e.CheatingCount) : 0;
            var screenshotsUrl = NormalizeScreenshotsUrl(latestScreenshotsEvent?.ScreenshotsUrl ?? latestAiEvent?.ScreenshotsUrl);

            // بعد ما تخلص الـ answers وقبل ما تعمل الـ Report
            var existingEvent = await _unitOfWork.Repository<SuspiciousEvent>()
                .FindOneAsync(s => s.SessionId == sessionId);

            if (existingEvent == null)
            {
                // الـ AI مبعتش — اعمل Event بالبيانات المتاحة من الفرونت
                var suspiciousEvent = new SuspiciousEvent
                {
                    SessionId = sessionId,
                    StudentUniversityID = student.StudentUniversityID,
                    StudentName = studentUser?.FullName ?? "Unknown",
                    CheatingCount = 0,               // الـ AI لسه مبعتش
                    LeavingExamCount = dto.LeavingExam,
                    ScreenshotsUrl = "Not Available", // الـ AI لسه مبعتش
                    Timestamp = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.Repository<SuspiciousEvent>().AddAsync(suspiciousEvent);
            }
            else
            {
                // الـ AI بعت بالفعل — ضيف بس الـ LeavingExamCount
                existingEvent.LeavingExamCount = dto.LeavingExam;
                existingEvent.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Repository<SuspiciousEvent>().Update(existingEvent);
            }


            var existingReport = await _unitOfWork.Repository<ExamReport>()
                .FindOneAsync(r => r.SessionId == sessionId);

            var scorePercent = total > 0 ? Math.Round((decimal)correct / total * 100, 2) : 0;

            if (existingReport == null)
            {
                var report = new ExamReport
                {
                    SessionId = sessionId,
                    InstructorId = instructor.Id,

                    StudentName = studentUser?.FullName ?? "Unknown",
                    StudentUniversityId = student.StudentUniversityID,

                    InstructorName = instructorUser?.FullName ?? "Unknown",
                    InstructorCode = instructor.InstructorCode,

                    TotalQuestions = total,
                    CorrectAnswers = correct,
                    ScorePercent = scorePercent,

                    CheatingCount = cheatingCount,
                    ScreenshotsUrl = screenshotsUrl,

                    Status = "Pending",
                    Notes = $"Leaving Exam Counter: {dto.LeavingExam}.",

                    GeneratedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Repository<ExamReport>().AddAsync(report);
                existingReport = report;
            }
            else
            {
                existingReport.TotalQuestions = total;
                existingReport.CorrectAnswers = correct;
                existingReport.ScorePercent = scorePercent;
                existingReport.CheatingCount = cheatingCount;
                existingReport.ScreenshotsUrl = screenshotsUrl;
                existingReport.Notes = $"Leaving Exam Counter: {dto.LeavingExam}.";
                existingReport.Status = string.IsNullOrWhiteSpace(existingReport.Status) ? "Pending" : existingReport.Status;
                existingReport.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Repository<ExamReport>().Update(existingReport);
            }

            await _unitOfWork.CompleteAsync();

            return Ok(new
            {
                success = true,
                SessionId = sessionId,
                TotalQuestions = total,
                CorrectAnswers = correct,
                ScorePercent = existingReport.ScorePercent,
                CheatingCount = existingReport.CheatingCount,
                ScreenshotsUrl = existingReport.ScreenshotsUrl,
                LeavingExam = dto.LeavingExam
            });
        }

        //[HttpPost("{sessionId}/submit")]
        //public async Task<IActionResult> Submit(int sessionId, SubmitExamDto dto)
        //{
        //    var session = await _unitOfWork.Repository<ExamSession>()
        //        .GetByIdAsync(sessionId);

        //    if (session == null)
        //    {
        //        return NotFound(new
        //        {
        //            success = false,
        //            message = "Session not found"
        //        });
        //    }

        //    if (session.Status != "InProgress")
        //    {
        //        return BadRequest(new
        //        {
        //            success = false,
        //            message = "Session already submitted",
        //            status = session.Status
        //        });
        //    }

        //    var examQuestions = (await _unitOfWork.Repository<Question>()
        //        .FindAsync(q => q.ExamId == session.ExamId))
        //        .OrderBy(q => q.OrderIndex)
        //        .ToList();

        //    if (!examQuestions.Any())
        //    {
        //        return BadRequest(new
        //        {
        //            success = false,
        //            message = "No questions found for this exam"
        //        });
        //    }

        //    var submittedAnswers = (dto.Answers ?? new List<SubmitAnswerDto>())
        //        .Where(a => a.QuestionId > 0)
        //        .GroupBy(a => a.QuestionId)
        //        .ToDictionary(g => g.Key, g => g.Last());

        //    var oldAnswers = await _unitOfWork.Repository<StudentAnswer>()
        //        .FindAsync(a => a.SessionId == sessionId);

        //    var oldAnswerQuestionIds = oldAnswers
        //        .Select(a => a.QuestionId)
        //        .ToHashSet();

        //    int correct = 0;
        //    int total = examQuestions.Count;

        //    foreach (var question in examQuestions)
        //    {
        //        submittedAnswers.TryGetValue(question.Id, out var submittedAnswer);

        //        var selectedAnswer = submittedAnswer?.SelectedAnswer;

        //        if (string.IsNullOrWhiteSpace(selectedAnswer))
        //        {
        //            selectedAnswer = "Not Answered";
        //        }

        //        var isNotAnswered = selectedAnswer.Equals("Not Answered", StringComparison.OrdinalIgnoreCase);
        //        var isCorrect = !isNotAnswered &&
        //                        !string.IsNullOrWhiteSpace(question.CorrectAnswer) &&
        //                        question.CorrectAnswer.Equals(selectedAnswer, StringComparison.OrdinalIgnoreCase);

        //        if (isCorrect)
        //        {
        //            correct++;
        //        }

        //        if (!oldAnswerQuestionIds.Contains(question.Id))
        //        {
        //            var studentAnswer = new StudentAnswer
        //            {
        //                SessionId = sessionId,
        //                QuestionId = question.Id,
        //                SelectedAnswer = selectedAnswer,
        //                IsCorrect = isCorrect,
        //                AnsweredAt = DateTime.UtcNow,
        //                CreatedAt = DateTime.UtcNow
        //            };

        //            await _unitOfWork.Repository<StudentAnswer>().AddAsync(studentAnswer);
        //        }
        //    }

        //    session.Status = "Completed";
        //    session.EndTime = DateTime.UtcNow;
        //    session.UpdatedAt = DateTime.UtcNow;
        //    _unitOfWork.Repository<ExamSession>().Update(session);

        //    var exam = await _unitOfWork.Repository<Exam>().GetByIdAsync(session.ExamId);
        //    if (exam == null)
        //    {
        //        return BadRequest(new
        //        {
        //            success = false,
        //            message = "Exam not found for this session"
        //        });
        //    }

        //    var instructor = await _unitOfWork.Repository<Instructor>().GetByIdAsync(exam.InstructorId);
        //    var student = await _unitOfWork.Repository<Student>().GetByIdAsync(session.StudentId);

        //    if (instructor == null || student == null)
        //    {
        //        return BadRequest(new
        //        {
        //            success = false,
        //            message = "Instructor or student data was not found"
        //        });
        //    }

        //    var studentUser = await _unitOfWork.Repository<User>().GetByIdAsync(student.UserId);
        //    var instructorUser = await _unitOfWork.Repository<User>().GetByIdAsync(instructor.UserId);

        //    var sessionEvents = (await _unitOfWork.Repository<SuspiciousEvent>()
        //        .FindAsync(e => e.SessionId == sessionId))
        //        .OrderByDescending(e => e.Timestamp)
        //        .ToList();

        //    var latestAiEvent = sessionEvents.FirstOrDefault();
        //    var latestScreenshotsEvent = sessionEvents
        //        .FirstOrDefault(e => HasRealScreenshotsUrl(e.ScreenshotsUrl));

        //    // استخدم أكبر CheatingCount محفوظ للـ session عشان نتيجة متأخرة أو ناقصة متصفرش التقرير.
        //    var cheatingCount = sessionEvents.Any() ? sessionEvents.Max(e => e.CheatingCount) : 0;
        //    var screenshotsUrl = NormalizeScreenshotsUrl(latestScreenshotsEvent?.ScreenshotsUrl ?? latestAiEvent?.ScreenshotsUrl);

        //    var existingReport = await _unitOfWork.Repository<ExamReport>()
        //        .FindOneAsync(r => r.SessionId == sessionId);

        //    var scorePercent = total > 0 ? Math.Round((decimal)correct / total * 100, 2) : 0;

        //    if (existingReport == null)
        //    {
        //        var report = new ExamReport
        //        {
        //            SessionId = sessionId,
        //            InstructorId = instructor.Id,

        //            StudentName = studentUser?.FullName ?? "Unknown",
        //            StudentUniversityId = student.StudentUniversityID,

        //            InstructorName = instructorUser?.FullName ?? "Unknown",
        //            InstructorCode = instructor.InstructorCode,

        //            TotalQuestions = total,
        //            CorrectAnswers = correct,
        //            ScorePercent = scorePercent,

        //            CheatingCount = cheatingCount,
        //            ScreenshotsUrl = screenshotsUrl,

        //            Status = "Pending",
        //            Notes = $"Leaving Exam Counter: {dto.LeavingExam}.",

        //            GeneratedAt = DateTime.UtcNow,
        //            CreatedAt = DateTime.UtcNow
        //        };

        //        await _unitOfWork.Repository<ExamReport>().AddAsync(report);
        //        existingReport = report;
        //    }
        //    else
        //    {
        //        existingReport.TotalQuestions = total;
        //        existingReport.CorrectAnswers = correct;
        //        existingReport.ScorePercent = scorePercent;
        //        existingReport.CheatingCount = cheatingCount;
        //        existingReport.ScreenshotsUrl = screenshotsUrl;
        //        existingReport.Notes = $"Leaving Exam Counter: {dto.LeavingExam}.";
        //        existingReport.Status = string.IsNullOrWhiteSpace(existingReport.Status) ? "Pending" : existingReport.Status;
        //        existingReport.UpdatedAt = DateTime.UtcNow;

        //        _unitOfWork.Repository<ExamReport>().Update(existingReport);
        //    }

        //    await _unitOfWork.CompleteAsync();

        //    return Ok(new
        //    {
        //        success = true,
        //        SessionId = sessionId,
        //        TotalQuestions = total,
        //        CorrectAnswers = correct,
        //        ScorePercent = existingReport.ScorePercent,
        //        CheatingCount = existingReport.CheatingCount,
        //        ScreenshotsUrl = existingReport.ScreenshotsUrl,
        //        LeavingExam = dto.LeavingExam
        //    });
        //}

        // الدكتور يشوف Sessions
        
        
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var sessions = await _unitOfWork.Repository<ExamSession>()
                .GetAllAsync(s => s.Student.User, s => s.Exam);

            var result = sessions.Select(s => new
            {
                s.Id,
                StudentName = s.Student.User.FullName,
                ExamSubject = s.Exam.Subject,
                s.Status,
                s.StartTime
            });

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var session = await _unitOfWork.Repository<ExamSession>()
                .GetByIdAsync(id,
                    s => s.Student.User,
                    s => s.Exam
                );

            if (session == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                session.Id,
                session.ExamId,
                ExamSubject = session.Exam?.Subject,
                ExamDuration = session.Exam?.DurationMinutes,
                session.Status,
                session.StartTime,
                StudentName = session.Student?.User != null ? session.Student.User.FullName : "Unknown",
                UniversityId = session.Student?.StudentUniversityID,
            });
        }

        private static bool HasRealScreenshotsUrl(string? screenshotsUrl)
        {
            if (string.IsNullOrWhiteSpace(screenshotsUrl)) return false;

            var value = screenshotsUrl.Trim();
            return !value.Equals("Not Available", StringComparison.OrdinalIgnoreCase)
                   && value != "-";
        }

        private static string NormalizeScreenshotsUrl(string? screenshotsUrl)
        {
            if (string.IsNullOrWhiteSpace(screenshotsUrl))
            {
                return "Not Available";
            }

            return screenshotsUrl.Trim();
        }
    }
}