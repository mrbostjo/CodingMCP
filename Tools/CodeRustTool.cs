using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using CodingMCP.Configuration;

namespace CodingMCP.Tools;

[McpServerToolType]
public class CodeRustTool
{
    private readonly ILogger<CodeRustTool> _logger;
    private readonly CodingSettings _settings;

    public CodeRustTool(ILogger<CodeRustTool> logger, CodingSettings settings)
    {
        _logger = logger;
        _settings = settings;
    }

    [McpServerTool, Description("Execute Rust cargo commands for building, running, or managing Rust projects")]
    public async Task<string> ExecuteCargoCommand(
        [Description("The cargo command to execute (e.g., 'build', 'run', 'test', 'new myproject')")] string command,
        [Description("Working directory for the command execution (optional)")] string? workingDirectory = null)
    {
        _logger.LogInformation("Executing cargo command: {Command}", command);

        try
        {
            var toolConfig = _settings.Tools.Rust;
            string executablePath;
            
            // If path is empty, use just the executable name (assume it's in PATH)
            if (string.IsNullOrWhiteSpace(toolConfig.Path))
            {
                executablePath = toolConfig.ExecutableName;
                _logger.LogInformation("Using executable from PATH: {Executable}", executablePath);
            }
            else
            {
                // Path is configured, use full path and verify it exists
                executablePath = toolConfig.FullPath;
                if (!File.Exists(executablePath))
                {
                    return $"Error: cargo executable not found at {executablePath}. Please update config.json with the correct path or leave path empty to use PATH.";
                }
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = executablePath,
                Arguments = command,
                WorkingDirectory = workingDirectory ?? Directory.GetCurrentDirectory(),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    outputBuilder.AppendLine(e.Data);
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    errorBuilder.AppendLine(e.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            var timeout = TimeSpan.FromSeconds(_settings.Features.DefaultTimeout);
            if (!process.WaitForExit((int)timeout.TotalMilliseconds))
            {
                process.Kill();
                return $"Error: Command execution timed out after {timeout.TotalSeconds} seconds.";
            }

            var output = outputBuilder.ToString();
            var error = errorBuilder.ToString();

            var result = new StringBuilder();
            result.AppendLine($"Exit Code: {process.ExitCode}");
            
            if (!string.IsNullOrWhiteSpace(output))
            {
                result.AppendLine("\n=== Output ===");
                result.AppendLine(output);
            }

            if (!string.IsNullOrWhiteSpace(error))
            {
                result.AppendLine("\n=== Errors/Warnings ===");
                result.AppendLine(error);
            }

            return result.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing cargo command: {Command}", command);
            return $"Error: {ex.Message}";
        }
    }
}
