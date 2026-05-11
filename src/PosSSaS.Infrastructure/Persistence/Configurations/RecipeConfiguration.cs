using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PosSSaS.Domain.Entities;

namespace PosSSaS.Infrastructure.Persistence.Configurations;

public class RecipeConfiguration : IEntityTypeConfiguration<Recipe>
{
    public void Configure(EntityTypeBuilder<Recipe> b)
    {
        b.ToTable("Recipes");
        b.HasKey(r => r.Id);
        b.Property(r => r.QuantityRequired).HasPrecision(18, 3);

        b.HasOne(r => r.Product)
            .WithMany(p => p.RecipeItems)
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(r => r.Ingredient)
            .WithMany()
            .HasForeignKey(r => r.IngredientId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(r => new { r.ProductId, r.IngredientId }).IsUnique();
    }
}
