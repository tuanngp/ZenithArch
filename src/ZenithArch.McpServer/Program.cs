using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ZenithArch.McpServer;

internal static class Program
{
	/// <summary>
	/// Configures and runs the MCP server over stdio transport.
	/// </summary>
	public static async Task Main(string[] args)
	{
		HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

		builder.Logging.ClearProviders();
		builder.Logging.AddConsole(options =>
		{
			options.LogToStandardErrorThreshold = LogLevel.Trace;
		});

		builder.Services
			.AddMcpServer()
			.WithStdioServerTransport()
			.WithToolsFromAssembly(Assembly.GetExecutingAssembly())
			.WithPromptsFromAssembly(Assembly.GetExecutingAssembly())
			.WithResourcesFromAssembly(Assembly.GetExecutingAssembly());

		using IHost host = builder.Build();
		await host.RunAsync().ConfigureAwait(false);
	}
}
