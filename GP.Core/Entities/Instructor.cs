    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    namespace GP.Core.Entities
    {
        public class Instructor : BaseEntity
        {
            public int UserId { get; set; }
            public string InstructorCode { get; set; }

            public string Department { get; set; }
            public string University { get; set; }
            public string College { get; set; }

            // Navigation
            public User User { get; set; }
            public ICollection<Exam> Exams { get; set; } = new HashSet<Exam>();
            public ICollection<ExamReport> Reports { get; set; } = new HashSet<ExamReport>();
        }
    }