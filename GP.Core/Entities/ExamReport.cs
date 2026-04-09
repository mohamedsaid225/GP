using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GP.Core.Entities
{
    public class ExamReport : BaseEntity
    {
        public int SessionId { get; set; }
        public int InstructorId { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public decimal FinalRiskScore { get; set; }
        public string RiskLevel { get; set; }
        public decimal ScorePercent { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }

        public DateTime GeneratedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }

        public ExamSession Session { get; set; }
        public Instructor Instructor { get; set; }
    }
}
