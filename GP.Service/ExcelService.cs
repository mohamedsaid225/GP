using System.Drawing;
using ClosedXML.Excel;
using GP.Core.Entities;
using GP.Core.Interfaces;

namespace GP.Service
{
    public class ExcelService : IExcelService
    {
        public Task<byte[]> GenerateExamReportAsync(ExamReport report, ExamSession session,
            User studentUser, Student student, Exam exam,
            IEnumerable<SuspiciousEvent> events, IEnumerable<StudentAnswer> answers)
        {
            using var workbook = new XLWorkbook();

            // ─── Sheet 1: Summary ────────────────────────────
            var summary = workbook.Worksheets.Add("Summary");

            summary.Cell("A1").Value = "AI Online Exam Anti-Cheating System";
            summary.Cell("A1").Style.Font.Bold = true;
            summary.Cell("A1").Style.Font.FontSize = 16;
            summary.Range("A1:D1").Merge();

            summary.Cell("A3").Value = "Student Name";
            summary.Cell("B3").Value = $"{studentUser.FirstName} {studentUser.LastName}";

            summary.Cell("A4").Value = "Student ID";
            summary.Cell("B4").Value = student.StudentUniversityID;

            summary.Cell("A5").Value = "Exam Subject";
            summary.Cell("B5").Value = exam.Subject;

            summary.Cell("A6").Value = "Exam Code";
            summary.Cell("B6").Value = exam.Code;

            summary.Cell("A7").Value = "Start Time";
            summary.Cell("B7").Value = session.StartTime.ToString("yyyy-MM-dd HH:mm:ss");

            summary.Cell("A8").Value = "End Time";
            summary.Cell("B8").Value = session.EndTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A";

            summary.Cell("A10").Value = "Total Questions";
            summary.Cell("B10").Value = report.TotalQuestions;

            summary.Cell("A11").Value = "Correct Answers";
            summary.Cell("B11").Value = report.CorrectAnswers;

            summary.Cell("A12").Value = "Score";
            summary.Cell("B12").Value = $"{report.ScorePercent}%";

            summary.Cell("A14").Value = "Risk Score";
            summary.Cell("B14").Value = report.FinalRiskScore;
            summary.Cell("A14").Style.Font.Bold = true;

            summary.Cell("A15").Value = "Risk Level";
            summary.Cell("B15").Value = report.RiskLevel;

            // لون الـ Risk Level
            var riskCell = summary.Cell("B15");
            riskCell.Style.Font.Bold = true;
            riskCell.Style.Font.FontColor = report.RiskLevel switch
            {
                "High" => XLColor.Red,
                "Medium" => XLColor.Orange,
                _ => XLColor.Green
            };

            summary.Cell("A17").Value = "Status";
            summary.Cell("B17").Value = report.Status;

            summary.Cell("A18").Value = "Notes";
            summary.Cell("B18").Value = report.Notes ?? "No notes";

            summary.Columns().AdjustToContents();

            // ─── Sheet 2: Suspicious Events ──────────────────
            var eventsSheet = workbook.Worksheets.Add("Suspicious Events");

            eventsSheet.Cell("A1").Value = "Event Type";
            eventsSheet.Cell("B1").Value = "Source";
            eventsSheet.Cell("C1").Value = "Risk Contribution";
            eventsSheet.Cell("D1").Value = "Confidence";
            eventsSheet.Cell("E1").Value = "Timestamp";
            eventsSheet.Cell("F1").Value = "Frame URL";

            var headerRow = eventsSheet.Range("A1:F1");
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = XLColor.LightBlue;

            int row = 2;
            foreach (var ev in events)
            {
                eventsSheet.Cell(row, 1).Value = ev.EventType.ToString();
                eventsSheet.Cell(row, 2).Value = ev.Source;
                eventsSheet.Cell(row, 3).Value = (double)ev.RiskScoreContribution;
                eventsSheet.Cell(row, 4).Value = ev.Confidence;
                eventsSheet.Cell(row, 5).Value = ev.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
                eventsSheet.Cell(row, 6).Value = ev.FrameUrl ?? "N/A";
                row++;
            }

            eventsSheet.Columns().AdjustToContents();

            // ─── Sheet 3: Answers ────────────────────────────
            var answersSheet = workbook.Worksheets.Add("Answers");

            answersSheet.Cell("A1").Value = "Question ID";
            answersSheet.Cell("B1").Value = "Selected Answer";
            answersSheet.Cell("C1").Value = "Is Correct";
            answersSheet.Cell("D1").Value = "Answered At";

            var answersHeader = answersSheet.Range("A1:D1");
            answersHeader.Style.Font.Bold = true;
            answersHeader.Style.Fill.BackgroundColor = XLColor.LightBlue;

            int ansRow = 2;
            foreach (var answer in answers)
            {
                answersSheet.Cell(ansRow, 1).Value = answer.QuestionId;
                answersSheet.Cell(ansRow, 2).Value = answer.SelectedAnswer;
                answersSheet.Cell(ansRow, 3).Value = answer.IsCorrect ? "✓" : "✗";
                answersSheet.Cell(ansRow, 4).Value = answer.AnsweredAt.ToString("yyyy-MM-dd HH:mm:ss");

                // لون الإجابة
                answersSheet.Cell(ansRow, 3).Style.Font.FontColor = answer.IsCorrect
                    ? XLColor.Green
                    : XLColor.Red;

                ansRow++;
            }

            answersSheet.Columns().AdjustToContents();

            // ─── Convert to bytes ────────────────────────────
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return Task.FromResult(stream.ToArray());
        }
    }
}