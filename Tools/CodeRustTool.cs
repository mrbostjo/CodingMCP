using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;
using System.ComponentModel;
using CodingMCP.Configuration;
using CodingMCP.Executors;

namespace CodingMCP.Tools;

[McpServerToolType]
public class CodeRustTool
{
    private readonly CargoExecutor _executor;

    public CodeRustTool(ILogger<CodeRustTool> logger, ILoggerFactory loggerFactory, IOptionsMonitor<CodingSettings> settingsMonitor)
    {
        _executor = new CargoExecutor(loggerFactory.CreateLogger<CargoExecutor>(), settingsMonitor);
    }

    [McpServerTool, Description("Execute Rust cargo commands for building, running, or managing Rust projects")]
    public async Task<string> ExecuteCargoCommand(
        [Description("The cargo command to execute (e.g., 'build', 'run', 'test', 'new myproject')")] string command,
        [Description("Working directory for the command execution (optional)")] string? workingDirectory = null)
    {
        var result = await _executor.ExecuteAsync(command, workingDirectory);
        return result.ToString();
    }
}
