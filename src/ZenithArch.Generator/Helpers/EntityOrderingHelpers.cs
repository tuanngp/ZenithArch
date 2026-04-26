using System.Collections.Immutable;
using ZenithArch.Generator.Models;

namespace ZenithArch.Generator.Helpers;

internal static class EntityOrderingHelpers
{
    public static EntityModel[] SortByNamespaceThenName(ImmutableArray<EntityModel> entities)
    {
        var sorted = new EntityModel[entities.Length];
        for (int i = 0; i < entities.Length; i++)
        {
            sorted[i] = entities[i];
        }

        Array.Sort(sorted, static (x, y) =>
        {
            int namespaceCompare = StringComparer.Ordinal.Compare(x.Namespace, y.Namespace);
            if (namespaceCompare != 0)
            {
                return namespaceCompare;
            }

            return StringComparer.Ordinal.Compare(x.Name, y.Name);
        });

        return sorted;
    }

    public static List<string> SortNamespaces(HashSet<string> namespaces)
    {
        var list = new List<string>(namespaces.Count);
        foreach (var ns in namespaces)
        {
            list.Add(ns);
        }

        list.Sort(StringComparer.Ordinal);
        return list;
    }
}