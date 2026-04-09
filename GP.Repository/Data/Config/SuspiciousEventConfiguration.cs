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

            // Enum stored as string for readability
            builder.Property(se => se.EventType)
                   .IsRequired()
                   .HasConversion<string>()
                   .HasMaxLength(100);

            builder.Property(se => se.RiskScoreContribution)
                   .HasColumnType("decimal(5,2)")
                   .HasDefaultValue(0.00m);

            builder.Property(se => se.Confidence)
                   .IsRequired()
                   .HasDefaultValue(0.0);

            builder.Property(se => se.Source)
                   .IsRequired()
                   .HasMaxLength(50)
                   .HasDefaultValue("AI");

            builder.Property(se => se.FrameUrl)
                   .IsRequired(false)
                   .HasMaxLength(500);

            builder.Property(se => se.MetadataJson)
                   .IsRequired(false)
                   .HasColumnType("nvarchar(max)");

            builder.Property(se => se.Timestamp)
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
