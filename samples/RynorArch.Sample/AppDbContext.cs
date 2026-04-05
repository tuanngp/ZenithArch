using Microsoft.EntityFrameworkCore;
using RynorArch.Sample.Domain;

namespace RynorArch.Sample;

/// <summary>
/// Application DbContext. Convention-named for RynorArch generated handlers.
/// </summary>
public class AppDbContext : DbContext
{
    public DbSet<Trip> Trips => Set<Trip>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
}
