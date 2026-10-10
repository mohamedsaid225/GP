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
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Email)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.HasIndex(u => u.Email)
                   .IsUnique();

            builder.Property(u => u.Password)
                   .IsRequired(false)
                   .HasMaxLength(50);

            builder.Property(u => u.PasswordHash)
                   .IsRequired()
                   .HasMaxLength(256);

            builder.Property(u => u.UniversityID)
                   .IsRequired(false)
                   .HasMaxLength(20);

            builder.Property(u => u.FullName)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(u => u.CreatedAt)
                   .IsRequired()
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(u => u.UpdatedAt)
                   .HasDefaultValueSql("GETUTCDATE()");

            // One User -> One Student
            builder.HasOne(u => u.Student)
                   .WithOne(s => s.User)
                   .HasForeignKey<Student>(s => s.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // One User -> One Instructor
            builder.HasOne(u => u.Instructor)
                   .WithOne(i => i.User)
                   .HasForeignKey<Instructor>(i => i.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // One User -> Many AuditLogs
            builder.HasMany(u => u.AuditLogs)
                   .WithOne(a => a.User)
                   .HasForeignKey(a => a.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
