using ZenithArch.Abstractions.Attributes;
using ZenithArch.Abstractions.Base;
using ZenithArch.Abstractions.Interfaces;

namespace ZenithArch.Sample.Domain;

/// <summary>
/// Sample Trip entity demonstrating all ZenithArch attributes.
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
public partial class Trip : EntityBase, ISoftDelete, IAuditable
{
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
    [QueryFilter]
    [Required]
    [MinLength(5)]
    [MaxLength(100)]
    public required string Title { get; set; }

    [Required]
    [MaxLength(500)]
    public required string Description { get; set; }

    [QueryFilter]
    [Required]
    public required string Destination { get; set; }

    public required DateTime StartDate { get; set; }

    public required DateTime EndDate { get; set; }

    [QueryFilter]
    public required decimal Budget { get; set; }

    public string? CoverImageUrl { get; set; }

    [QueryFilter]
    public required bool IsPublic { get; set; }
}
