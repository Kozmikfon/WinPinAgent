using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WinPinAgent.Domain.Entities;

namespace WinPinAgent.Infrastructure.Data.Configurations;

public class RatingConfiguration : IEntityTypeConfiguration<Rating>
{
    public void Configure(EntityTypeBuilder<Rating> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Score).IsRequired();
        builder.Property(r => r.Comment).HasMaxLength(500);

        builder.HasOne(r => r.Rater)
               .WithMany(u => u.GivenRatings)
               .HasForeignKey(r => r.RaterId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.RatedUser)
               .WithMany(u => u.ReceivedRatings)
               .HasForeignKey(r => r.RatedUserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Offer)
               .WithOne(o => o.Rating)
               .HasForeignKey<Rating>(r => r.OfferId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}