using RynorArch.Abstractions.Attributes;
using RynorArch.Abstractions.Base;

namespace RynorArch.Sample.Domain;

/// <summary>
/// Sample Trip entity demonstrating all RynorArch attributes.
/// The source generator will produce:
/// - CreateTripCommand, UpdateTripCommand, DeleteTripCommand
/// - GetTripByIdQuery, GetTripListQuery
/// - All corresponding handlers with lifecycle hooks
/// - TripSpecification (from QueryFilter properties)
/// - CreateTripValidator, UpdateTripValidator
/// - TripCreatedEvent, TripUpdatedEvent, TripDeletedEvent (from AggregateRoot)
/// </summary>
[Entity]
[AggregateRoot]
public partial class Trip : EntityBase
{
    [QueryFilter]
    public required string Title { get; set; }

    public required string Description { get; set; }

    [QueryFilter]
    public required string Destination { get; set; }

    public required DateTime StartDate { get; set; }

    public required DateTime EndDate { get; set; }

    [QueryFilter]
    public required decimal Budget { get; set; }

    public string? CoverImageUrl { get; set; }

    [QueryFilter]
    public required bool IsPublic { get; set; }
}
