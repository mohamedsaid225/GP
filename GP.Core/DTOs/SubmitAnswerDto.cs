using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GP.Core.DTOs
{
    public class SubmitAnswerDto
    {
        public int QuestionId { get; set; }
        public string SelectedAnswer { get; set; }
    }
}
