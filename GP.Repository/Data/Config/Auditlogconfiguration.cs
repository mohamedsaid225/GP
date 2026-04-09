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
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            // AuditLog uses long Id (bigint) - NOT inheriting BaseEntity
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Id)
                   .ValueGeneratedOnAdd();

            builder.Property(a => a.UserId)
                   .IsRequired();

            builder.Property(a => a.Action)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(a => a.IPAddress)
                   .HasMaxLength(50);

            builder.Property(a => a.Details)
                   .HasColumnType("nvarchar(max)");

            builder.Property(a => a.Timestamp)
                   .IsRequired()
                   .HasDefaultValueSql("GETUTCDATE()");
        }
    }
}
