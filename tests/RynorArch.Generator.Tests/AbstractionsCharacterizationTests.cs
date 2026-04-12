using RynorArch.Abstractions.Attributes;
using RynorArch.Abstractions.Base;
using RynorArch.Abstractions.Enums;
using RynorArch.Abstractions.Interfaces;
using Xunit;

namespace RynorArch.Generator.Tests;

public sealed class AbstractionsCharacterizationTests
{
    [Fact]
    public void Entity_base_accumulates_and_clears_domain_events()
    {
        var entity = new DemoEntity();

        entity.RaiseDomainEvent(new DemoEvent());
        entity.RaiseDomainEvent(new DemoEvent());

        Assert.Equal(2, entity.DomainEvents.Count);

        entity.ClearDomainEvents();

        Assert.Empty(entity.DomainEvents);
    }

    [Fact]
    public void Entity_base_throws_for_null_domain_event()
    {
        var entity = new DemoEntity();

        Assert.Throws<ArgumentNullException>(() => entity.RaiseDomainEvent(null!));
    }

    [Fact]
    public void Domain_event_sets_occurrence_time_to_utc_now()
    {
        var before = DateTime.UtcNow;
        var evt = new DemoEvent();
        var after = DateTime.UtcNow;

        Assert.InRange(evt.OccurredOn, before.AddSeconds(-1), after.AddSeconds(1));
    }

    [Fact]
    public void Architecture_attribute_has_expected_defaults()
    {
        var attribute = new ArchitectureAttribute();

        Assert.Equal(ArchitectureProfile.Custom, attribute.Profile);
        Assert.Equal(ArchitecturePattern.Cqrs, attribute.Pattern);
        Assert.Equal(CqrsSaveMode.PerHandler, attribute.CqrsSaveMode);
        Assert.False(attribute.UseSpecification);
        Assert.False(attribute.UseUnitOfWork);
        Assert.False(attribute.EnableValidation);
        Assert.False(attribute.GenerateDependencyInjection);
        Assert.False(attribute.GenerateEndpoints);
        Assert.False(attribute.EnableExperimentalEndpoints);
        Assert.False(attribute.GenerateDtos);
        Assert.False(attribute.GenerateEfConfigurations);
        Assert.False(attribute.GenerateCachingDecorators);
        Assert.False(attribute.GeneratePagination);
        Assert.Null(attribute.DbContextType);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(42)]
    public void Min_length_attribute_preserves_configured_length(int length)
    {
        var attribute = new MinLengthAttribute(length);

        Assert.Equal(length, attribute.Length);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(128)]
    public void Max_length_attribute_preserves_configured_length(int length)
    {
        var attribute = new MaxLengthAttribute(length);

        Assert.Equal(length, attribute.Length);
    }

    [Fact]
    public void Map_to_attribute_preserves_target_type()
    {
        var attribute = new MapToAttribute(typeof(DemoEntity));

        Assert.Equal(typeof(DemoEntity), attribute.TargetType);
    }

    [Fact]
    public void Map_to_attribute_throws_when_target_type_is_null()
    {
        Assert.Throws<ArgumentNullException>(() => new MapToAttribute(null!));
    }

    [Fact]
    public void Marker_attributes_are_applicable_to_expected_targets()
    {
        Assert.Equal(AttributeTargets.Class, GetTarget(typeof(EntityAttribute)));
        Assert.Equal(AttributeTargets.Class, GetTarget(typeof(AggregateRootAttribute)));
        Assert.Equal(AttributeTargets.Class, GetTarget(typeof(MapToAttribute)));
        Assert.Equal(AttributeTargets.Property, GetTarget(typeof(QueryFilterAttribute)));
        Assert.Equal(AttributeTargets.Property, GetTarget(typeof(RequiredAttribute)));
        Assert.Equal(AttributeTargets.Property, GetTarget(typeof(EmailAttribute)));
        Assert.Equal(AttributeTargets.Property, GetTarget(typeof(MinLengthAttribute)));
        Assert.Equal(AttributeTargets.Property, GetTarget(typeof(MaxLengthAttribute)));
    }

    private static AttributeTargets GetTarget(Type attributeType)
    {
        var usage = (AttributeUsageAttribute?)Attribute.GetCustomAttribute(attributeType, typeof(AttributeUsageAttribute));
        Assert.NotNull(usage);
        return usage!.ValidOn;
    }

    private sealed class DemoEntity : EntityBase;

    private sealed record DemoEvent : DomainEvent;
}
