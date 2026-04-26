using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using ZenithArch.DependencyInjection;
using ZenithArch.Sample;

namespace ZenithArch.Integration.Tests;

internal sealed class IntegrationTestHost : IAsyncDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ServiceProvider _provider;

    private IntegrationTestHost(SqliteConnection connection, ServiceProvider provider)
    {
        _connection = connection;
        _provider = provider;
    }

    public static async Task<IntegrationTestHost> CreateAsync(Action<IServiceCollection>? configureServices = null)
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options => options.UseSqlite(connection));
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddDistributedMemoryCache();
        services.AddZenithArchDependencies<AppDbContext>();

        configureServices?.Invoke(services);

        var provider = services.BuildServiceProvider(validateScopes: true);

        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureCreatedAsync();
        }

        return new IntegrationTestHost(connection, provider);
    }

    public IServiceScope CreateScope() => _provider.CreateScope();

    public async ValueTask DisposeAsync()
    {
        await _provider.DisposeAsync();
        await _connection.DisposeAsync();
    }

    public static string BuildTripCacheKey(Guid id) => "Trip_" + id;

    public static async Task<byte[]?> ReadTripCacheAsync(IServiceProvider services, Guid id)
    {
        var cache = services.GetRequiredService<IDistributedCache>();
        return await cache.GetAsync(BuildTripCacheKey(id));
    }
}
