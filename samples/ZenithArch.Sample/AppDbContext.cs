using Microsoft.EntityFrameworkCore;
using ZenithArch.Sample.Domain;

namespace ZenithArch.Sample;

/// <summary>
/// Application DbContext. Convention-named for ZenithArch generated handlers.
/// </summary>
public class AppDbContext : DbContext
{
    public DbSet<Trip> Trips => Set<Trip>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
}
