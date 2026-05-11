using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PosSSaS.Domain.Entities;

namespace PosSSaS.Infrastructure.Persistence.Configurations;

public class IngredientStockConfiguration : IEntityTypeConfiguration<IngredientStock>
{
    public void Configure(EntityTypeBuilder<IngredientStock> b)
    {
        b.ToTable("IngredientStocks");
        b.HasKey(s => s.Id);
        b.Property(s => s.Quantity).HasPrecision(18, 3);
        b.Property(s => s.RowVersion).IsRowVersion();

        b.HasOne(s => s.Branch)
            .WithMany(br => br.Stocks)
            .HasForeignKey(s => s.BranchId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(s => s.Ingredient)
            .WithMany(i => i.Stocks)
            .HasForeignKey(s => s.IngredientId)
            .OnDelete(DeleteBehavior.Restrict);

        // One stock row per (branch, ingredient) — enforced unique.
        b.HasIndex(s => new { s.BranchId, s.IngredientId }).IsUnique();
    }
}
