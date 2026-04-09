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
    public class ExamSessionConfiguration : IEntityTypeConfiguration<ExamSession>
    {
        public void Configure(EntityTypeBuilder<ExamSession> builder)
        {
            builder.HasKey(es => es.Id);

            builder.Property(es => es.ExamId)
                   .IsRequired();

            builder.Property(es => es.StudentId)
                   .IsRequired();

            builder.Property(es => es.StartTime)
                   .IsRequired();

            // EndTime is nullable - session might still be in progress
            builder.Property(es => es.EndTime)
                   .IsRequired(false);

            builder.Property(es => es.FinalRiskScore)
                   .HasColumnType("decimal(5,2)")
                   .HasDefaultValue(0.00m);

            builder.Property(es => es.RiskLevel)
                   .IsRequired()
                   .HasMaxLength(50)
                   .HasDefaultValue("Low");

            builder.Property(es => es.Status)
                   .IsRequired()
                   .HasMaxLength(50)
                   .HasDefaultValue("InProgress");

            builder.Property(es => es.CreatedAt)
                   .IsRequired()
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(es => es.UpdatedAt)
                   .HasDefaultValueSql("GETUTCDATE()");

            // One ExamSession -> Many StudentAnswers
            builder.HasMany(es => es.Answers)
                   .WithOne(sa => sa.Session)
                   .HasForeignKey(sa => sa.SessionId)
                   .OnDelete(DeleteBehavior.Cascade);

            // One ExamSession -> Many SuspiciousEvents
            builder.HasMany(es => es.SuspiciousEvents)
                   .WithOne(se => se.Session)
                   .HasForeignKey(se => se.SessionId)
                   .OnDelete(DeleteBehavior.Cascade);

            // One ExamSession -> One ExamReport
            builder.HasOne(es => es.Report)
                   .WithOne(er => er.Session)
                   .HasForeignKey<ExamReport>(er => er.SessionId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
