using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PosSSaS.Domain.Entities;

namespace PosSSaS.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> b)
    {
        b.ToTable("Categories");
        b.HasKey(c => c.Id);
        b.Property(c => c.Name).IsRequired().HasMaxLength(200);
        b.Property(c => c.Description).HasMaxLength(1000);
        b.HasIndex(c => new { c.TenantId, c.Name }).IsUnique();
    }
}
