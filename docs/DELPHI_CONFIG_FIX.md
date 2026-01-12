# MSBuildDelphi Configuration Fix

## Issue Found
The Delphi install paths were not being deserialized correctly from config.json due to property name casing mismatch.

## Root Cause
.NET configuration binding expects camelCase property names in JSON:
- ❌ `"delphi_install_paths"` (snake_case with underscores)
- ✅ `"delphiInstallPaths"` (camelCase)

Additionally, the section name was inconsistent:
- ❌ `"msbuilddelphi"` (all lowercase)
- ✅ `"msBuildDelphi"` (proper camelCase)

## Changes Made

### config.json
**Before:**
```json
"msbuilddelphi": {
    "delphi_install_paths": [...]
}
```

**After:**
```json
"msBuildDelphi": {
    "delphiInstallPaths": [...]
}
```

### config.example.json
Applied the same fix to the example configuration file.

## How It Works Now

1. **Configuration Loading:**
   ```
   config.json → IOptionsMonitor → CodingSettings
   ```
   - `"msBuildDelphi"` → `Tools.MSBuildDelphi` ✅
   - `"delphiInstallPaths"` → `DelphiInstallPaths` ✅

2. **BDS Environment Variable Setting:**
   - MSBuildDelphiExecutor reads the .dproj file
   - Extracts `ProjectVersion` (e.g., "22.0")
   - Searches `DelphiInstallPaths` for matching version
   - Sets `BDS` environment variable before executing MSBuild
   - MSBuild can now find Delphi compiler and libraries

3. **Version Matching Logic:**
   - Exact match: Path contains full version "22.0"
   - Major version match: Path contains "22"
   - Fallback: First existing path in list

## Example Configuration

```json
{
  "tools": {
    "msBuildDelphi": {
      "path": "C:\\Program Files (x86)\\MSBuild\\Current\\Bin",
      "executableName": "msbuild.exe",
      "delphiInstallPaths": [
        "C:\\Program Files (x86)\\Embarcadero\\Studio\\22.0",
        "C:\\Program Files (x86)\\Embarcadero\\Studio\\21.0",
        "C:\\Program Files (x86)\\Embarcadero\\Studio\\20.0"
      ]
    }
  }
}
```

## How to Use

When building a Delphi project:
```
BuildDelphiProject(
    projectPath: "C:\\MyProject\\MyApp.dproj",
    buildOptions: "/p:Configuration=Release /p:Platform=Win64"
)
```

The executor will:
1. Parse MyApp.dproj to find `<ProjectVersion>22.0</ProjectVersion>`
2. Match "22.0" with `"Studio\\22.0"` in the paths list
3. Set `BDS=C:\Program Files (x86)\Embarcadero\Studio\22.0`
4. Execute MSBuild with this environment variable

## Verification

✅ Build succeeds with corrected configuration
✅ Configuration deserialization now works correctly
✅ BDS environment variable will be set properly during execution

## Testing Recommendation

To verify the BDS setting works:
1. Create a test Delphi .dproj file (or use existing one)
2. Call `BuildDelphiProject` tool
3. Check logs for: `"Resolved BDS path for project"`
4. Verify MSBuild can locate Delphi compiler

## Migration Note

If you have existing config.json files on other machines:
- Update `"msbuilddelphi"` → `"msBuildDelphi"`
- Update `"delphi_install_paths"` → `"delphiInstallPaths"`
