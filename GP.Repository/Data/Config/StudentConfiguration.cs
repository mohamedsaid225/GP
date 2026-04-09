using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GP.Repository.Data.Config
{
    public class StudentConfiguration : IEntityTypeConfiguration<Student>
    {
        public void Configure(EntityTypeBuilder<Student> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.UserId)
                   .IsRequired();

            builder.Property(s => s.StudentUniversityID)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.HasIndex(s => s.StudentUniversityID)
                   .IsUnique();

            builder.Property(s => s.Department)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(s => s.University)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(s => s.College)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(s => s.Level)
                   .IsRequired();

            builder.Property(s => s.EnrollmentYear)
                   .IsRequired();

            builder.Property(s => s.CreatedAt)
                   .IsRequired()
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(s => s.UpdatedAt)
                   .HasDefaultValueSql("GETUTCDATE()");

            // One Student -> Many ExamEnrollments
            builder.HasMany(s => s.Enrollments)
                   .WithOne(e => e.Student)
                   .HasForeignKey(e => e.StudentId)
                   .OnDelete(DeleteBehavior.Cascade);

            // One Student -> Many ExamSessions
            builder.HasMany(s => s.Sessions)
                   .WithOne(es => es.Student)
                   .HasForeignKey(es => es.StudentId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
