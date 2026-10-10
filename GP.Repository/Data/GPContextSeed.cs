using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace GP.Repository.Data
{
    public static class GPContextSeed
    {
        private static readonly string[] FullNames = {
               "Ahmed Mohamed Ali Hassan","Mohamed Ibrahim Khaled Omar","Omar Hassan Hussein Ali","Ali Ahmed Mohamed Ibrahim",
               "Hassan Khaled Omar Mahmoud","Hussein Mohamed Ahmed Salem","Mahmoud Ali Hassan Fathy","Khaled Omar Ibrahim Salah",
               "Ibrahim Hussein Mahmoud Gomaa","Youssef Ahmed Ali Sayed","Amr Mohamed Hassan Abdallah","Tamer Khaled Omar Abdelrahman",
               "Hossam Ali Hussein Abdelaziz","Sherif Ahmed Mohamed Mansour","Wael Ibrahim Hassan Ramadan","Essam Omar Ali Mostafa",
               "Karim Mohamed Khaled Youssef","Samer Hussein Ahmed Khalil","Fadi Ali Omar Hamdy","Nader Mohamed Hassan Nasser",
               "Mostafa Khaled Ibrahim Zaki","Tarek Ahmed Ali Farouk","Walid Omar Mohamed Lotfy","Samir Hassan Hussein Wahba",
               "Ramy Ali Ahmed Shehata","Adel Mohamed Omar Soliman","Ashraf Ibrahim Khaled Amin","Emad Hussein Ali Mohamed",
               "Nabil Ahmed Hassan Omar","Magdy Omar Mohamed Ali"
            };
        private static readonly string[] Universities = {
                "Capital University", "Cairo University", "Ain Shams University",
                "Alexandria University", "Mansoura University"
            };

        private static readonly string[] Colleges = {
                "Science", "Engineering", "Commerce", "Arts"
            };

        // ✅ ربط الكليات بالأقسام
        private static readonly Dictionary<string, string[]> CollegeDepartments = new()
        {
            ["Science"] = new[]
            {
                    "Computer Science & Statistics", "Computer Science & Mathematics", "Physics", "Chemistry",
                    "Biology", "Geology", "Biochemistry", "Mathematics"
                },

            ["Engineering"] = new[]
            {
                    "Civil Engineering", "Mechanical Engineering", "Electrical Engineering",
                    "Computer Engineering", "Architecture Engineering",
                    "Chemical Engineering", "Industrial Engineering"
                },

            ["Commerce"] = new[]
            {
                    "Accounting", "Business Administration", "Economics",
                    "Finance", "Marketing", "Management Information Systems"
                },

            ["Arts"] = new[]
            {
                    "English Literature", "Arabic Literature", "History",
                    "Geography", "Philosophy", "Sociology",
                    "Psychology", "Media Studies"
                }
        };

        public static async Task SeedAsync(GPContext context)
        {
            if (!await context.Users.AnyAsync())
            {
                await SeedInstructorsAsync(context);
                await SeedStudentsAsync(context);
            }
        }

        // ─── Instructors ────────────────────────────────────────
        //private static async Task SeedInstructorsAsync(GPContext context)
        //{
        //    if (await context.Users.AnyAsync(u => u.Role == "Instructor")) return;

        //    var random = new Random(42);
        //    var instructorUsers = new List<User>();

        //    for (int i = 1; i <= 50; i++)
        //    {
        //        var firstName = FirstNames[random.Next(FirstNames.Length)];
        //        var lastName = LastNames[random.Next(LastNames.Length)];

        //        instructorUsers.Add(new User
        //        {
        //            Email = $"dr.{firstName.ToLower()}{i}@university.edu",
        //            PasswordHash = HashPassword($"Dr@{i}"),
        //            FirstName = firstName,
        //            LastName = lastName,
        //            Role = "Instructor",
        //            CreatedAt = DateTime.UtcNow
        //        });
        //    }

        //    await context.Users.AddRangeAsync(instructorUsers);
        //    await context.SaveChangesAsync();

        //    var instructors = instructorUsers.Select((u, index) =>
        //    {
        //        var college = Colleges[index % Colleges.Length];
        //        var departments = CollegeDepartments[college];
        //        var department = departments[random.Next(departments.Length)];

        //        return new Instructor
        //        {
        //            UserId = u.Id,
        //            Department = department,
        //            University = Universities[index % Universities.Length],
        //            College = college,
        //            CreatedAt = DateTime.UtcNow
        //        };
        //    }).ToList();

        //    await context.Instructors.AddRangeAsync(instructors);
        //    await context.SaveChangesAsync();

        //    Console.WriteLine("✅ 50 Instructors seeded successfully");
        //}
        // ─── Instructors ────────────────────────────────────────
        private static async Task SeedInstructorsAsync(GPContext context)
        {
            if (await context.Users.AnyAsync(u => u.Role == "Instructor")) return;

            var random = new Random(42);
            var instructorUsers = new List<User>();
            var instructorCodes = new List<string>(); // ← هنحفظ الكود هنا
            var currentYear = DateTime.UtcNow.Year;

            for (int i = 1; i <= 50; i++)
            {
                var fullName = FullNames[random.Next(FullNames.Length)];
                var nameParts = fullName.Split(' ');
                var threeName = string.Join("", nameParts.Take(3)).ToLower();

                // عمل الـ InstructorCode هنا
                var year = random.Next(2010, currentYear + 1);
                var serial = i.ToString("D2");
                var instructorCode = $"{year}{serial}";
                instructorCodes.Add(instructorCode); // ← احفظه

                var email = $"dr.{threeName}{instructorCode}@university.edu";
                //var password = $"Dr@{i}";
                var password = GenerateRandomPassword(8);
                instructorUsers.Add(new User
                {
                    Email = email,
                    Password = password,
                    PasswordHash = HashPassword(password),
                    FullName = fullName,
                    UniversityID = instructorCode, // ← دلوقتي موجود
                    Role = "Instructor",
                    CreatedAt = DateTime.UtcNow
                });
            }

            await context.Users.AddRangeAsync(instructorUsers);
            await context.SaveChangesAsync();

            var instructors = instructorUsers.Select((u, index) =>
            {
                var college = Colleges[index % Colleges.Length];
                var departments = CollegeDepartments[college];
                var department = departments[random.Next(departments.Length)];

                return new Instructor
                {
                    UserId = u.Id,
                    InstructorCode = instructorCodes[index], // ← من الـ List
                    Department = department,
                    University = Universities[index % Universities.Length],
                    College = college,
                    CreatedAt = DateTime.UtcNow
                };
            }).ToList();

            await context.Instructors.AddRangeAsync(instructors);
            await context.SaveChangesAsync();

            Console.WriteLine("✅ 50 Instructors seeded successfully");
        }

        // ─── Students ───────────────────────────────────────────
        private static async Task SeedStudentsAsync(GPContext context)
        {
            if (await context.Users.AnyAsync(u => u.Role == "Student")) return;

            var random = new Random(42);
            var batchSize = 100;
            var totalStudents = 2000;
            var startYear = 2020;
            var levelCounters = new int[5];

            for (int batch = 0; batch < totalStudents / batchSize; batch++)
            {
                var studentUsers = new List<User>();

                for (int i = 1; i <= batchSize; i++)
                {
                    var globalIndex = batch * batchSize + i;
                    var fullName = FullNames[random.Next(FullNames.Length)];

                    // عمل الـ level و serial هنا
                    var level = (globalIndex % 5) + 1;
                    levelCounters[level - 1]++;
                    var serial = levelCounters[level - 1];
                    var studentUniversityID = $"26{level}{serial:D3}";

                    studentUsers.Add(new User
                    {
                        Email = $"st.{fullName.Split(' ')[0].ToLower()}{globalIndex}@university.edu",
                        PasswordHash = HashPassword($"St@{globalIndex}"),
                        FullName = fullName,
                        UniversityID = studentUniversityID, // ← دلوقتي موجود
                        Role = "Student",
                        CreatedAt = DateTime.UtcNow
                    });
                }

                await context.Users.AddRangeAsync(studentUsers);
                await context.SaveChangesAsync();

                var students = studentUsers.Select((u, index) =>
                {
                    var globalIndex = batch * batchSize + index;

                    var college = Colleges[globalIndex % Colleges.Length];
                    var departments = CollegeDepartments[college];
                    var department = departments[random.Next(departments.Length)];

                    // جيب الـ level من الـ UniversityID
                    var lvl = int.Parse(u.UniversityID[2].ToString());

                    return new Student
                    {
                        UserId = u.Id,
                        StudentUniversityID = u.UniversityID,
                        Department = department,
                        University = Universities[globalIndex % Universities.Length],
                        College = college,
                        Level = lvl,
                        EnrollmentYear = startYear + (globalIndex % 4),
                        CreatedAt = DateTime.UtcNow
                    };
                }).ToList();

                await context.Students.AddRangeAsync(students);
                await context.SaveChangesAsync();

                Console.WriteLine($"✅ Batch {batch + 1}/20 seeded ({(batch + 1) * batchSize} students)");
            }

            Console.WriteLine("✅ 2000 Students seeded successfully");
        }

        // ─── Helper ─────────────────────────────────────────────
        private static string HashPassword(string password)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes("GPSecretSalt"));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hash);
        }
        private static string GenerateRandomPassword(int length = 8)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();

            return new string(Enumerable.Range(0, length)
                .Select(_ => chars[random.Next(chars.Length)])
                .ToArray());
        }
    }
}