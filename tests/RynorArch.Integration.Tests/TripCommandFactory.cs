using RynorArch.Sample.Domain;
using RynorArch.Sample.Domain.Cqrs;

namespace RynorArch.Integration.Tests;

internal static class TripCommandFactory
{
    public static CreateTripCommand Create(string token = "A", string? title = null)
    {
        var date = DateTime.UtcNow.Date;
        return new CreateTripCommand
        {
            IsDeleted = false,
            CreatedAt = date.AddYears(-1),
            CreatedBy = null,
            LastModifiedAt = null,
            LastModifiedBy = null,
            Title = title ?? $"Trip {token} Title",
            Description = $"Trip {token} description",
            Destination = $"Destination-{token}",
            StartDate = date.AddDays(14),
            EndDate = date.AddDays(20),
            Budget = 2500m,
            CoverImageUrl = "https://example.com/trip-cover.jpg",
            IsPublic = true,
        };
    }

    public static UpdateTripCommand UpdateFrom(Trip entity, string token = "B")
    {
        return new UpdateTripCommand
        {
            Id = entity.Id,
            IsDeleted = entity.IsDeleted,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            LastModifiedAt = entity.LastModifiedAt,
            LastModifiedBy = entity.LastModifiedBy,
            Title = $"Updated {token} Title",
            Description = $"Updated {token} description",
            Destination = $"Updated-{token}",
            StartDate = entity.StartDate.AddDays(1),
            EndDate = entity.EndDate.AddDays(1),
            Budget = entity.Budget + 300m,
            CoverImageUrl = entity.CoverImageUrl,
            IsPublic = entity.IsPublic,
        };
    }
}
