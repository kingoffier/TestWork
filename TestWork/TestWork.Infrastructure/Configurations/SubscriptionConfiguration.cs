using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestWork.Core.Models;

namespace TestWork.Infrastructure.Configurations;

public class SubscriptionConfiguration : IEntityTypeConfiguration<SubscriptionModel>
{
    public void Configure(EntityTypeBuilder<SubscriptionModel> builder)
    {
        builder.ToTable("Subscriptions");
        builder.HasKey(subscription => subscription.Id);

        builder.Property(subscription => subscription.ApartmentUrl)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(subscription => subscription.Email)
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(subscription => subscription.ApartmentName)
            .HasMaxLength(300);

        builder.Property(subscription => subscription.CurrentPrice)
            .HasPrecision(18, 2);

        builder.HasIndex(subscription => new
        {
            subscription.ApartmentUrl,
            subscription.Email
        }).IsUnique();
    }
}
