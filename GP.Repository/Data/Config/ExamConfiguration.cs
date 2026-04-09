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
    public class ExamConfiguration : IEntityTypeConfiguration<Exam>
    {
        public void Configure(EntityTypeBuilder<Exam> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.InstructorId)
                   .IsRequired();

            builder.Property(e => e.Code)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.HasIndex(e => e.Code)
                   .IsUnique();

            builder.Property(e => e.Subject)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(e => e.Department)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(e => e.University)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(e => e.College)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(e => e.Level)
                   .IsRequired();

            builder.Property(e => e.DurationMinutes)
                   .IsRequired();

            builder.Property(e => e.Status)
                   .IsRequired()
                   .HasMaxLength(50)
                   .HasDefaultValue("Draft");

            builder.Property(e => e.CreatedAt)
                   .IsRequired()
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(e => e.UpdatedAt)
                   .HasDefaultValueSql("GETUTCDATE()");

            // One Exam -> Many Questions
            builder.HasMany(e => e.Questions)
                   .WithOne(q => q.Exam)
                   .HasForeignKey(q => q.ExamId)
                   .OnDelete(DeleteBehavior.Cascade);

            // One Exam -> Many ExamEnrollments
            builder.HasMany(e => e.Enrollments)
                   .WithOne(en => en.Exam)
                   .HasForeignKey(en => en.ExamId)
                   .OnDelete(DeleteBehavior.Cascade);

            // One Exam -> Many ExamSessions
            builder.HasMany(e => e.Sessions)
                   .WithOne(es => es.Exam)
                   .HasForeignKey(es => es.ExamId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
