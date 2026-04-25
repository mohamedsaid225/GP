using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GP.Core.Entities
{
    public class SuspiciousEvent : BaseEntity
    {
        public int SessionId { get; set; }
        public string StudentUniversityID { get; set; }
        public string StudentName { get; set; }
        public int CheatingCount { get; set; }
        public int LeavingExamCount { get; set; }
        public string ScreenshotsUrl { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Navigation
        public ExamSession Session { get; set; }
    }
}