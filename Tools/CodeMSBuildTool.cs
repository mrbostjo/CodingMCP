using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using CodingMCP.Configuration;

namespace CodingMCP.Tools;

[McpServerToolType]
public class CodeMSBuildTool
{
    private readonly ILogger<CodeMSBuildTool> _logger;
    private readonly CodingSettings _settings;

    public CodeMSBuildTool(ILogger<CodeMSBuildTool> logger, CodingSettings settings)
    {
        _logger = logger;
        _settings = settings;
    }

    [McpServerTool, Description("Build projects using MSBuild. Supports Delphi (.dproj), C++ (.vcxproj), C# (.csproj), and other MSBuild-compatible projects. For Delphi: use .dproj files. For C/C++: use .vcxproj files from Visual Studio projects.")]
    public async Task<string> BuildProject(
        [Description("Path to the project file (.dproj for Delphi, .vcxproj for C/C++, .csproj for C#, or .sln for solution)")] string projectPath,
        [Description("Additional MSBuild options (optional, e.g., '/t:Rebuild' or '/p:Configuration=Release /p:Platform=x64')")] string? buildOptions = null)
    {
        _logger.LogInformation("Building project: {ProjectPath}", projectPath);

        try
        {
            var toolConfig = _settings.Tools.MSBuild;
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
                    return $"Error: MSBuild not found at {executablePath}. Please update config.json with the correct path or leave path empty to use PATH.";
                }
            }

            if (!File.Exists(projectPath))
            {
                return $"Error: Project file not found at {projectPath}";
            }

            var arguments = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(buildOptions))
            {
                arguments.Append($"{buildOptions} ");
            }
            arguments.Append($"\"{projectPath}\"");

            var startInfo = new ProcessStartInfo
            {
                FileName = executablePath,
                Arguments = arguments.ToString(),
                WorkingDirectory = Path.GetDirectoryName(projectPath) ?? Directory.GetCurrentDirectory(),
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
                return $"Error: Build timed out after {timeout.TotalSeconds} seconds.";
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
            _logger.LogError(ex, "Error building project: {ProjectPath}", projectPath);
            return $"Error: {ex.Message}";
        }
    }
}
