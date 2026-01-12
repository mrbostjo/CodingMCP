using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;
using CodingMCP.Configuration;

namespace CodingMCP.Executors;

public abstract class CommandExecutor
{
    protected readonly ILogger Logger;
    protected readonly CodingSettings Settings;
    protected readonly ToolConfig ToolConfig;

    protected CommandExecutor(ILogger logger, CodingSettings settings, ToolConfig toolConfig)
    {
        Logger = logger;
        Settings = settings;
        ToolConfig = toolConfig;
    }

    /// <summary>
    /// Execute a command with the configured tool
    /// </summary>
    public async Task<ExecutionResult> ExecuteAsync(string command, string? workingDirectory = null)
    {
        var toolName = GetToolName();
        Logger.LogInformation("Executing {ToolName} command: {Command}", toolName, command);

        try
        {
            // Get the executable path
            var executablePath = GetExecutablePath();
            if (executablePath == null)
            {
                return new ExecutionResult
                {
                    ErrorMessage = $"{toolName} executable not found at {ToolConfig.FullPath}. " +
                                   "Please update config.json with the correct path or leave path empty to use PATH."
                };
            }

            // Allow derived classes to perform pre-execution setup
            var preExecuteResult = await PreExecuteAsync(command, workingDirectory);
            if (!string.IsNullOrWhiteSpace(preExecuteResult))
            {
                return new ExecutionResult { ErrorMessage = preExecuteResult };
            }

            // Format arguments (allow customization by derived classes)
            var arguments = FormatArguments(command);

            // Create and configure the process
            var startInfo = CreateProcessStartInfo(
                executablePath,
                arguments,
                workingDirectory ?? Directory.GetCurrentDirectory()
            );

            // Allow derived classes to modify process start info
            ModifyProcessStartInfo(startInfo);

            // Execute the process
            var result = await ExecuteProcessAsync(startInfo);

            // Allow derived classes to perform post-execution cleanup/processing
            await PostExecuteAsync(result);

            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error executing {ToolName} command: {Command}", toolName, command);
            return new ExecutionResult { ErrorMessage = ex.Message };
        }
    }

    /// <summary>
    /// Get the executable path, handling both PATH and explicit path configurations
    /// </summary>
    protected virtual string? GetExecutablePath()
    {
        if (string.IsNullOrWhiteSpace(ToolConfig.Path))
        {
            var executableName = ToolConfig.ExecutableName;
            Logger.LogInformation("Using executable from PATH: {Executable}", executableName);
            return executableName;
        }
        
        var fullPath = ToolConfig.FullPath;
        if (!File.Exists(fullPath))
        {
            return null;
        }
        
        return fullPath;
    }

    /// <summary>
    /// Create the ProcessStartInfo with common settings
    /// </summary>
    protected virtual ProcessStartInfo CreateProcessStartInfo(string executable, string arguments, string workingDirectory)
    {
        return new ProcessStartInfo
        {
            FileName = executable,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
    }

    /// <summary>
    /// Execute the process and capture output
    /// </summary>
    protected virtual async Task<ExecutionResult> ExecuteProcessAsync(ProcessStartInfo startInfo)
    {
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

        var timeout = TimeSpan.FromSeconds(Settings.Features.DefaultTimeout);
        var completed = process.WaitForExit((int)timeout.TotalMilliseconds);

        if (!completed)
        {
            process.Kill();
            return new ExecutionResult
            {
                TimedOut = true,
                ErrorMessage = $"Command execution timed out after {timeout.TotalSeconds} seconds.",
                Output = outputBuilder.ToString(),
                Error = errorBuilder.ToString()
            };
        }

        return new ExecutionResult
        {
            ExitCode = process.ExitCode,
            Output = outputBuilder.ToString(),
            Error = errorBuilder.ToString()
        };
    }

    // Virtual hooks for derived classes to customize behavior

    /// <summary>
    /// Get the name of the tool for logging
    /// </summary>
    protected abstract string GetToolName();

    /// <summary>
    /// Format command arguments. Override for tool-specific formatting.
    /// </summary>
    protected virtual string FormatArguments(string command) => command;

    /// <summary>
    /// Called before execution. Return error message to abort, or null/empty to continue.
    /// </summary>
    protected virtual Task<string?> PreExecuteAsync(string command, string? workingDirectory)
    {
        return Task.FromResult<string?>(null);
    }

    /// <summary>
    /// Modify ProcessStartInfo before execution. Override to add environment variables, etc.
    /// </summary>
    protected virtual void ModifyProcessStartInfo(ProcessStartInfo startInfo)
    {
        // Base implementation does nothing
    }

    /// <summary>
    /// Called after execution. Override for cleanup or result processing.
    /// </summary>
    protected virtual Task PostExecuteAsync(ExecutionResult result)
    {
        return Task.CompletedTask;
    }
}
