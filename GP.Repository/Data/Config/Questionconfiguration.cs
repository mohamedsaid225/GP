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
    public class QuestionConfiguration : IEntityTypeConfiguration<Question>
    {
        public void Configure(EntityTypeBuilder<Question> builder)
        {
            builder.HasKey(q => q.Id);

            builder.Property(q => q.ExamId)
                   .IsRequired();

            builder.Property(q => q.QuestionText)
                   .IsRequired()
                   .HasColumnType("nvarchar(max)");

            builder.Property(q => q.OptionA)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(q => q.OptionB)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(q => q.OptionC)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(q => q.OptionD)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(q => q.CorrectAnswer)
                   .IsRequired()
                   .HasMaxLength(1); // "A", "B", "C", "D"

            builder.Property(q => q.OrderIndex)
                   .IsRequired()
                   .HasDefaultValue(0);

            builder.Property(q => q.CreatedAt)
                   .IsRequired()
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(q => q.UpdatedAt)
                   .HasDefaultValueSql("GETUTCDATE()");

            // One Question -> Many StudentAnswers
            builder.HasMany(q => q.StudentAnswers)
                   .WithOne(sa => sa.Question)
                   .HasForeignKey(sa => sa.QuestionId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
