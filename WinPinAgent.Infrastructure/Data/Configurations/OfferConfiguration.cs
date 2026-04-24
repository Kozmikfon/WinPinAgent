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
    public class OfferConfiguration : IEntityTypeConfiguration<Offer>
    {
        public void Configure(EntityTypeBuilder<Offer> builder)
        {
            builder.HasKey(o => o.Id);
            builder.Property(o => o.Price).HasColumnType("decimal(18,2)");
            builder.Property(o => o.StockStatus).HasMaxLength(50);
            builder.Property(o => o.Note).HasMaxLength(500);

            builder.HasOne(o => o.PartRequest)
                   .WithMany(r => r.Offers)
                   .HasForeignKey(o => o.PartRequestId);

            builder.HasOne(o => o.Seller)
                   .WithMany(u => u.Offers)
                   .HasForeignKey(o => o.SellerId);
        }
    }
}
