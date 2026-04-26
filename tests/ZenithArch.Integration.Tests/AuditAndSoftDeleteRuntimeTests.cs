using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ZenithArch.Sample;
using ZenithArch.Sample.Domain.Cqrs;
using Xunit;

namespace ZenithArch.Integration.Tests;

public sealed class AuditAndSoftDeleteRuntimeTests
{
    [Fact]
    public async Task Create_stamps_created_at_with_utc_now()
    {
        await using var host = await IntegrationTestHost.CreateAsync();
        using var scope = host.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var before = DateTime.UtcNow;
        var id = await mediator.Send(TripCommandFactory.Create("Audit"));
        var after = DateTime.UtcNow;

        var entity = await db.Trips.AsNoTracking().SingleAsync(x => x.Id == id);

        Assert.InRange(entity.CreatedAt, before.AddSeconds(-1), after.AddSeconds(1));
    }

    [Fact]
    public async Task Update_and_delete_stamp_last_modified_at()
    {
        await using var host = await IntegrationTestHost.CreateAsync();
        using var scope = host.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var id = await mediator.Send(TripCommandFactory.Create("Audit2"));
        var created = await db.Trips.AsNoTracking().SingleAsync(x => x.Id == id);

        await mediator.Send(TripCommandFactory.UpdateFrom(created, "Audit2"));
        var updated = await db.Trips.AsNoTracking().SingleAsync(x => x.Id == id);

        Assert.NotNull(updated.LastModifiedAt);
        Assert.Equal(created.CreatedAt, updated.CreatedAt);

        await mediator.Send(new DeleteTripCommand(id));
        var deleted = await db.Trips.AsNoTracking().SingleAsync(x => x.Id == id);

        Assert.True(deleted.IsDeleted);
        Assert.NotNull(deleted.LastModifiedAt);
    }

    [Fact]
    public async Task List_query_excludes_soft_deleted_entities()
    {
        await using var host = await IntegrationTestHost.CreateAsync();
        using var scope = host.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var keepId = await mediator.Send(TripCommandFactory.Create("Keep"));
        var deleteId = await mediator.Send(TripCommandFactory.Create("Gone"));

        await mediator.Send(new DeleteTripCommand(deleteId));

        var list = await mediator.Send(new GetTripListQuery { Skip = 0, Take = 50 });

        Assert.Contains(list, x => x.Id == keepId);
        Assert.DoesNotContain(list, x => x.Id == deleteId);
    }
}
