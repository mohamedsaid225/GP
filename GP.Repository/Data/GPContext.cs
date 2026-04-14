using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GP.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace GP.Repository.Data
{
    public class GPContext : DbContext
    {
        public GPContext(DbContextOptions<GPContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Instructor> Instructors { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<ExamEnrollment> ExamEnrollments { get; set; }
        public DbSet<ExamSession> ExamSessions { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<StudentAnswer> StudentAnswers { get; set; }
        public DbSet<ExamReport> ExamReports { get; set; }
        public DbSet<SuspiciousEvent> SuspiciousEvents { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Comment> Comments { get; set; }

    }
}