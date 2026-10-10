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
            summary.Cell("B3").Value = $"{studentUser.FullName}";

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

            summary.Cell("A9").Value = "Total Questions";
            summary.Cell("B9").Value = report.TotalQuestions;

            summary.Cell("10").Value = "Correct Answers";
            summary.Cell("B10").Value = report.CorrectAnswers;

            summary.Cell("A11").Value = "Score";
            summary.Cell("B11").Value = $"{report.ScorePercent}%";

            summary.Cell("A12").Value = "Status";
            summary.Cell("B12").Value = report.Status;

            summary.Cell("A13").Value = "Notes";
            summary.Cell("B13").Value = report.Notes ?? "No notes";

            summary.Columns().AdjustToContents();

            
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

        //public Task<byte[]> GenerateExamReportsAsync(List<object> data, Exam exam)
        //{
        //    using var workbook = new XLWorkbook();
        //    var sheet = workbook.Worksheets.Add("Exam Report");

        //    // 1. تحديث الـ Header لإضافة عمود الـ Notes
        //    sheet.Cell(1, 1).Value = "Student Name";
        //    sheet.Cell(1, 2).Value = "University ID";
        //    sheet.Cell(1, 3).Value = "Exam Code";
        //    sheet.Cell(1, 4).Value = "Subject";
        //    sheet.Cell(1, 5).Value = "Score %";
        //    sheet.Cell(1, 6).Value = "Correct Answers";
        //    sheet.Cell(1, 7).Value = "Total Questions";
        //    sheet.Cell(1, 8).Value = "Risk Level";
        //    sheet.Cell(1, 9).Value = "Final Risk Score";
        //    sheet.Cell(1, 10).Value = "Notes"; // <--- ضفنا العنوان هنا
        //    sheet.Cell(1, 11).Value = "Status";

        //    // تحديث نطاق الـ Style ليشمل العمود K (العمود 11)
        //    var header = sheet.Range("A1:K1");
        //    header.Style.Font.Bold = true;
        //    header.Style.Fill.BackgroundColor = XLColor.LightBlue;

        //    int row = 2;

        //    foreach (var item in data)
        //    {
        //        var type = item.GetType();

        //        sheet.Cell(row, 1).Value = type.GetProperty("StudentName")?.GetValue(item)?.ToString();
        //        sheet.Cell(row, 2).Value = type.GetProperty("StudentUniversityID")?.GetValue(item)?.ToString();
        //        sheet.Cell(row, 3).Value = type.GetProperty("ExamCode")?.GetValue(item)?.ToString();
        //        sheet.Cell(row, 4).Value = type.GetProperty("ExamSubject")?.GetValue(item)?.ToString();
        //        sheet.Cell(row, 5).Value = Convert.ToDouble(type.GetProperty("ScorePercent")?.GetValue(item) ?? 0);
        //        sheet.Cell(row, 6).Value = Convert.ToInt32(type.GetProperty("CorrectAnswers")?.GetValue(item) ?? 0);
        //        sheet.Cell(row, 7).Value = Convert.ToInt32(type.GetProperty("TotalQuestions")?.GetValue(item) ?? 0);
        //        sheet.Cell(row, 8).Value = type.GetProperty("RiskLevel")?.GetValue(item)?.ToString();
        //        sheet.Cell(row, 9).Value = Convert.ToDouble(type.GetProperty("FinalRiskScore")?.GetValue(item) ?? 0);
        //        // 2. قراءة قيمة الـ Notes ووضعها في العمود رقم 11
        //        sheet.Cell(row, 10).Value = type.GetProperty("Notes")?.GetValue(item)?.ToString() ?? "";
        //        sheet.Cell(row, 11).Value = type.GetProperty("Status")?.GetValue(item)?.ToString();



        //        // 🎨 Risk coloring
        //        var risk = type.GetProperty("RiskLevel")?.GetValue(item)?.ToString();
        //        var riskCell = sheet.Cell(row, 8);
        //        riskCell.Style.Font.Bold = true;
        //        riskCell.Style.Font.FontColor = risk switch
        //        {
        //            "High" => XLColor.Red,
        //            "Medium" => XLColor.Orange,
        //            _ => XLColor.Green
        //        };

        //        row++;
        //    }

        //    sheet.Columns().AdjustToContents();
        //    using var stream = new MemoryStream();
        //    workbook.SaveAs(stream);

        //    return Task.FromResult(stream.ToArray());
        //}

        //    public Task<byte[]> GenerateExamReportsAsync(List<object> data, Exam exam)
        //    {
        //        using var workbook = new XLWorkbook();
        //        var sheet = workbook.Worksheets.Add("Exam Report");

        //        string[] headers = {
        //    "Student Name", "University ID", "Exam Code", "Subject",
        //    "Score %", "Correct Answers", "Total Questions",
        //    "Risk Level", "Final Risk Score", "Notes", "Status"
        //};

        //        // --- تنسيق الهيدر ---
        //        for (int i = 0; i < headers.Length; i++)
        //        {
        //            var cell = sheet.Cell(1, i + 1);
        //            cell.Value = headers[i];
        //            cell.Style.Font.Bold = true;
        //            cell.Style.Font.FontColor = XLColor.White;
        //            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#2c3e50");
        //            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        //        }

        //        int row = 2;
        //        foreach (var item in data)
        //        {
        //            var type = item.GetType();

        //            sheet.Cell(row, 1).Value = type.GetProperty("StudentName")?.GetValue(item)?.ToString();
        //            sheet.Cell(row, 2).Value = type.GetProperty("StudentUniversityID")?.GetValue(item)?.ToString();
        //            sheet.Cell(row, 3).Value = type.GetProperty("ExamCode")?.GetValue(item)?.ToString();
        //            sheet.Cell(row, 4).Value = type.GetProperty("ExamSubject")?.GetValue(item)?.ToString();

        //            // --- تعديل تنسيق الـ Score عشان ميظهرش ####### ---
        //            var scoreVal = Convert.ToDouble(type.GetProperty("ScorePercent")?.GetValue(item) ?? 0);
        //            var scoreCell = sheet.Cell(row, 5);
        //            scoreCell.Value = scoreVal / 100; // نقسم على 100 عشان إكسيل بيفهم الـ % ككسر
        //            scoreCell.Style.NumberFormat.Format = "0%"; // تنسيق مئوي قياسي

        //            sheet.Cell(row, 6).Value = Convert.ToInt32(type.GetProperty("CorrectAnswers")?.GetValue(item) ?? 0);
        //            sheet.Cell(row, 7).Value = Convert.ToInt32(type.GetProperty("TotalQuestions")?.GetValue(item) ?? 0);
        //            sheet.Cell(row, 8).Value = type.GetProperty("Notes")?.GetValue(item)?.ToString() ?? "-";
        //            sheet.Cell(row, 9).Value = type.GetProperty("Status")?.GetValue(item)?.ToString();

        //            // محاذاة البيانات في المنتصف
        //            sheet.Range(row, 1, row, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        //            row++;
        //        }

        //        // --- لمسات تحسين الشكل النهائي ---

        //        // 1. تحويل الجدول لـ Excel Table
        //        var range = sheet.Range(1, 1, row - 1, 11);
        //        var table = range.CreateTable();
        //        table.Theme = XLTableTheme.TableStyleMedium2;

        //        // 2. ضبط العرض تلقائياً لكل الأعمدة
        //        sheet.Columns().AdjustToContents();

        //        // 3. زيادة عرض عمود الـ Notes والـ Score يدوياً لضمان عدم ظهور الشباك #
        //        sheet.Column(5).Width = 15;  // Score column
        //        sheet.Column(10).Width = 45; // Notes column (خليته أوسع عشان الكلام ميبقاش مقطوع)

        //        sheet.SheetView.FreezeRows(1);

        //        using var stream = new MemoryStream();
        //        workbook.SaveAs(stream);
        //        return Task.FromResult(stream.ToArray());
        //    }


        public Task<byte[]> GenerateExamReportsAsync(List<object> data, Exam exam)
        {
            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Exam Report");

            string[] headers = {"Student Name", "University ID", "Exam Code", "Subject",
                                "Score %", "Correct Answers", "Total Questions",
                                "Cheating Count", "Screenshots URL", "Notes", "Status" };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = sheet.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#2c3e50");
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            int row = 2;
            foreach (var item in data)
            {
                var type = item.GetType();

                sheet.Cell(row, 1).Value = type.GetProperty("StudentName")?.GetValue(item)?.ToString();
                sheet.Cell(row, 2).Value = type.GetProperty("StudentUniversityID")?.GetValue(item)?.ToString();
                sheet.Cell(row, 3).Value = type.GetProperty("ExamCode")?.GetValue(item)?.ToString();
                sheet.Cell(row, 4).Value = type.GetProperty("ExamSubject")?.GetValue(item)?.ToString();

                var scoreVal = Convert.ToDouble(type.GetProperty("ScorePercent")?.GetValue(item) ?? 0);
                var scoreCell = sheet.Cell(row, 5);
                scoreCell.Value = scoreVal / 100;
                scoreCell.Style.NumberFormat.Format = "0%";

                sheet.Cell(row, 6).Value = Convert.ToInt32(type.GetProperty("CorrectAnswers")?.GetValue(item) ?? 0);
                sheet.Cell(row, 7).Value = Convert.ToInt32(type.GetProperty("TotalQuestions")?.GetValue(item) ?? 0);
                sheet.Cell(row, 8).Value = Convert.ToInt32(type.GetProperty("CheatingCount")?.GetValue(item) ?? 0);
                sheet.Cell(row, 9).Value = type.GetProperty("ScreenshotsUrl")?.GetValue(item)?.ToString() ?? "-";
                sheet.Cell(row, 10).Value = type.GetProperty("Notes")?.GetValue(item)?.ToString() ?? "-";
                sheet.Cell(row, 11).Value = type.GetProperty("Status")?.GetValue(item)?.ToString();

                sheet.Range(row, 1, row, 11).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                row++;
            }

            var range = sheet.Range(1, 1, row - 1, 11);
            var table = range.CreateTable();
            table.Theme = XLTableTheme.TableStyleMedium2;

            sheet.Columns().AdjustToContents();
            sheet.Column(2).Width = 25;
            sheet.Column(3).Width = 15;
            sheet.Column(4).Width = 15;
            sheet.Column(5).Width = 15;
            sheet.Column(6).Width = 25;
            sheet.Column(7).Width = 25;
            sheet.Column(8).Width = 25;
            sheet.Column(9).Width = 45;
            sheet.Column(10).Width = 45;
            sheet.Column(11).Width = 15;

            sheet.SheetView.FreezeRows(1);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return Task.FromResult(stream.ToArray());
        }
    }
}