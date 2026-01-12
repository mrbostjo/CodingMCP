using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;
using System.ComponentModel;
using CodingMCP.Configuration;
using CodingMCP.Executors;

namespace CodingMCP.Tools;

[McpServerToolType]
public class CodePythonTool
{
    private readonly PythonExecutor _executor;

    public CodePythonTool(ILogger<CodePythonTool> logger, ILoggerFactory loggerFactory, IOptionsMonitor<CodingSettings> settingsMonitor)
    {
        _executor = new PythonExecutor(loggerFactory.CreateLogger<PythonExecutor>(), settingsMonitor);
    }

    [McpServerTool, Description("Execute Python scripts or commands using the Python interpreter")]
    public async Task<string> ExecutePythonCommand(
        [Description("The Python command to execute (e.g., 'script.py', '-m pip install package', '-c \"print(hello)\"')")] string command,
        [Description("Working directory for the command execution (optional)")] string? workingDirectory = null)
    {
        var result = await _executor.ExecuteAsync(command, workingDirectory);
        return result.ToString();
    }
}
