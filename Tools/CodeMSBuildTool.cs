using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using CodingMCP.Configuration;
using CodingMCP.Executors;

namespace CodingMCP.Tools;

[McpServerToolType]
public class CodeMSBuildTool
{
    private readonly MSBuildExecutor _executor;

    public CodeMSBuildTool(ILogger<CodeMSBuildTool> logger, ILoggerFactory loggerFactory, CodingSettings settings)
    {
        _executor = new MSBuildExecutor(loggerFactory.CreateLogger<MSBuildExecutor>(), settings);
    }

    [McpServerTool, Description("Build projects using MSBuild. Supports Delphi (.dproj), C++ (.vcxproj), C# (.csproj), and other MSBuild-compatible projects. For Delphi: use .dproj files. For C/C++: use .vcxproj files from Visual Studio projects.")]
    public async Task<string> BuildProject(
        [Description("Path to the project file (.dproj for Delphi, .vcxproj for C/C++, .csproj for C#, or .sln for solution)")] string projectPath,
        [Description("Additional MSBuild options (optional, e.g., '/t:Rebuild' or '/p:Configuration=Release /p:Platform=x64')")] string? buildOptions = null)
    {
        var result = await _executor.BuildProjectAsync(projectPath, buildOptions);
        return result.ToString();
    }
}
