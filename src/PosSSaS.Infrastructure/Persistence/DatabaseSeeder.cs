using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PosSSaS.Application.Common.Interfaces;
using PosSSaS.Domain.Entities;
using PosSSaS.Domain.Enums;

namespace PosSSaS.Infrastructure.Persistence;

/// <summary>
/// Applies pending migrations and, if the database is empty, inserts a demo tenant with
/// 2 branches, 3 categories, 5 ingredients, 3 products + recipes, and per-branch stock.
/// Two users seeded: admin (all branches) and cashier (locked to Branch 1).
/// Everything is persisted to SQL Server via EF — no in-memory/static data is served at request time.
/// </summary>
public class DatabaseSeeder
{
    private readonly ApplicationDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(ApplicationDbContext db, IPasswordHasher hasher, ILogger<DatabaseSeeder> logger)
    {
        _db = db; _hasher = hasher; _logger = logger;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await _db.Database.MigrateAsync(ct);

        if (await _db.Tenants.AnyAsync(ct))
        {
            _logger.LogInformation("Seed skipped — tenants already exist.");
            return;
        }

        _logger.LogInformation("Seeding demo tenant + branches + reference data...");

        // ---- Tenant ---------------------------------------------------------
        var tenant = new Tenant { Name = "DemoMilkTeaShop", IsActive = true };

        // ---- Branches (chuỗi 2 chi nhánh) -----------------------------------
        var br1 = new Branch { TenantId = tenant.Id, Name = "Chi nhanh 1 - Quan 1",
                               Address = "123 Nguyen Hue, Q1, TPHCM", IsActive = true };
        var br2 = new Branch { TenantId = tenant.Id, Name = "Chi nhanh 2 - Quan 7",
                               Address = "456 Nguyen Thi Thap, Q7, TPHCM", IsActive = true };

        // ---- Users ----------------------------------------------------------
        var admin = new User
        {
            TenantId = tenant.Id, BranchId = null,
            Username = "admin", PasswordHash = _hasher.Hash("admin123"), Role = UserRole.Admin
        };
        var cashier = new User
        {
            TenantId = tenant.Id, BranchId = br1.Id,
            Username = "cashier", PasswordHash = _hasher.Hash("cashier123"), Role = UserRole.Cashier
        };

        // ---- Categories ------------------------------------------------------
        var catMilkTea  = new Category { TenantId = tenant.Id, Name = "Tra sua",     IsActive = true };
        var catPureTea  = new Category { TenantId = tenant.Id, Name = "Tra thuan",   IsActive = true };
        var catTopping  = new Category { TenantId = tenant.Id, Name = "Topping",     IsActive = true };

        // ---- Ingredients (metadata only) ------------------------------------
        var tea     = new Ingredient { TenantId = tenant.Id, Name = "Tea Leaves",     Unit = "gram" };
        var milk    = new Ingredient { TenantId = tenant.Id, Name = "Fresh Milk",     Unit = "ml" };
        var sugar   = new Ingredient { TenantId = tenant.Id, Name = "Sugar",          Unit = "gram" };
        var tapioca = new Ingredient { TenantId = tenant.Id, Name = "Tapioca Pearls", Unit = "gram" };
        var cup     = new Ingredient { TenantId = tenant.Id, Name = "Cup",            Unit = "pcs" };

        // ---- Products --------------------------------------------------------
        var milkTea   = new Product { TenantId = tenant.Id, CategoryId = catMilkTea.Id, Name = "Milk Tea Classic", Price = 35000, IsActive = true };
        var blackTea  = new Product { TenantId = tenant.Id, CategoryId = catPureTea.Id, Name = "Black Tea",        Price = 25000, IsActive = true };
        var bubbleTea = new Product { TenantId = tenant.Id, CategoryId = catMilkTea.Id, Name = "Bubble Milk Tea",  Price = 45000, IsActive = true };

        _db.Tenants.Add(tenant);
        _db.Branches.AddRange(br1, br2);
        _db.Users.AddRange(admin, cashier);
        _db.Categories.AddRange(catMilkTea, catPureTea, catTopping);
        _db.Ingredients.AddRange(tea, milk, sugar, tapioca, cup);
        _db.Products.AddRange(milkTea, blackTea, bubbleTea);

        // ---- Recipes (tenant-wide BOM) --------------------------------------
        void Recipe(Product p, Ingredient i, decimal qty) =>
            _db.Recipes.Add(new Recipe { TenantId = tenant.Id, ProductId = p.Id, IngredientId = i.Id, QuantityRequired = qty });

        Recipe(milkTea, tea, 20);   Recipe(milkTea, milk, 100); Recipe(milkTea, sugar, 15); Recipe(milkTea, cup, 1);
        Recipe(blackTea, tea, 20);  Recipe(blackTea, sugar, 10); Recipe(blackTea, cup, 1);
        Recipe(bubbleTea, tea, 20); Recipe(bubbleTea, milk, 100); Recipe(bubbleTea, sugar, 15);
        Recipe(bubbleTea, tapioca, 50); Recipe(bubbleTea, cup, 1);

        // ---- Per-branch stock -----------------------------------------------
        // Branch 1 nhập kho đầy đủ; branch 2 nhập ít hơn để dễ test scenario "hết hàng" theo chi nhánh.
        void Stock(Branch br, Ingredient ing, decimal qty) =>
            _db.IngredientStocks.Add(new IngredientStock
            {
                TenantId = tenant.Id, BranchId = br.Id, IngredientId = ing.Id, Quantity = qty
            });

        // Branch 1: full
        Stock(br1, tea, 2000);  Stock(br1, milk, 10000); Stock(br1, sugar, 1000);
        Stock(br1, tapioca, 3000); Stock(br1, cup, 500);

        // Branch 2: thiếu tapioca để demo cross-branch isolation
        Stock(br2, tea, 1500);  Stock(br2, milk, 8000); Stock(br2, sugar, 800);
        Stock(br2, tapioca, 100); Stock(br2, cup, 200);

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Seed complete. Tenant='{Tenant}', Branches=['{B1}','{B2}'], " +
            "Admin='admin/admin123' (any branch), Cashier='cashier/cashier123' (locked to {B1}).",
            tenant.Name, br1.Name, br2.Name, br1.Name);
    }
}
