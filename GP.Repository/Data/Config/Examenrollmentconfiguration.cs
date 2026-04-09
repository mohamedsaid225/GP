using GP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GP.Repository.Data.Config
{
    public class ExamEnrollmentConfiguration : IEntityTypeConfiguration<ExamEnrollment>
    {
        public void Configure(EntityTypeBuilder<ExamEnrollment> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.ExamId)
                   .IsRequired();

            builder.Property(e => e.StudentId)
                   .IsRequired();

            builder.Property(e => e.EnrolledAt)
                   .IsRequired()
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(e => e.CreatedAt)
                   .IsRequired()
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(e => e.UpdatedAt)
                   .HasDefaultValueSql("GETUTCDATE()");

            // Prevent duplicate enrollment
            builder.HasIndex(e => new { e.ExamId, e.StudentId })
                   .IsUnique();

            // FK to Student فقط — FK to Exam متعرف في ExamConfiguration
            builder.HasOne(e => e.Student)
                   .WithMany(s => s.Enrollments)
                   .HasForeignKey(e => e.StudentId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}