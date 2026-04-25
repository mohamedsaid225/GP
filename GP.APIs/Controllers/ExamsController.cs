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
                        InstructorName = user != null ? $"{user.FullName}" : "Unknown"
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
                    Status = "Inactive", // هبقي اعدلها لما عمرو يخلص ال Button
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
                        exam.Status,
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
            //[HttpPut("{id}")]
            //[Authorize(Roles = "Instructor")]
            //public async Task<IActionResult> Update(int id, CreateExamDto dto)
            //{
            //    var exam = await _unitOfWork.Repository<Exam>().GetByIdAsync(id);
            //    if (exam == null) return NotFound();

            //    exam.Code = dto.Code;
            //    exam.Subject = dto.Subject;
            //    exam.Department = dto.Department;
            //    exam.University = dto.University;
            //    exam.College = dto.College;
            //    exam.Level = dto.Level;
            //    exam.DurationMinutes = dto.DurationMinutes;
            //    exam.UpdatedAt = DateTime.UtcNow;

            //    _unitOfWork.Repository<Exam>().Update(exam);
            //    await _unitOfWork.CompleteAsync();

            //    return NoContent();
            //}

            [HttpPut("{id}")]
            [Authorize(Roles = "Instructor")]
            public async Task<IActionResult> Update(int id, CreateExamDto dto)
            {
                var exam = await _unitOfWork.Repository<Exam>().GetByIdAsync(id);
                if (exam == null) return NotFound();

                // تأكد إن الدكتور صاحب الامتحان
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                var userId = int.Parse(userIdClaim.Value);
                var instructor = await _unitOfWork.Repository<Instructor>()
                    .FindOneAsync(i => i.UserId == userId);

                if (instructor == null || exam.InstructorId != instructor.Id)
                    return Unauthorized("You don't own this exam");

                // عدّل بيانات الامتحان
                exam.Code = dto.Code;
                exam.Subject = dto.Subject;
                exam.Department = dto.Department;
                exam.University = dto.University;
                exam.College = dto.College;
                exam.Level = dto.Level;
                exam.DurationMinutes = dto.DurationMinutes;
                exam.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Repository<Exam>().Update(exam);

                // لو في أسئلة جديدة
                if (dto.Questions != null && dto.Questions.Any())
                {
                    // امسح الأسئلة القديمة
                    var oldQuestions = await _unitOfWork.Repository<Question>()
                        .FindAsync(q => q.ExamId == id);

                    foreach (var q in oldQuestions)
                        _unitOfWork.Repository<Question>().Delete(q);

                    // ضيف الأسئلة الجديدة
                    var newQuestions = dto.Questions.Select(q => new Question
                    {
                        ExamId = id,
                        QuestionText = q.QuestionText,
                        OptionA = q.OptionA,
                        OptionB = q.OptionB,
                        OptionC = q.OptionC,
                        OptionD = q.OptionD,
                        CorrectAnswer = q.CorrectAnswer.ToUpper(),
                        OrderIndex = q.OrderIndex,
                        CreatedAt = DateTime.UtcNow
                    }).ToList();

                    await _unitOfWork.Repository<Question>().AddRangeAsync(newQuestions);
                }

                await _unitOfWork.CompleteAsync();
                return NoContent();
            }

            //الدكتور يغير Status الامتحان
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
                var userId = int.Parse(userIdClaim.Value);
                var instructor = await _unitOfWork.Repository<Instructor>()
                    .FindOneAsync(i => i.UserId == userId);

                if (instructor == null || exam.InstructorId != instructor.Id)
                    return Unauthorized("You don't own this exam");

                exam.Status = status;
                exam.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Repository<Exam>().Update(exam);
                await _unitOfWork.CompleteAsync();

                return Ok(new { Message = $"Exam status changed to {status}", ExamId = id });
            }


        //[HttpGet("{id}")]
        //public async Task<IActionResult> GetById(int id)
        //{
        //    var exam = await _unitOfWork.Repository<Exam>().GetByIdAsync(id);
        //    if (exam == null) return NotFound();
        //    return Ok(exam);
        //}


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            // 1. جلب الامتحان مع الأسئلة، المدرس، والسيشنز (بدون الطلاب لأن الـ Include مش واصلالهم)
            var exam = await _unitOfWork.Repository<Exam>().GetByIdAsync(
                id,
                e => e.Questions,
                e => e.Instructor.User,
                e => e.Sessions
            );

            if (exam == null) return NotFound();

            // 2. مصفوفة (List) عشان نجمع فيها بيانات كل سيشن مع طالبها
            var sessionsWithStudents = new List<object>();

            if (exam.Sessions != null)
            {
                foreach (var session in exam.Sessions)
                {
                    // بنروح نجيب الطالب واليوزر بتاعه لكل سيشن بشكل منفصل
                    var student = await _unitOfWork.Repository<Student>()
                        .FindOneAsync(s => s.Id == session.StudentId, s => s.User);

                    sessionsWithStudents.Add(new
                    {
                        session.Id,
                        session.Status,
                        session.StartTime,
                        session.EndTime,
                        // بيانات الطالب
                        StudentInfo = new
                        {
                            Id = student?.Id,
                            UniversityId = student?.StudentUniversityID,
                            FullName = student?.User != null
                                ? $"{student.User.FullName}"
                                : "Unknown"
                        }
                    });
                }
            }

            // 3. بناء الـ Result النهائي اللي فيه "كل حاجة"
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
                exam.Status,
                Instructor = new
                {
                    Id = exam.InstructorId,
                    Name = exam.Instructor?.User != null
                        ? $"{exam.Instructor.User.FullName}"
                        : "Unknown"
                },
                // عرض كل الأسئلة
                Questions = exam.Questions.Select(q => new
                {
                    q.Id,
                    q.QuestionText,
                    q.OptionA,
                    q.OptionB,
                    q.OptionC,
                    q.OptionD,
                    q.CorrectAnswer,
                    q.OrderIndex
                }),
                // عرض كل الطلاب اللي في السيشينز (اللي جهزناهم فوق)
                Sessions = sessionsWithStudents
            };

            // 4. أهم خطوة: ترجع الـ result اللي فيه كل الداتا
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

                return NoContent();
            }



            // Endpoint لترجيع الطلاب المسجلين في امتحان معين فقط
            //[HttpGet("{id}/enrolled-students")]
            //public async Task<IActionResult> GetEnrolledStudents(int id)
            //{
            //   // 1. تأكد إن الامتحان موجود أصلاً
            //   var exam = await _unitOfWork.Repository<Exam>().GetByIdAsync(id);
            //   if (exam == null) return NotFound("Exam not found");

            //   // 2. هنجيب كل الـ Enrollments الخاصة بالامتحان ده
            //   // وبما إننا محتاجين بيانات الطالب، هنستخدم الـ FindAsync مع التصفية
            //    var enrollments = await _unitOfWork.Repository<Enrollment>()
            //                                       .FindAsync(e => e.ExamId == id);

            //    var studentIds = enrollments.Select(e => e.StudentId).ToList();

            //    // 3. هنجيب بيانات الطلاب بناءً على الـ IDs اللي طلعناها
            //    var studentsResult = new List<object>();

            //    foreach (var studentId in studentIds)
            //    {
            //       var student = await _unitOfWork.Repository<Student>().GetByIdAsync(studentId);
            //       if (student != null)
            //       {
            //         studentsResult.Add(new
            //         {
            //            student.Id,
            //            student.StudentUniversityID, // الرقم اللي الفرونت إند بيعمل عليه Validation
            //            student.Department,
            //            student.Level
            //         });
            //       }
            //    }

            //    return Ok(studentsResult);
            //}




        }
    }