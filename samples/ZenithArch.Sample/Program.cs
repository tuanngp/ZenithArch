using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using ZenithArch.DependencyInjection;
using ZenithArch.Endpoints;
using ZenithArch.Sample;
using ZenithArch.Sample.Domain;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("zenitharch-sample")
           .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning)));
builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<AppDbContext>());
builder.Services.AddDistributedMemoryCache();
builder.Services.AddZenithArchDependencies<AppDbContext>();

var app = builder.Build();

await SeedSampleDataAsync(app.Services);

app.MapGet("/", () => Results.Ok(new
{
    name = "ZenithArch.Sample",
    endpoints = new[]
    {
        "GET /api/trips/{id}",
        "POST /api/trips",
        "PUT /api/trips/{id}",
        "DELETE /api/trips/{id}"
    }
}));
app.MapZenithArchEndpoints();

app.Run();

static async Task SeedSampleDataAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    await db.Database.EnsureCreatedAsync();

    if (await db.Trips.AnyAsync())
    {
        return;
    }

    db.Trips.Add(new Trip
    {
        Title = "Nordic Expedition",
        Description = "A seeded sample trip for local end-to-end verification.",
        Destination = "Iceland",
        StartDate = DateTime.UtcNow.Date.AddDays(14),
        EndDate = DateTime.UtcNow.Date.AddDays(21),
        Budget = 2400m,
        IsPublic = true,
        CoverImageUrl = "https://example.com/trip-cover.jpg"
    });

    await db.SaveChangesAsync();
}

public partial class Program;
