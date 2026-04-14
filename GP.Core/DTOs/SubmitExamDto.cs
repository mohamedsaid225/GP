using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GP.Core.DTOs
{
    public class SubmitExamDto
    {
        public List<SubmitAnswerDto> Answers { get; set; }
        public int LeavingExam {  get; set; }
    }
}
