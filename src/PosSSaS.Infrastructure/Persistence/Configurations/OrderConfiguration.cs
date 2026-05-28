using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PosSSaS.Domain.Entities;

namespace PosSSaS.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> b)
    {
        b.ToTable("Orders");
        b.HasKey(o => o.Id);
        b.Property(o => o.TotalAmount).HasPrecision(18, 2);
        b.Property(o => o.Status).HasConversion<int>();

        b.HasOne(o => o.Branch)
            .WithMany(br => br.Orders)
            .HasForeignKey(o => o.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasMany(o => o.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(o => new { o.TenantId, o.BranchId, o.OrderDate });
    }
}

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> b)
    {
        b.ToTable("OrderItems");
        b.HasKey(i => i.Id);
        b.Property(i => i.Price).HasPrecision(18, 2);
        b.Ignore(i => i.LineTotal);

        b.HasOne(i => i.Product)
            .WithMany()
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
