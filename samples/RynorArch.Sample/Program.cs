using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RynorArch.DependencyInjection;
using RynorArch.Endpoints;
using RynorArch.Sample;
using RynorArch.Sample.Domain;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("rynorarch-sample"));
builder.Services.AddDistributedMemoryCache();
builder.Services.AddRynorArchDependencies<AppDbContext>();

var app = builder.Build();

await SeedSampleDataAsync(app.Services);

app.MapGet("/", () => Results.Ok(new
{
    name = "RynorArch.Sample",
    endpoints = new[]
    {
        "GET /api/trips/{id}",
        "POST /api/trips",
        "PUT /api/trips/{id}",
        "DELETE /api/trips/{id}"
    }
}));
app.MapRynorArchEndpoints();

app.Run();

static async Task SeedSampleDataAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

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
