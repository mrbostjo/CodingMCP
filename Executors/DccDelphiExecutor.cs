using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.IO;
using System.Text;
using CodingMCP.Configuration;

namespace CodingMCP.Executors;

public class DccDelphiExecutor : CommandExecutor
{
    private string? _dprPath;
    private string _architecture = "Win32";

    public DccDelphiExecutor(ILogger<DccDelphiExecutor> logger, IOptionsMonitor<CodingSettings> settingsMonitor)
        : base(logger, settingsMonitor, () => settingsMonitor.CurrentValue.Tools.MSBuildDelphi)
    {
    }

    protected override string GetToolName() => "DCC Delphi";

    /// <summary>
    /// Compile a Delphi DPR using DCC32 or DCC64 depending on architecture
    /// </summary>
    public async Task<ExecutionResult> CompileDprAsync(string dprPath, string architecture = "Win32")
    {
        _dprPath = dprPath;
        _architecture = string.IsNullOrWhiteSpace(architecture) ? "Win32" : architecture;

        // dcc expects the dpr file path as argument
        var command = new StringBuilder();
        command.Append($"\"{dprPath}\"");

        // Use the directory of the dpr as working directory
        var workingDirectory = Path.GetDirectoryName(dprPath) ?? Directory.GetCurrentDirectory();

        return await ExecuteAsync(command.ToString(), workingDirectory);
    }

    protected override string? GetExecutablePath(ToolConfig toolConfig)
    {
        // Snapshot settings at execution time
        var settings = SettingsMonitor.CurrentValue;
        
        // Determine exe name based on architecture
        var exeName = _architecture.Equals("Win64", System.StringComparison.OrdinalIgnoreCase) || 
                      _architecture.Equals("x64", System.StringComparison.OrdinalIgnoreCase)
            ? "dcc64.exe"
            : "dcc32.exe";

        // Try to extract version from DPR's project file if it exists
        string? projectVersion = null;
        if (!string.IsNullOrWhiteSpace(_dprPath))
        {
            var dprojPath = Path.ChangeExtension(_dprPath, ".dproj");
            if (File.Exists(dprojPath))
            {
                projectVersion = ExtractProjectVersion(dprojPath);
            }
        }

        // Use smart path resolver
        var configuredPaths = settings.Tools.MSBuildDelphi?.DelphiInstallPaths;
        var installPath = DelphiPathResolver.ResolveInstallPath(projectVersion, configuredPaths);

        if (installPath != null)
        {
            // Try standard bin location
            var candidate = Path.Combine(installPath, "bin", exeName);
            if (File.Exists(candidate))
            {
                Logger.LogInformation("Found {ExeName} at: {Path}", exeName, candidate);
                return candidate;
            }

            // Try alternate location (some versions have different structure)
            var candidate2 = Path.Combine(installPath, "bin", "win32", exeName);
            if (File.Exists(candidate2))
            {
                Logger.LogInformation("Found {ExeName} at: {Path}", exeName, candidate2);
                return candidate2;
            }
        }

        // Fallback: check if configured tool path has the exe
        if (!string.IsNullOrWhiteSpace(toolConfig.Path))
        {
            var candidate = Path.Combine(toolConfig.Path, exeName);
            if (File.Exists(candidate))
            {
                Logger.LogInformation("Found {ExeName} at configured path: {Path}", exeName, candidate);
                return candidate;
            }
        }

        // Last resort: try PATH
        Logger.LogInformation("Attempting to use {ExeName} from system PATH", exeName);
        return exeName;
    }

    private string? ExtractProjectVersion(string dprojPath)
    {
        try
        {
            var doc = System.Xml.Linq.XDocument.Load(dprojPath);
            var projectElement = doc.Root;
            if (projectElement == null) return null;

            var ns = projectElement.Name.NamespaceName;
            var propertyGroup = projectElement.Elements(System.Xml.Linq.XName.Get("PropertyGroup", ns)).FirstOrDefault();
            if (propertyGroup == null) return null;

            var versionElement = propertyGroup.Element(System.Xml.Linq.XName.Get("ProjectVersion", ns));
            return versionElement?.Value;
        }
        catch
        {
            return null;
        }
    }

    protected override Task<string?> PreExecuteAsync(
        string command, 
        string? workingDirectory,
        CodingSettings settings,
        ToolConfig toolConfig)
    {
        if (string.IsNullOrWhiteSpace(_dprPath) || !File.Exists(_dprPath))
        {
            return Task.FromResult<string?>("DPR file not found or path not provided.");
        }

        return base.PreExecuteAsync(command, workingDirectory, settings, toolConfig);
    }
}
