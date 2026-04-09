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
    public class ExamReportConfiguration : IEntityTypeConfiguration<ExamReport>
    {
        public void Configure(EntityTypeBuilder<ExamReport> builder)
        {
            builder.HasKey(er => er.Id);

            builder.Property(er => er.SessionId)
                   .IsRequired();

            builder.Property(er => er.InstructorId)
                   .IsRequired();

            builder.Property(er => er.FinalRiskScore)
                   .HasColumnType("decimal(5,2)")
                   .HasDefaultValue(0.00m);

            builder.Property(er => er.RiskLevel)
                   .IsRequired()
                   .HasMaxLength(50)
                   .HasDefaultValue("Low");

            builder.Property(er => er.ScorePercent)
                   .HasColumnType("decimal(5,2)")
                   .HasDefaultValue(0.00m);

            builder.Property(er => er.Status)
                   .IsRequired()
                   .HasMaxLength(50)
                   .HasDefaultValue("Pending");

            builder.Property(er => er.Notes)
                   .HasColumnType("nvarchar(max)")
                   .HasDefaultValue("");

            builder.Property(er => er.GeneratedAt)
                   .IsRequired()
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(er => er.TotalQuestions)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(er => er.CorrectAnswers)
                   .IsRequired()
                   .HasDefaultValue(0);

            // ReviewedAt is nullable
            builder.Property(er => er.ReviewedAt)
                   .IsRequired(false);

            builder.Property(er => er.CreatedAt)
                   .IsRequired()
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(er => er.UpdatedAt)
                   .HasDefaultValueSql("GETUTCDATE()");

            // FK to Instructor
            builder.HasOne(er => er.Instructor)
                   .WithMany()
                   .HasForeignKey(er => er.InstructorId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
