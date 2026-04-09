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
        private static readonly string[] FirstNames = {
            "Ahmed", "Mohamed", "Omar", "Ali", "Hassan", "Hussein", "Mahmoud", "Khaled",
            "Ibrahim", "Youssef", "Amr", "Tamer", "Hossam", "Sherif", "Wael", "Essam",
            "Karim", "Samer", "Fadi", "Nader", "Mostafa", "Tarek", "Walid", "Samir",
            "Ramy", "Adel", "Ashraf", "Emad", "Nabil", "Magdy"
        };

        private static readonly string[] LastNames = {
            "Mohamed", "Ahmed", "Hassan", "Hussein", "Ali", "Ibrahim", "Mahmoud", "Khaled",
            "Salem", "Fathy", "Salah", "Gomaa", "Sayed", "Abdallah", "Abdelrahman",
            "Abdelaziz", "Mansour", "Ramadan", "Mostafa", "Youssef", "Khalil", "Hamdy",
            "Nasser", "Zaki", "Farouk", "Lotfy", "Wahba", "Shehata", "Soliman", "Amin"
        };

        private static readonly string[] Departments = {
            "Computer Science", "Mathematics", "Physics", "Chemistry",
            "Biology", "Statistics", "Geology", "Biochemistry"
        };

        private static readonly string[] Universities = {
            "Helwan University", "Cairo University", "Ain Shams University",
            "Alexandria University", "Mansoura University"
        };

        private static readonly string[] Colleges = {
            "Science", "Engineering", "Commerce", "Arts"
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
        private static async Task SeedInstructorsAsync(GPContext context)
        {
            if (await context.Users.AnyAsync(u => u.Role == "Instructor")) return;

            var random = new Random(42);
            var instructorUsers = new List<User>();

            for (int i = 1; i <= 50; i++)
            {
                var firstName = FirstNames[random.Next(FirstNames.Length)];
                var lastName = LastNames[random.Next(LastNames.Length)];
                var id = 200000 + i;

                instructorUsers.Add(new User
                {
                    Email = $"dr.{firstName.ToLower()}{id}@university.edu",
                    PasswordHash = HashPassword($"Dr@{id}"),
                    FirstName = firstName,
                    LastName = lastName,
                    Role = "Instructor",
                    CreatedAt = DateTime.UtcNow
                });
            }

            await context.Users.AddRangeAsync(instructorUsers);
            await context.SaveChangesAsync();

            var instructors = instructorUsers.Select((u, index) => new Instructor
            {
                UserId = u.Id,
                Department = Departments[index % Departments.Length],
                University = Universities[index % Universities.Length],
                College = Colleges[index % Colleges.Length],
                CreatedAt = DateTime.UtcNow
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
            var batchSize = 100; // هنضيف 100 في كل مرة عشان ما يبطأش
            var totalStudents = 2000;
            var startYear = 2020;

            for (int batch = 0; batch < totalStudents / batchSize; batch++)
            {
                var studentUsers = new List<User>();

                for (int i = 1; i <= batchSize; i++)
                {
                    var globalIndex = batch * batchSize + i;
                    var firstName = FirstNames[random.Next(FirstNames.Length)];
                    var lastName = LastNames[random.Next(LastNames.Length)];
                    var universityId = 200000 + globalIndex;

                    studentUsers.Add(new User
                    {
                        Email = $"st.{firstName.ToLower()}{universityId}@university.edu",
                        PasswordHash = HashPassword($"St@{universityId}"),
                        FirstName = firstName,
                        LastName = lastName,
                        Role = "Student",
                        CreatedAt = DateTime.UtcNow
                    });
                }

                await context.Users.AddRangeAsync(studentUsers);
                await context.SaveChangesAsync();

                var students = studentUsers.Select((u, index) =>
                {
                    var globalIndex = batch * batchSize + index;
                    return new Student
                    {
                        UserId = u.Id,
                        StudentUniversityID = (200000 + batch * batchSize + index + 1).ToString(),
                        Department = Departments[globalIndex % Departments.Length],
                        University = Universities[globalIndex % Universities.Length],
                        College = Colleges[globalIndex % Colleges.Length],
                        Level = (globalIndex % 4) + 1,
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
    }
}