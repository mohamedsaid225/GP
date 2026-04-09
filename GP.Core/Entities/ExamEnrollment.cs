using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GP.Core.Entities
{
    public class ExamEnrollment : BaseEntity
    {
        public int ExamId { get; set; }
        public int StudentId { get; set; }

        public DateTime EnrolledAt { get; set; }

        public Exam Exam { get; set; }
        public Student Student { get; set; }
    }
}
