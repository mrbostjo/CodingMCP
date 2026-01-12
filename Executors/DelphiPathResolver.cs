using System.Text.RegularExpressions;

namespace CodingMCP.Executors;

/// <summary>
/// Helper class to resolve Delphi installation paths automatically
/// </summary>
public static class DelphiPathResolver
{
    private static readonly string[] StandardBasePaths = new[]
    {
        @"C:\Program Files (x86)\Embarcadero\Studio",
        @"C:\Program Files\Embarcadero\Studio"
    };

    /// <summary>
    /// Resolve the best matching Delphi installation path for a given project version
    /// </summary>
    /// <param name="projectVersion">Version from .dproj (e.g., "22.0")</param>
    /// <param name="configuredPaths">Optional configured paths to check first</param>
    /// <returns>Best matching installation path or null if none found</returns>
    public static string? ResolveInstallPath(string? projectVersion, IEnumerable<string>? configuredPaths = null)
    {
        // First, try configured paths if provided
        if (configuredPaths != null)
        {
            var configuredMatch = FindBestMatch(projectVersion, configuredPaths);
            if (configuredMatch != null)
                return configuredMatch;
        }

        // Scan standard installation locations
        var foundInstallations = ScanStandardLocations();
        if (foundInstallations.Count == 0)
            return null;

        // If only one installation, use it regardless of version
        if (foundInstallations.Count == 1)
            return foundInstallations[0].Path;

        // Apply version matching strategy
        return FindBestMatch(projectVersion, foundInstallations.Select(x => x.Path));
    }

    /// <summary>
    /// Scan standard Delphi installation locations
    /// </summary>
    private static List<DelphiInstallation> ScanStandardLocations()
    {
        var installations = new List<DelphiInstallation>();

        foreach (var basePath in StandardBasePaths)
        {
            if (!Directory.Exists(basePath))
                continue;

            try
            {
                var subdirs = Directory.GetDirectories(basePath);
                foreach (var dir in subdirs)
                {
                    var dirName = Path.GetFileName(dir);
                    var version = ParseVersion(dirName);
                    
                    if (version != null && IsValidDelphiInstall(dir))
                    {
                        installations.Add(new DelphiInstallation
                        {
                            Path = dir,
                            Version = version,
                            VersionString = dirName
                        });
                    }
                }
            }
            catch
            {
                // Ignore access errors
            }
        }

        return installations;
    }

    /// <summary>
    /// Find the best matching installation path using fallback strategy
    /// </summary>
    private static string? FindBestMatch(string? projectVersion, IEnumerable<string> paths)
    {
        var pathsList = paths.Where(p => !string.IsNullOrWhiteSpace(p) && Directory.Exists(p)).ToList();
        if (pathsList.Count == 0)
            return null;

        if (pathsList.Count == 1)
            return pathsList[0];

        // Parse target version
        var targetVersion = ParseVersion(projectVersion);
        if (targetVersion == null)
        {
            // No version to match, return first valid path
            return pathsList.FirstOrDefault();
        }

        var installations = pathsList
            .Select(p => new
            {
                Path = p,
                Version = ParseVersion(Path.GetFileName(p)),
                DirName = Path.GetFileName(p)
            })
            .Where(x => x.Version != null)
            .Select(x => new DelphiInstallation
            {
                Path = x.Path,
                Version = x.Version!,
                VersionString = x.DirName
            })
            .ToList();

        if (installations.Count == 0)
            return pathsList.FirstOrDefault();

        // 1. Try exact match
        var exactMatch = installations.FirstOrDefault(i => 
            i.Version.Major == targetVersion.Major && 
            i.Version.Minor == targetVersion.Minor);
        if (exactMatch != null)
            return exactMatch.Path;

        // 2. Try same major, highest minor
        var sameMajor = installations
            .Where(i => i.Version.Major == targetVersion.Major)
            .OrderByDescending(i => i.Version.Minor)
            .FirstOrDefault();
        if (sameMajor != null)
            return sameMajor.Path;

        // 3. Try next higher major version
        var nextMajor = installations
            .Where(i => i.Version.Major > targetVersion.Major)
            .OrderBy(i => i.Version.Major)
            .ThenByDescending(i => i.Version.Minor)
            .FirstOrDefault();
        if (nextMajor != null)
            return nextMajor.Path;

        // 4. Fallback to highest available version
        return installations
            .OrderByDescending(i => i.Version.Major)
            .ThenByDescending(i => i.Version.Minor)
            .FirstOrDefault()?.Path;
    }

    /// <summary>
    /// Parse version string (e.g., "22.0", "21.0") to Version object
    /// </summary>
    private static Version? ParseVersion(string? versionString)
    {
        if (string.IsNullOrWhiteSpace(versionString))
            return null;

        // Try standard parsing first (handles "22.0", "21.0", etc.)
        if (Version.TryParse(versionString, out var version))
            return version;

        // Try extracting version with regex (handles "Studio 22.0" or similar)
        var match = Regex.Match(versionString, @"(\d+)\.(\d+)");
        if (match.Success)
        {
            if (int.TryParse(match.Groups[1].Value, out var major) &&
                int.TryParse(match.Groups[2].Value, out var minor))
            {
                return new Version(major, minor);
            }
        }

        return null;
    }

    /// <summary>
    /// Check if a directory is a valid Delphi installation
    /// </summary>
    private static bool IsValidDelphiInstall(string path)
    {
        // Check for bin directory and key executables
        var binPath = Path.Combine(path, "bin");
        if (!Directory.Exists(binPath))
            return false;

        // Check for at least one of the key executables
        return File.Exists(Path.Combine(binPath, "dcc32.exe")) ||
               File.Exists(Path.Combine(binPath, "dcc64.exe")) ||
               File.Exists(Path.Combine(binPath, "bds.exe"));
    }

    private class DelphiInstallation
    {
        public string Path { get; set; } = string.Empty;
        public Version Version { get; set; } = new Version();
        public string VersionString { get; set; } = string.Empty;
    }
}
