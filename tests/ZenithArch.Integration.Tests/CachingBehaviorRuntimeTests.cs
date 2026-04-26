using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ZenithArch.Sample;
using ZenithArch.Sample.Domain.Cqrs;
using Xunit;

namespace ZenithArch.Integration.Tests;

public sealed class CachingBehaviorRuntimeTests
{
    [Fact]
    public async Task Get_by_id_populates_cache_and_update_invalidates_it()
    {
        await using var host = await IntegrationTestHost.CreateAsync();
        using var scope = host.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var id = await mediator.Send(TripCommandFactory.Create("Cache"));

        _ = await mediator.Send(new GetTripByIdQuery(id));
        var cachedBefore = await IntegrationTestHost.ReadTripCacheAsync(scope.ServiceProvider, id);

        Assert.NotNull(cachedBefore);

        var current = await db.Trips.AsNoTracking().SingleAsync(x => x.Id == id);
        await mediator.Send(TripCommandFactory.UpdateFrom(current, "Cache"));

        var cachedAfter = await IntegrationTestHost.ReadTripCacheAsync(scope.ServiceProvider, id);
        Assert.Null(cachedAfter);
    }
}
