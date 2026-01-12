using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using CodingMCP.Configuration;
using CodingMCP.Executors;

namespace CodingMCP.Tools;

[McpServerToolType]
public class CodeMSBuildDelphiTool
{
    private readonly MSBuildDelphiExecutor _executor;

    public CodeMSBuildDelphiTool(ILogger<CodeMSBuildDelphiTool> logger, ILoggerFactory loggerFactory, CodingSettings settings)
    {
        _executor = new MSBuildDelphiExecutor(loggerFactory.CreateLogger<MSBuildDelphiExecutor>(), settings);
    }

    [McpServerTool, Description("Build Delphi projects using MSBuild. Supports .dproj files.")]
    public async Task<string> BuildDelphiProject(
        [Description("Path to the Delphi project file (.dproj)")] string projectPath,
        [Description("Additional MSBuild options (optional, e.g., '/t:Rebuild' or '/p:Configuration=Release /p:Platform=Win64')")] string? buildOptions = null)
    {
        var result = await _executor.BuildProjectAsync(projectPath, buildOptions);
        return result.ToString();
    }
}
