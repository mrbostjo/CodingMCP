using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using CodingMCP.Configuration;

namespace CodingMCP.Tools;

[McpServerToolType]
public class CodePythonTool
{
    private readonly ILogger<CodePythonTool> _logger;
    private readonly CodingSettings _settings;

    public CodePythonTool(ILogger<CodePythonTool> logger, CodingSettings settings)
    {
        _logger = logger;
        _settings = settings;
    }

    [McpServerTool, Description("Execute Python scripts or commands using the Python interpreter")]
    public async Task<string> ExecutePythonCommand(
        [Description("The Python command to execute (e.g., 'script.py', '-m pip install package', '-c \"print(hello)\"')")] string command,
        [Description("Working directory for the command execution (optional)")] string? workingDirectory = null)
    {
        _logger.LogInformation("Executing python command: {Command}", command);

        try
        {
            var pythonPath = _settings.Tools.Python.FullPath;
            
            if (!File.Exists(pythonPath))
            {
                return $"Error: python executable not found at {pythonPath}. Please update config.json with the correct path.";
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = pythonPath,
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
            _logger.LogError(ex, "Error executing python command: {Command}", command);
            return $"Error: {ex.Message}";
        }
    }
}
