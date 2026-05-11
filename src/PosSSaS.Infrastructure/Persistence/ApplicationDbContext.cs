using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PosSSaS.Application.Common.Interfaces;
using PosSSaS.Domain.Common;
using PosSSaS.Domain.Common.Interfaces;
using PosSSaS.Domain.Entities;

namespace PosSSaS.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentUserService _currentUser;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService currentUser)
        : base(options)
    {
        _currentUser = currentUser;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<IngredientStock> IngredientStocks => Set<IngredientStock>();
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        => Database.BeginTransactionAsync(cancellationToken);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Apply scoping filters via reflection. Branch-scoped entities get a stricter
        // (Tenant AND Branch) filter; tenant-only entities get just the tenant filter.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clr = entityType.ClrType;

            if (typeof(IMustHaveBranch).IsAssignableFrom(clr))
            {
                typeof(ApplicationDbContext)
                    .GetMethod(nameof(ApplyBranchFilter), Flags)!
                    .MakeGenericMethod(clr)
                    .Invoke(this, new object[] { modelBuilder });
            }
            else if (typeof(IMustHaveTenant).IsAssignableFrom(clr))
            {
                typeof(ApplicationDbContext)
                    .GetMethod(nameof(ApplyTenantFilter), Flags)!
                    .MakeGenericMethod(clr)
                    .Invoke(this, new object[] { modelBuilder });
            }
        }

        base.OnModelCreating(modelBuilder);
    }

    private const System.Reflection.BindingFlags Flags =
        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

    private void ApplyTenantFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, IMustHaveTenant
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(
            e => !_currentUser.TenantId.HasValue || e.TenantId == _currentUser.TenantId.Value);
    }

    private void ApplyBranchFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, IMustHaveBranch
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e =>
            (!_currentUser.TenantId.HasValue || e.TenantId == _currentUser.TenantId.Value) &&
            (!_currentUser.BranchId.HasValue || e.BranchId == _currentUser.BranchId.Value));
    }

    public override int SaveChanges()
    {
        ApplyAuditAndScope();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditAndScope();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditAndScope()
    {
        var tenantId = _currentUser.TenantId;
        var branchId = _currentUser.BranchId;
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is BaseEntity be)
            {
                if (entry.State == EntityState.Added && be.CreatedAt == default)
                    be.CreatedAt = now;
                if (entry.State == EntityState.Modified)
                    be.UpdatedAt = now;
            }

            if (entry.State != EntityState.Added) continue;

            // Tenant stamping
            if (entry.Entity is IMustHaveTenant tenanted)
            {
                if (tenanted.TenantId == Guid.Empty)
                {
                    if (!tenantId.HasValue)
                        throw new InvalidOperationException(
                            $"Cannot insert {entry.Entity.GetType().Name}: no TenantId on the current principal.");
                    tenanted.TenantId = tenantId.Value;
                }
                else if (tenantId.HasValue && tenanted.TenantId != tenantId.Value)
                {
                    throw new InvalidOperationException(
                        $"TenantId mismatch on insert of {entry.Entity.GetType().Name}.");
                }
            }

            // Branch stamping (subset of tenant-scoped entities)
            if (entry.Entity is IMustHaveBranch branched)
            {
                if (branched.BranchId == Guid.Empty)
                {
                    if (!branchId.HasValue)
                        throw new InvalidOperationException(
                            $"Cannot insert {entry.Entity.GetType().Name}: no BranchId on the current principal.");
                    branched.BranchId = branchId.Value;
                }
                else if (branchId.HasValue && branched.BranchId != branchId.Value)
                {
                    throw new InvalidOperationException(
                        $"BranchId mismatch on insert of {entry.Entity.GetType().Name}.");
                }
            }
        }
    }
}
