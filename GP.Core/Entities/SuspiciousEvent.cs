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
        public EventType EventType { get; set; }
        public decimal RiskScoreContribution { get; set; }
        public double Confidence { get; set; }
        public string Source { get; set; } = "AI";
        public string? FrameUrl { get; set; }
        public string? MetadataJson { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Navigation
        public ExamSession Session { get; set; } = null!;
    }
}