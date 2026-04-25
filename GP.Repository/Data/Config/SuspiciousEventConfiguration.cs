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
    public class SuspiciousEventConfiguration : IEntityTypeConfiguration<SuspiciousEvent>
    {
        public void Configure(EntityTypeBuilder<SuspiciousEvent> builder)
        {
            builder.HasKey(se => se.Id);

            builder.Property(se => se.SessionId)
                   .IsRequired();

            builder.Property(se => se.StudentUniversityID)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(se => se.StudentName)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(se => se.CheatingCount)
                   .IsRequired()
                   .HasDefaultValue(0);

            builder.Property(se => se.LeavingExamCount)
                   .IsRequired()
                   .HasDefaultValue(0);

            builder.Property(se => se.ScreenshotsUrl)
                   .IsRequired(false)
                   .HasColumnType("nvarchar(max)");

            builder.Property(se => se.Timestamp)
                   .IsRequired()
                   .IsRequired()
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(se => se.CreatedAt)
                   .IsRequired()
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(se => se.UpdatedAt)
                   .HasDefaultValueSql("GETUTCDATE()");
        }
    }
}
