using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GP.Core.DTOs
{
    public class CreateExamDto
    {
        public string Code { get; set; }
        public string Subject { get; set; }
        public string Department { get; set; }
        public string University { get; set; }
        public string College { get; set; }
        public int Level { get; set; }
        public int DurationMinutes { get; set; }
        public List<CreateQuestionDto> Questions { get; set; } = new List<CreateQuestionDto>();
    }
}
