using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using CodingMCP.Configuration;
using CodingMCP.Tools;

var builder = Host.CreateApplicationBuilder(args);

// Configure CodingSettings from config.json with hot-reload support
builder.Configuration.AddJsonFile(
    Path.Combine(AppContext.BaseDirectory, "config.json"),
    optional: false,
    reloadOnChange: true);

// Register CodingSettings with IOptionsMonitor for hot-reload
builder.Services.Configure<CodingSettings>(
    builder.Configuration);

// Configure logging to stderr for MCP compatibility
builder.Logging.AddConsole(consoleLogOptions =>
{
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

// Register tool classes in DI container
builder.Services.AddSingleton<CodeDotnetTool>();
builder.Services.AddSingleton<CodeRustTool>();
builder.Services.AddSingleton<CodePythonTool>();
builder.Services.AddSingleton<CodeMSBuildTool>();
builder.Services.AddSingleton<CodeMSBuildDelphiTool>();
builder.Services.AddSingleton<CodeDccDelphiTool>();

// Configure MCP server with stdio transport and tool discovery
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

await app.RunAsync();
