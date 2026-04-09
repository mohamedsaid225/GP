using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GP.Core.Entities
{
    public class StudentAnswer : BaseEntity
    {
        public int SessionId { get; set; }
        public int QuestionId { get; set; }
        public string SelectedAnswer { get; set; } // "A", "B", "C", "D"
        public bool IsCorrect { get; set; }
        public DateTime AnsweredAt { get; set; }

        // Navigation
        public ExamSession Session { get; set; }
        public Question Question { get; set; }
    }
}