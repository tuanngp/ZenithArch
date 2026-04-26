using System.Reflection;
using ZenithArch.Abstractions.Attributes;
using ZenithArch.Abstractions.Base;
using ZenithArch.Abstractions.Enums;
using ZenithArch.Abstractions.Interfaces;
using Xunit;

namespace ZenithArch.Abstractions.Tests;

public sealed class AbstractionsContractTests
{
    [Fact]
    public void Entity_base_buffers_and_clears_domain_events()
    {
        var entity = new DemoEntity();

        entity.RaiseDomainEvent(new DemoEvent());
        entity.RaiseDomainEvent(new DemoEvent());

        Assert.Equal(2, entity.DomainEvents.Count);

        entity.ClearDomainEvents();

        Assert.Empty(entity.DomainEvents);
    }

    [Fact]
    public void Entity_base_throws_when_domain_event_is_null()
    {
        var entity = new DemoEntity();

        Assert.Throws<ArgumentNullException>(() => entity.RaiseDomainEvent(null!));
    }

    [Fact]
    public void Entity_base_domain_events_is_read_only_collection()
    {
        var entity = new DemoEntity();
        entity.RaiseDomainEvent(new DemoEvent());

        Assert.IsAssignableFrom<IReadOnlyCollection<IDomainEvent>>(entity.DomainEvents);
    }

    [Fact]
    public void Domain_event_occurrence_timestamp_uses_utc_now()
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
    [InlineData(64)]
    public void Min_length_attribute_preserves_configured_value(int value)
    {
        var attribute = new MinLengthAttribute(value);

        Assert.Equal(value, attribute.Length);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(128)]
    public void Max_length_attribute_preserves_configured_value(int value)
    {
        var attribute = new MaxLengthAttribute(value);

        Assert.Equal(value, attribute.Length);
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
    public void Marker_attributes_are_scoped_to_expected_targets()
    {
        Assert.Equal(AttributeTargets.Assembly, GetTarget(typeof(ArchitectureAttribute)));
        Assert.Equal(AttributeTargets.Class, GetTarget(typeof(EntityAttribute)));
        Assert.Equal(AttributeTargets.Class, GetTarget(typeof(AggregateRootAttribute)));
        Assert.Equal(AttributeTargets.Class, GetTarget(typeof(MapToAttribute)));
        Assert.Equal(AttributeTargets.Property, GetTarget(typeof(QueryFilterAttribute)));
        Assert.Equal(AttributeTargets.Property, GetTarget(typeof(RequiredAttribute)));
        Assert.Equal(AttributeTargets.Property, GetTarget(typeof(EmailAttribute)));
        Assert.Equal(AttributeTargets.Property, GetTarget(typeof(MinLengthAttribute)));
        Assert.Equal(AttributeTargets.Property, GetTarget(typeof(MaxLengthAttribute)));
    }

    [Fact]
    public void Core_enums_keep_expected_numeric_contracts()
    {
        Assert.Equal(0, (int)ArchitecturePattern.Cqrs);
        Assert.Equal(1, (int)ArchitecturePattern.Repository);
        Assert.Equal(2, (int)ArchitecturePattern.FullStack);

        Assert.Equal(0, (int)ArchitectureProfile.Custom);
        Assert.Equal(1, (int)ArchitectureProfile.CqrsQuickStart);
        Assert.Equal(2, (int)ArchitectureProfile.RepositoryQuickStart);
        Assert.Equal(3, (int)ArchitectureProfile.FullStackQuickStart);

        Assert.Equal(0, (int)CqrsSaveMode.PerHandler);
        Assert.Equal(1, (int)CqrsSaveMode.PerRequestTransaction);
    }

    [Fact]
    public void Execution_observer_contract_exposes_expected_methods()
    {
        var methodNames = typeof(IZenithArchExecutionObserver)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Select(m => m.Name)
            .OrderBy(x => x)
            .ToArray();

        Assert.Equal(new[]
        {
            "OnHandlerCompleted",
            "OnHandlerExecuting",
            "OnValidationFailed"
        }, methodNames);
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
