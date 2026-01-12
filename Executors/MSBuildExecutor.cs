using Microsoft.Extensions.Logging;
using System.Text;
using CodingMCP.Configuration;

namespace CodingMCP.Executors;

public class MSBuildExecutor : CommandExecutor
{
    private string? _projectPath;

    public MSBuildExecutor(ILogger<MSBuildExecutor> logger, CodingSettings settings)
        : base(logger, settings, settings.Tools.MSBuild)
    {
    }

    protected override string GetToolName() => "MSBuild";

    /// <summary>
    /// Execute MSBuild for a specific project file
    /// </summary>
    public async Task<ExecutionResult> BuildProjectAsync(string projectPath, string? buildOptions = null)
    {
        _projectPath = projectPath;
        
        // Combine build options and project path
        var command = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(buildOptions))
        {
            command.Append($"{buildOptions} ");
        }
        command.Append($"\"{projectPath}\"");

        // Use the directory of the project as working directory
        var workingDirectory = Path.GetDirectoryName(projectPath) ?? Directory.GetCurrentDirectory();
        
        return await ExecuteAsync(command.ToString(), workingDirectory);
    }

    protected override async Task<string?> PreExecuteAsync(string command, string? workingDirectory)
    {
        // Validate project file exists
        if (_projectPath != null && !File.Exists(_projectPath))
        {
            return $"Project file not found at {_projectPath}";
        }
        
        return await base.PreExecuteAsync(command, workingDirectory);
    }
}
