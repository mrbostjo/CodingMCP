using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Xml.Linq;
using System.IO;
using System.Linq;
using CodingMCP.Configuration;

namespace CodingMCP.Executors;

public class MSBuildDelphiExecutor : MSBuildExecutor
{
    private string? _delphiProjectPath;
    private string? _bdsPath;
    private string? _projectVersion;

    public MSBuildDelphiExecutor(ILogger<MSBuildDelphiExecutor> logger, IOptionsMonitor<CodingSettings> settingsMonitor)
        : base(logger, settingsMonitor)
    {
    }

    protected override string GetToolName() => "MSBuild (Delphi)";

    /// <summary>
    /// Build a Delphi project and try to resolve the appropriate BDS path from configuration based on the project's ProjectVersion.
    /// </summary>
    public new async Task<ExecutionResult> BuildProjectAsync(string projectPath, string? buildOptions = null)
    {
        _delphiProjectPath = projectPath;
        _projectVersion = GetProjectVersion(projectPath);

        _bdsPath = ResolveBdsPath(_projectVersion);
        if (!string.IsNullOrWhiteSpace(_bdsPath))
        {
            Logger.LogInformation("Resolved BDS path for project {ProjectPath}: {BdsPath}", projectPath, _bdsPath);
        }
        else
        {
            Logger.LogWarning("No Delphi installation found for project {ProjectPath} (version: {Version})", projectPath, _projectVersion ?? "unknown");
        }

        return await base.BuildProjectAsync(projectPath, buildOptions);
    }

    protected override void ModifyProcessStartInfo(ProcessStartInfo startInfo)
    {
        base.ModifyProcessStartInfo(startInfo);

        // If we resolved a BDS path earlier, set it; otherwise set empty string
        startInfo.EnvironmentVariables["BDS"] = _bdsPath ?? string.Empty;
    }

    private string? ResolveBdsPath(string? projectVersion)
    {
        try
        {
            // Snapshot settings at resolution time
            var settings = SettingsMonitor.CurrentValue;
            var configuredPaths = settings.Tools.MSBuildDelphi?.DelphiInstallPaths;

            // Use smart path resolver with configured paths as priority
            var resolvedPath = DelphiPathResolver.ResolveInstallPath(projectVersion, configuredPaths);
            
            if (resolvedPath != null)
            {
                Logger.LogInformation("Resolved Delphi installation: {Path} for project version {Version}", 
                    resolvedPath, projectVersion ?? "auto-detected");
            }
            else if (configuredPaths != null && configuredPaths.Count > 0)
            {
                Logger.LogWarning("No matching Delphi installation found in configured paths or standard locations");
            }
            else
            {
                Logger.LogWarning("No Delphi installation found in standard locations. Consider adding delphiInstallPaths to config.json");
            }

            return resolvedPath;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error resolving BDS path from configuration");
            return null;
        }
    }

    /// <summary>
    /// Extract the ProjectVersion from a Delphi .dproj file
    /// </summary>
    public string? GetProjectVersion(string projectPath)
    {
        try
        {
            if (!File.Exists(projectPath))
            {
                Logger.LogWarning("Project file not found: {ProjectPath}", projectPath);
                return null;
            }

            var doc = XDocument.Load(projectPath);
            var projectElement = doc.Root;

            if (projectElement == null)
            {
                Logger.LogWarning("Invalid project file format: {ProjectPath}", projectPath);
                return null;
            }

            var ns = projectElement.Name.NamespaceName;
            var propertyGroup = projectElement.Elements(XName.Get("PropertyGroup", ns)).FirstOrDefault();

            if (propertyGroup == null)
            {
                Logger.LogWarning("PropertyGroup not found in project file: {ProjectPath}", projectPath);
                return null;
            }

            var versionElement = propertyGroup.Element(XName.Get("ProjectVersion", ns));
            var projectVersion = versionElement?.Value;

            if (projectVersion != null)
            {
                Logger.LogInformation("Extracted ProjectVersion from {ProjectPath}: {ProjectVersion}", projectPath, projectVersion);
            }
            else
            {
                Logger.LogWarning("ProjectVersion element not found in project file: {ProjectPath}", projectPath);
            }

            return projectVersion;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error extracting ProjectVersion from {ProjectPath}", projectPath);
            return null;
        }
    }
}
