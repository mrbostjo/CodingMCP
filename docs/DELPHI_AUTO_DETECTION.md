# Delphi Auto-Detection Feature

## Overview
The CodingMCP server now automatically detects Delphi installations on your system, eliminating the need for manual configuration in most cases.

## How It Works

### Smart Path Resolution
When building or compiling a Delphi project, the system:

1. **Extracts version** from the .dproj file (e.g., "22.0")
2. **Scans standard locations**:
   - `C:\Program Files (x86)\Embarcadero\Studio\`
   - `C:\Program Files\Embarcadero\Studio\`
3. **Applies intelligent fallback strategy**:
   - Exact match: Uses version 22.0 if found
   - Same major: Uses highest minor (22.5 preferred over 22.0)
   - Next major: Uses 23.0 if no 22.x found
   - Single install: Uses it regardless of version

### Fallback Strategy Example

**Scenario:** Building a Delphi 22.0 project

**Available installations:**
- Studio\21.0
- Studio\22.5
- Studio\23.0

**Resolution:**
1. No exact 22.0 match
2. ✅ Found 22.5 (same major, highest minor)
3. Uses Studio\22.5

**Scenario:** Only one installation

**Available installations:**
- Studio\23.0

**Resolution:**
- ✅ Uses Studio\23.0 (only available version)

## Configuration

### Zero Configuration (Recommended)
If you have standard Delphi installations, no configuration is needed! The system will auto-detect.

### Manual Override (Optional)
You can still specify paths in config.json to:
- Add non-standard installation locations
- Override auto-detection
- Prioritize specific versions

```json
{
  "tools": {
    "msBuildDelphi": {
      "path": "C:\\Program Files (x86)\\MSBuild\\Current\\Bin",
      "executableName": "msbuild.exe",
      "delphiInstallPaths": [
        "C:\\Custom\\Delphi\\22.0",
        "D:\\Embarcadero\\Studio\\21.0"
      ]
    }
  }
}
```

**Priority:** Configured paths are checked first, then auto-detection kicks in.

## Validation

The system verifies each candidate path by checking for:
- `bin\dcc32.exe` (32-bit compiler)
- `bin\dcc64.exe` (64-bit compiler)
- `bin\bds.exe` (Delphi IDE)

Only valid installations are considered.

## Affected Tools

### MSBuildDelphiExecutor
- Automatically sets `BDS` environment variable
- MSBuild can locate Delphi compiler and libraries

### DccDelphiExecutor  
- Finds dcc32.exe or dcc64.exe based on architecture
- Checks corresponding .dproj for version information
- Falls back to PATH if auto-detection fails

## Logging

The system logs its resolution process:

```
[INFO] Extracted ProjectVersion from MyApp.dproj: 22.0
[INFO] Resolved Delphi installation: C:\Program Files (x86)\Embarcadero\Studio\22.5 for project version 22.0
[INFO] Resolved BDS path for project MyApp.dproj: C:\Program Files (x86)\Embarcadero\Studio\22.5
```

Or warnings if nothing is found:

```
[WARN] No Delphi installation found in standard locations. Consider adding delphiInstallPaths to config.json
```

## Benefits

✅ **Zero configuration** for standard installations
✅ **Intelligent version matching** with smart fallbacks
✅ **Multiple Delphi versions** handled automatically
✅ **Manual override** still available when needed
✅ **Validation** ensures only working installations are used

## Migration from Old Configuration

**Old config.json** (snake_case):
```json
"msbuilddelphi": {
  "delphi_install_paths": [...]
}
```

**New config.json** (camelCase):
```json
"msBuildDelphi": {
  "delphiInstallPaths": [...]
}
```

**But now:** You can often remove `delphiInstallPaths` entirely if using standard installations!

## Technical Details

### DelphiPathResolver Class
Location: `Executors/DelphiPathResolver.cs`

**Key method:**
```csharp
string? ResolveInstallPath(string? projectVersion, IEnumerable<string>? configuredPaths = null)
```

**Process:**
1. Check configured paths (if provided)
2. Scan standard locations
3. Parse versions from directory names
4. Apply fallback strategy
5. Validate installation
6. Return best match

### Version Parsing
Supports:
- Simple versions: "22.0", "21.0"
- Complex strings: Extracts "22.0" from "Studio 22.0"
- Regex pattern: `(\d+)\.(\d+)`

## Troubleshooting

### Issue: "No Delphi installation found"
**Solutions:**
1. Verify Delphi is installed in standard locations
2. Check directory names contain version numbers (e.g., "22.0")
3. Add custom path to config.json `delphiInstallPaths`

### Issue: "Wrong version used"
**Solutions:**
1. Check .dproj file has correct `<ProjectVersion>`
2. Add explicit path to config.json to override
3. Check logs to see what was detected

### Issue: "DCC32.exe not found"
**Solutions:**
1. Verify installation has `bin\dcc32.exe`
2. Check validation passed (logs should show)
3. Manually add to PATH as last resort

## Example Usage

**Simple case** (auto-detection):
```csharp
BuildDelphiProject(
    projectPath: "C:\\MyProject\\MyApp.dproj",
    buildOptions: "/p:Configuration=Release"
)
```

Result: Automatically finds Delphi 22.5, sets BDS, builds successfully.

**Manual override**:
```json
{
  "tools": {
    "msBuildDelphi": {
      "delphiInstallPaths": [
        "D:\\MyCustomDelphi\\22.0"
      ]
    }
  }
}
```

Result: Uses custom path first, falls back to auto-detection if needed.
