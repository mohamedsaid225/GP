//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using GP.Core.Entities;
//using GP.Core.Interfaces;

//namespace GP.Service
//{
//    public class ExamEnrollmentService : IExamEnrollmentService
//    {
//        private readonly IUnitOfWork _unitOfWork;

//        public ExamEnrollmentService(IUnitOfWork unitOfWork)
//        {
//            _unitOfWork = unitOfWork;
//        }

//        public async Task<bool> EnrollStudentInExamAsync(string studentUserId, string examCode)
//        {
//            // 1. هات بيانات الطالب (بما فيها الجامعة والكلية والقسم)
//            var student = await _unitOfWork.Repository<Student>().GetByIdAsync(s => s.UserId == studentUserId);

//            // 2. هات بيانات الامتحان عن طريق الـ Code واعمل Include للـ Instructor
//            var exam = await _unitOfWork.Repository<Exam>().GetWithIncludeAsync(
//                e => e.Code == examCode,
//                e => e.Instructor // لازم نربط بالدكتور عشان نعرف هو منين
//            );

//            if (student == null || exam == null) return false;

//            // 3. الـ Validation Logic (مربط الفرس)
//            bool isAllowed = student.University == exam.Instructor.University &&
//                             student.College == exam.Instructor.College &&
//                             student.Department == exam.Instructor.Department;

//            if (!isAllowed)
//            {
//                // ممكن ترمي Exception مخصص هنا عشان الـ Controller يلقطه
//                throw new Exception("غير مسموح لك بدخول هذا الامتحان لأنه خارج نطاق قسمك أو كليتك.");
//            }

//            // 4. لو تمام، سجل الطالب في الامتحان
//            var enrollment = new ExamEnrollment
//            {
//                StudentId = student.Id,
//                ExamId = exam.Id,
//                EnrolledAt = DateTime.UtcNow
//            };

//            await _unitOfWork.Repository<ExamEnrollment>().AddAsync(enrollment);
//            return await _unitOfWork.CompleteAsync() > 0;
//        }
//    }
//}