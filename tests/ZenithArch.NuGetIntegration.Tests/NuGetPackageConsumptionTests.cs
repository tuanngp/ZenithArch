using ZenithArch.Abstractions.Attributes;
using ZenithArch.Abstractions.Base;
using ZenithArch.Abstractions.Enums;
using Xunit;

namespace ZenithArch.NuGetIntegration.Tests;

public sealed class NuGetPackageConsumptionTests
{
    [Fact]
    public void Can_consume_public_abstractions_api_from_packed_package()
    {
        var architecture = new ArchitectureAttribute
        {
            Pattern = ArchitecturePattern.Cqrs,
            Profile = ArchitectureProfile.CqrsQuickStart,
            GenerateDependencyInjection = true,
            CqrsSaveMode = CqrsSaveMode.PerHandler,
        };

        var entity = new NuGetEntity();
        entity.RaiseDomainEvent(new NuGetDomainEvent());

        Assert.Single(entity.DomainEvents);
        Assert.Equal(ArchitecturePattern.Cqrs, architecture.Pattern);
    }

    [Entity]
    private sealed partial class NuGetEntity : EntityBase;

    private sealed record NuGetDomainEvent : DomainEvent;
}
