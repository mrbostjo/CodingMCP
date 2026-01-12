using Microsoft.Extensions.Logging;
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

    public MSBuildDelphiExecutor(ILogger<MSBuildDelphiExecutor> logger, CodingSettings settings)
        : base(logger, settings)
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
            Logger.LogInformation("No matching BDS path found in configuration for project {ProjectPath}", projectPath);
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
            var candidates = Settings.Tools.MSBuildDelphi?.DelphiInstallPaths;
            if (candidates == null || candidates.Count == 0)
            {
                Logger.LogDebug("No Delphi install paths configured under Tools.MSBuildDelphi.DelphiInstallPaths");
                return null;
            }

            // Prefer exact match of version string in path
            if (!string.IsNullOrWhiteSpace(projectVersion))
            {
                foreach (var candidate in candidates)
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(candidate))
                            continue;

                        if (candidate.Contains(projectVersion, System.StringComparison.OrdinalIgnoreCase) && Directory.Exists(candidate))
                        {
                            return candidate;
                        }

                        // Also match by major version (e.g., '20' in '20.1')
                        var major = projectVersion.Split('.').FirstOrDefault();
                        if (!string.IsNullOrWhiteSpace(major) && candidate.Contains(major) && Directory.Exists(candidate))
                        {
                            return candidate;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogDebug(ex, "Error checking candidate Delphi install path: {Candidate}", candidate);
                    }
                }
            }

            // Fallback to first existing path
            foreach (var candidate in candidates)
            {
                if (string.IsNullOrWhiteSpace(candidate))
                    continue;

                if (Directory.Exists(candidate))
                    return candidate;
            }

            return null;
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
