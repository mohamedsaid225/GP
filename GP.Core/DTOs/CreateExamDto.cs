using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GP.Core.DTOs
{
    public class CreateExamDto
    {
        [Required]
        [MinLength(3)]
        public string Code { get; set; }

        [Required]
        [MinLength(3)]
        public string Subject { get; set; }

        [Required]
        [MinLength(3)]
        public string Department { get; set; }

        [Required]
        [MinLength(3)]
        public string University { get; set; }

        [Required]
        [MinLength(3)]
        public string College { get; set; }

        [Range(1, 10)]
        public int Level { get; set; }

        [Range(1, 500)]
        public int DurationMinutes { get; set; }

        [Required]
        public DateTime? ActiveFrom { get; set; }

        [Required]
        public DateTime? ActiveTo { get; set; }

        public List<CreateQuestionDto>? Questions { get; set; }


        //public string Code { get; set; }
        //public string Subject { get; set; }
        //public string Department { get; set; }
        //public string University { get; set; }
        //public string College { get; set; }
        //public int Level { get; set; }
        //public int DurationMinutes { get; set; }
        //public DateTime? ActiveFrom { get; set; }
        //public DateTime? ActiveTo { get; set; }
        //public List<CreateQuestionDto>? Questions { get; set; }
    }
}
