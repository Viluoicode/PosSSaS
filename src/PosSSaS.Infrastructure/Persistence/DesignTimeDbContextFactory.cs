using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using PosSSaS.Application.Common.Interfaces;

namespace PosSSaS.Infrastructure.Persistence;

/// <summary>
/// Used ONLY by EF Core tooling (dotnet ef migrations / database update) at design time.
/// Builds the DbContext without spinning up the full web host. Reads the connection string
/// from the API project's appsettings.json so we don't duplicate config.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Walk up to find the API project's appsettings.json regardless of where `dotnet ef` is invoked from.
        var basePath = Directory.GetCurrentDirectory();
        var apiSettingsPath = FindApiSettings(basePath)
            ?? throw new InvalidOperationException("Could not locate PosSSaS.API/appsettings.json");

        var config = new ConfigurationBuilder()
            .SetBasePath(Path.GetDirectoryName(apiSettingsPath)!)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(config.GetConnectionString("DefaultConnection"))
            .Options;

        // At design time there is no HTTP request, so feed the context an empty user service.
        return new ApplicationDbContext(options, new DesignTimeCurrentUser());
    }

    private static string? FindApiSettings(string startDir)
    {
        var dir = new DirectoryInfo(startDir);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, "src", "PosSSaS.API", "appsettings.json");
            if (File.Exists(candidate)) return candidate;

            candidate = Path.Combine(dir.FullName, "PosSSaS.API", "appsettings.json");
            if (File.Exists(candidate)) return candidate;

            // Walk up
            dir = dir.Parent;
        }
        return null;
    }

    private sealed class DesignTimeCurrentUser : ICurrentUserService
    {
        public Guid? UserId => null;
        public Guid? TenantId => null;
        public Guid? BranchId => null;
        public string? Username => null;
        public string? Role => null;
        public bool IsAuthenticated => false;
    }
}
