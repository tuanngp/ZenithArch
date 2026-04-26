using System.ComponentModel;
using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace ZenithArch.McpServer.Tools;

/// <summary>
/// File tools constrained to the current workspace root.
/// </summary>
[McpServerToolType]
[Description("Workspace-scoped file tools with strong path validation and safe defaults.")]
public sealed class WorkspaceFileTools
{
    private const int MinMaxCharacters = 1;
    private const int MaxMaxCharacters = 20000;
    private const int MinFileLimit = 1;
    private const int MaxFileLimit = 500;

    private readonly ILogger<WorkspaceFileTools> _logger;
    private readonly string _workspaceRoot;

    public WorkspaceFileTools(IHostEnvironment hostEnvironment, ILogger<WorkspaceFileTools> logger)
    {
        _logger = logger;
        _workspaceRoot = string.IsNullOrWhiteSpace(hostEnvironment.ContentRootPath)
            ? Directory.GetCurrentDirectory()
            : hostEnvironment.ContentRootPath;
    }

    /// <summary>
    /// Reads a UTF-8 text file relative to the workspace root.
    /// </summary>
    [McpServerTool(Name = "read_workspace_file")]
    [Description("Reads a UTF-8 text file from the workspace by relative path.")]
    public async Task<string> ReadWorkspaceFileAsync(
        [Description("Workspace-relative file path, for example src/ZenithArch.Cli/Program.cs.")] string relativePath,
        [Description("Maximum characters to return. Allowed range: 1 to 20000.")] int maxCharacters = 8000,
        CancellationToken cancellationToken = default)
    {
        EnsureHasValue(relativePath, nameof(relativePath));
        EnsureRange(maxCharacters, MinMaxCharacters, MaxMaxCharacters, nameof(maxCharacters));

        string fullPath = ResolveAndValidatePath(relativePath);

        if (!File.Exists(fullPath))
        {
            throw new McpProtocolException(
                $"File was not found: {relativePath}",
                McpErrorCode.ResourceNotFound);
        }

        if (Directory.Exists(fullPath))
        {
            throw new McpProtocolException(
                $"Path points to a directory, not a file: {relativePath}",
                McpErrorCode.InvalidParams);
        }

        _logger.LogInformation("Reading file {RelativePath} (max chars: {MaxCharacters})", relativePath, maxCharacters);

        try
        {
            string content = await File.ReadAllTextAsync(fullPath, Encoding.UTF8, cancellationToken).ConfigureAwait(false);
            if (content.Length <= maxCharacters)
            {
                return content;
            }

            return content[..maxCharacters] + "\n\n... [truncated]";
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed reading file {RelativePath}", relativePath);
            throw new McpProtocolException(
                $"Unable to read file '{relativePath}'.",
                ex,
                McpErrorCode.InternalError);
        }
    }

    /// <summary>
    /// Lists files under a workspace-relative directory.
    /// </summary>
    [McpServerTool(Name = "list_workspace_files")]
    [Description("Lists files in a workspace directory with optional extension filter.")]
    public string[] ListWorkspaceFiles(
        [Description("Workspace-relative directory path. Use '.' for repository root.")] string relativeDirectory = ".",
        [Description("Optional file extension filter including the leading dot, for example .cs or .md.")] string? extension = null,
        [Description("Maximum number of files to return. Allowed range: 1 to 500.")] int limit = 100)
    {
        EnsureHasValue(relativeDirectory, nameof(relativeDirectory));
        EnsureRange(limit, MinFileLimit, MaxFileLimit, nameof(limit));

        if (!string.IsNullOrWhiteSpace(extension) && !IsValidExtension(extension))
        {
            throw new McpProtocolException(
                "Extension must start with '.' and cannot contain path separators or wildcard characters.",
                McpErrorCode.InvalidParams);
        }

        string directoryPath = ResolveAndValidatePath(relativeDirectory);
        if (!Directory.Exists(directoryPath))
        {
            throw new McpProtocolException(
                $"Directory was not found: {relativeDirectory}",
                McpErrorCode.ResourceNotFound);
        }

        _logger.LogInformation(
            "Listing files in {RelativeDirectory} (extension: {Extension}, limit: {Limit})",
            relativeDirectory,
            extension ?? "<none>",
            limit);

        StringComparison comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        IEnumerable<string> filePaths = Directory.EnumerateFiles(directoryPath, "*", SearchOption.TopDirectoryOnly)
            .OrderBy(path => path, StringComparer.Ordinal);

        if (!string.IsNullOrWhiteSpace(extension))
        {
            filePaths = filePaths.Where(path => path.EndsWith(extension, comparison));
        }

        return filePaths
            .Take(limit)
            .Select(path => Path.GetRelativePath(_workspaceRoot, path).Replace('\\', '/'))
            .ToArray();
    }

    private static void EnsureHasValue(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new McpProtocolException(
                $"Parameter '{parameterName}' is required.",
                McpErrorCode.InvalidParams);
        }
    }

    private static void EnsureRange(int value, int minInclusive, int maxInclusive, string parameterName)
    {
        if (value < minInclusive || value > maxInclusive)
        {
            throw new McpProtocolException(
                $"Parameter '{parameterName}' must be between {minInclusive} and {maxInclusive}.",
                McpErrorCode.InvalidParams);
        }
    }

    private static bool IsValidExtension(string extension)
    {
        return extension.StartsWith(".", StringComparison.Ordinal)
            && !extension.Contains(Path.DirectorySeparatorChar)
            && !extension.Contains(Path.AltDirectorySeparatorChar)
            && !extension.Contains('*')
            && !extension.Contains('?');
    }

    private string ResolveAndValidatePath(string relativePath)
    {
        string candidate = Path.GetFullPath(Path.Combine(_workspaceRoot, relativePath));
        string normalizedRoot = EnsureTrailingSeparator(Path.GetFullPath(_workspaceRoot));

        StringComparison comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        if (!candidate.StartsWith(normalizedRoot, comparison))
        {
            throw new McpProtocolException(
                $"Path escapes workspace root: {relativePath}",
                McpErrorCode.InvalidParams);
        }

        return candidate;
    }

    private static string EnsureTrailingSeparator(string path)
    {
        if (path.EndsWith(Path.DirectorySeparatorChar) || path.EndsWith(Path.AltDirectorySeparatorChar))
        {
            return path;
        }

        return path + Path.DirectorySeparatorChar;
    }
}