using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GP.Core.Entities
{
    public class Exam : BaseEntity
    {
        public int InstructorId { get; set; }
        public string Code { get; set; }
        public string Subject { get; set; }
        public string Department { get; set; }
        public string University { get; set; }
        public string College { get; set; }
        public int Level { get; set; }
        public int DurationMinutes { get; set; }
        public string Status { get; set; }

        // Navigation
        public Instructor Instructor { get; set; }
        public ICollection<Question> Questions { get; set; } = new HashSet<Question>();
        public ICollection<ExamEnrollment> Enrollments { get; set; } = new HashSet<ExamEnrollment>();
        public ICollection<ExamSession> Sessions { get; set; } = new HashSet<ExamSession>();
    }
}