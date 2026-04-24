using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinPinAgent.Domain.Entities;

namespace WinPinAgent.Infrastructure.Data.Configurations
{
    public class PartRequestConfiguration : IEntityTypeConfiguration<PartRequest>
    {
        public void Configure(EntityTypeBuilder<PartRequest> builder)
        {
            builder.HasKey(r => r.Id);
            builder.Property(r => r.Vin).HasMaxLength(17);
            builder.Property(r => r.Brand).HasMaxLength(50);
            builder.Property(r => r.PartName).HasMaxLength(200);
            builder.Property(r => r.Status).HasMaxLength(20);

            builder.HasOne(r => r.Buyer)
                   .WithMany(u => u.Requests)
                   .HasForeignKey(r => r.BuyerId);
        }
    }
}
