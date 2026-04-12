namespace RynorArch.Generator.Helpers;

internal static class NamespaceHelpers
{
    private const string GeneratedNamespace = "RynorArch.Generated";

    public static string Compose(string? rootNamespace, string suffix)
    {
        if (string.IsNullOrEmpty(rootNamespace))
        {
            return GeneratedNamespace;
        }

        return $"{rootNamespace}.{suffix}";
    }
}