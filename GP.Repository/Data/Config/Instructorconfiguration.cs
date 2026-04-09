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
    public class InstructorConfiguration : IEntityTypeConfiguration<Instructor>
    {
        public void Configure(EntityTypeBuilder<Instructor> builder)
        {
            builder.HasKey(i => i.Id);

            builder.Property(i => i.UserId)
                   .IsRequired();

            builder.Property(i => i.Department)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(i => i.University)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(i => i.College)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(i => i.CreatedAt)
                   .IsRequired()
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(i => i.UpdatedAt)
                   .HasDefaultValueSql("GETUTCDATE()");

            // One Instructor -> Many Exams
            builder.HasMany(i => i.Exams)
                   .WithOne(e => e.Instructor)
                   .HasForeignKey(e => e.InstructorId)
                   .OnDelete(DeleteBehavior.Restrict);

            // One Instructor -> Many ExamReports
            builder.HasMany(i => i.Reports)
                   .WithOne(r => r.Instructor)
                   .HasForeignKey(r => r.InstructorId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
