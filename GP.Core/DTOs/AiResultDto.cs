using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GP.Core.DTOs
{
    public class AiResultDto
    {
        //public string EventType { get; set; }
        //public decimal RiskScoreContribution { get; set; }
        //public double Confidence { get; set; }
        //public string? FrameUrl { get; set; }
        //public string? MetadataJson { get; set; }
        public string StudentUniversityID { get; set; }
        public int CheatingCount { get; set; }
        public string ScreenshotsUrl { get; set; }
    }
}
