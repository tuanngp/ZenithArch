using Microsoft.CodeAnalysis;
using RynorArch.Generator.Helpers;
using RynorArch.Generator.Models;

namespace RynorArch.Generator.Emitters;

/// <summary>
/// Generates DDD infrastructure for AggregateRoot entities.
/// Emits domain event record stubs and the aggregate root partial extension.
/// </summary>
internal static class DddEmitter
{
    public static void Emit(SourceProductionContext context, EntityModel entity)
    {
        if (!entity.IsAggregateRoot) return;

        var source = Generate(entity);
        context.AddSource($"{entity.Name}.DomainEvents.g.cs", source);
    }

    internal static string Generate(EntityModel entity)
    {
        var w = new SourceWriter(1024);
        w.AppendFileHeader();

        w.AppendLine("using RynorArch.Abstractions.Base;");
        w.AppendLine();

        if (!string.IsNullOrEmpty(entity.Namespace))
        {
            w.AppendLine($"namespace {entity.Namespace}.DomainEvents;");
            w.AppendLine();
        }

        string name = entity.Name;

        // Created event
        w.AppendLine($"/// <summary>Raised when a new {name} is created.</summary>");
        w.AppendLine($"public sealed record {name}CreatedEvent(Guid {name}Id) : DomainEvent;");
        w.AppendLine();

        // Updated event
        w.AppendLine($"/// <summary>Raised when an existing {name} is updated.</summary>");
        w.AppendLine($"public sealed record {name}UpdatedEvent(Guid {name}Id) : DomainEvent;");
        w.AppendLine();

        // Deleted event
        w.AppendLine($"/// <summary>Raised when a {name} is deleted.</summary>");
        w.AppendLine($"public sealed record {name}DeletedEvent(Guid {name}Id) : DomainEvent;");

        return w.ToString();
    }
}
