using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GP.Core.Entities
{
    public class ExamSession : BaseEntity
    {
        public int ExamId { get; set; }
        public int StudentId { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        public decimal FinalRiskScore { get; set; }
        public string RiskLevel { get; set; }
        public string Status { get; set; }

        public Exam Exam { get; set; }
        public Student Student { get; set; }

        public ICollection<StudentAnswer> Answers { get; set; } = new HashSet<StudentAnswer>();
        public ICollection<SuspiciousEvent> SuspiciousEvents { get; set; } = new HashSet<SuspiciousEvent>();
        public ExamReport Report { get; set; }
    }
}
