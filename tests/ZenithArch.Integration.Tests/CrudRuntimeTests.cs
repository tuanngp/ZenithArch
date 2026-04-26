using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ZenithArch.Sample;
using ZenithArch.Sample.Domain.Cqrs;
using Xunit;

namespace ZenithArch.Integration.Tests;

public sealed class CrudRuntimeTests
{
    [Fact]
    public async Task Create_and_get_by_id_roundtrip_persists_entity()
    {
        await using var host = await IntegrationTestHost.CreateAsync();
        using var scope = host.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var id = await mediator.Send(TripCommandFactory.Create("Create"));
        var entity = await mediator.Send(new GetTripByIdQuery(id));

        Assert.NotNull(entity);
        Assert.Equal(id, entity!.Id);
        Assert.Equal("Trip Create Title", entity.Title);
    }

    [Fact]
    public async Task Update_command_modifies_existing_entity()
    {
        await using var host = await IntegrationTestHost.CreateAsync();
        using var scope = host.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var id = await mediator.Send(TripCommandFactory.Create("Update"));
        var current = await db.Trips.AsNoTracking().SingleAsync(x => x.Id == id);

        var success = await mediator.Send(TripCommandFactory.UpdateFrom(current, "Update"));
        var updated = await mediator.Send(new GetTripByIdQuery(id));

        Assert.True(success);
        Assert.NotNull(updated);
        Assert.Equal("Updated Update Title", updated!.Title);
        Assert.Equal(current.Budget + 300m, updated.Budget);
    }

    [Fact]
    public async Task Delete_command_soft_deletes_and_hides_entity_from_queries()
    {
        await using var host = await IntegrationTestHost.CreateAsync();
        using var scope = host.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var id = await mediator.Send(TripCommandFactory.Create("Delete"));

        var deleted = await mediator.Send(new DeleteTripCommand(id));
        var afterDelete = await mediator.Send(new GetTripByIdQuery(id));
        var row = await db.Trips.SingleAsync(x => x.Id == id);

        Assert.True(deleted);
        Assert.Null(afterDelete);
        Assert.True(row.IsDeleted);
    }
}
