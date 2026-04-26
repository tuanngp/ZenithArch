using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ZenithArch.Generated.Infrastructure;
using ZenithArch.Sample;
using ZenithArch.Sample.Domain;
using ZenithArch.Sample.Domain.Cqrs;
using Xunit;

namespace ZenithArch.Integration.Tests;

public sealed class ValidationAndTransactionRuntimeTests
{
    [Fact]
    public async Task Validation_failure_prevents_persistence()
    {
        await using var host = await IntegrationTestHost.CreateAsync();
        using var scope = host.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var invalidCommand = TripCommandFactory.Create("Invalid", title: "abc");

        await Assert.ThrowsAsync<ValidationException>(() => mediator.Send(invalidCommand));

        var count = await db.Trips.CountAsync();
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task Per_request_transaction_rolls_back_when_handler_throws()
    {
        await using var host = await IntegrationTestHost.CreateAsync(services =>
        {
            services.AddScoped<IRequestHandler<FailingWriteCommand, Unit>, FailingWriteCommandHandler>();
        });

        using var scope = host.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await Assert.ThrowsAsync<InvalidOperationException>(() => mediator.Send(new FailingWriteCommand()));

        var count = await db.Trips.CountAsync();
        Assert.Equal(0, count);
    }

    private sealed record FailingWriteCommand : IRequest<Unit>, IZenithArchWriteCommand;

    private sealed class FailingWriteCommandHandler : IRequestHandler<FailingWriteCommand, Unit>
    {
        private readonly AppDbContext _db;

        public FailingWriteCommandHandler(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Unit> Handle(FailingWriteCommand request, CancellationToken cancellationToken)
        {
            _db.Trips.Add(new Trip
            {
                Title = "Transient Transaction Title",
                Description = "Should rollback",
                Destination = "Rollback",
                StartDate = DateTime.UtcNow.Date.AddDays(10),
                EndDate = DateTime.UtcNow.Date.AddDays(12),
                Budget = 1500m,
                IsPublic = true,
            });

            await _db.SaveChangesAsync(cancellationToken);
            throw new InvalidOperationException("Rollback test");
        }
    }
}
