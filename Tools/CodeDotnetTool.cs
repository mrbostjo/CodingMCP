using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using CodingMCP.Configuration;
using CodingMCP.Executors;

namespace CodingMCP.Tools;

[McpServerToolType]
public class CodeDotnetTool
{
    private readonly DotnetExecutor _executor;

    public CodeDotnetTool(ILogger<CodeDotnetTool> logger, ILoggerFactory loggerFactory, CodingSettings settings)
    {
        _executor = new DotnetExecutor(loggerFactory.CreateLogger<DotnetExecutor>(), settings);
    }

    [McpServerTool, Description("Execute .NET CLI commands for building, running, or managing .NET projects")]
    public async Task<string> ExecuteDotnetCommand(
        [Description("The dotnet CLI command to execute (e.g., 'build', 'run', 'test', 'new console')")] string command,
        [Description("Working directory for the command execution (optional)")] string? workingDirectory = null)
    {
        var result = await _executor.ExecuteAsync(command, workingDirectory);
        return result.ToString();
    }
}
