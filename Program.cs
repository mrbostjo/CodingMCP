using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using CodingMCP.Configuration;
using CodingMCP.Tools;

var builder = Host.CreateApplicationBuilder(args);

// Load configuration from config.json
var configLoader = new ConfigurationLoader(
    Path.Combine(AppContext.BaseDirectory, "config.json"));
var codingSettings = configLoader.LoadConfiguration();

// Register configuration loader and settings
builder.Services.AddSingleton<IConfigurationLoader>(configLoader);
builder.Services.AddSingleton(codingSettings);

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

// Configure MCP server with stdio transport and tool discovery
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

await app.RunAsync();
