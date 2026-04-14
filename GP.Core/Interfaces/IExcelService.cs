using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GP.Core.Entities;

namespace GP.Core.Interfaces
{
    public interface IExcelService
    {
        Task<byte[]> GenerateExamReportAsync(ExamReport report, ExamSession session,
            User studentUser, Student student, Exam exam,
            IEnumerable<SuspiciousEvent> events, IEnumerable<StudentAnswer> answers);
        Task<byte[]> GenerateExamReportsAsync(List<object> data, Exam exam);

    }
}

