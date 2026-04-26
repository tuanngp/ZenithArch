# ZenithArch.McpServer

`ZenithArch.McpServer` is a stdio-based Model Context Protocol (MCP) server implemented in C#.

## What It Exposes

- `read_workspace_file`: Read a UTF-8 text file from the workspace using a relative path.
- `list_workspace_files`: List files in a workspace-relative directory, with optional extension filtering.

Both tools enforce path-safety checks so callers cannot escape the workspace root.

## Run Locally

From repository root:

```powershell
dotnet run --project src/ZenithArch.McpServer/ZenithArch.McpServer.csproj
```

The server uses stdio transport, so it will wait for an MCP client to connect.

## VS Code MCP Configuration Example

Create or update `.vscode/mcp.json`:

```json
{
  "servers": {
    "ZenithArchMcpServer": {
      "type": "stdio",
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "src/ZenithArch.McpServer/ZenithArch.McpServer.csproj"
      ]
    }
  }
}
```

## Test Prompts

- "List C# files in src/ZenithArch.Cli using list_workspace_files with extension .cs."
- "Read src/ZenithArch.Cli/Program.cs with read_workspace_file and maxCharacters 1500."

## Troubleshooting

- If the server fails to start, run `dotnet build src/ZenithArch.McpServer/ZenithArch.McpServer.csproj` and fix compile errors first.
- If the tool is visible but not called, explicitly mention the tool name in your prompt.
- Logs are sent to stderr to avoid breaking stdio protocol traffic.