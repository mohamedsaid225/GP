using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GP.Core.Entities
{
    public class Student : BaseEntity
    {
        public int UserId { get; set; }
        public string StudentUniversityID { get; set; }
        public string Department { get; set; }
        public string University { get; set; }
        public string College { get; set; }
        public int Level { get; set; }
        public int EnrollmentYear { get; set; }

        // Navigation
        public User User { get; set; }
        public ICollection<ExamEnrollment> Enrollments { get; set; } = new HashSet<ExamEnrollment>();
        public ICollection<ExamSession> Sessions { get; set; } = new HashSet<ExamSession>();
    }
}