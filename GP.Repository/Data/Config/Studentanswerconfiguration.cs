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
    public class StudentAnswerConfiguration : IEntityTypeConfiguration<StudentAnswer>
    {
        public void Configure(EntityTypeBuilder<StudentAnswer> builder)
        {
            builder.HasKey(sa => sa.Id);

            builder.Property(sa => sa.SessionId)
                   .IsRequired();

            builder.Property(sa => sa.QuestionId)
                   .IsRequired();

            builder.Property(sa => sa.SelectedAnswer)
                   .IsRequired(false)
                   .HasMaxLength(1); // "A", "B", "C", "D"

            builder.Property(sa => sa.IsCorrect)
                   .IsRequired()
                   .HasDefaultValue(false);

            builder.Property(sa => sa.AnsweredAt)
                   .IsRequired()
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(sa => sa.CreatedAt)
                   .IsRequired()
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(sa => sa.UpdatedAt)
                   .HasDefaultValueSql("GETUTCDATE()");

            // Prevent duplicate answer per question per session
            builder.HasIndex(sa => new { sa.SessionId, sa.QuestionId })
                   .IsUnique();
        }
    }
}
