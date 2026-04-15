using AI.Forged.TourOps.Infrastructure.Configuration;
using AI.Forged.TourOps.Infrastructure.Platform;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Options;

namespace AI.Forged.TourOps.Infrastructure.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=ai_forged_tour_ops;Username=postgres;Password=postgres");
        return new AppDbContext(
            optionsBuilder.Options,
            new TenantExecutionContextAccessor(Options.Create(new DeploymentSettings())));
    }
}
