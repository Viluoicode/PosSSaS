using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PosSSaS.Domain.Entities;

namespace PosSSaS.Infrastructure.Persistence.Configurations;

public class IngredientConfiguration : IEntityTypeConfiguration<Ingredient>
{
    public void Configure(EntityTypeBuilder<Ingredient> b)
    {
        b.ToTable("Ingredients");
        b.HasKey(i => i.Id);
        b.Property(i => i.Name).IsRequired().HasMaxLength(200);
        b.Property(i => i.Unit).IsRequired().HasMaxLength(20);

        b.HasIndex(i => new { i.TenantId, i.Name }).IsUnique();
    }
}
