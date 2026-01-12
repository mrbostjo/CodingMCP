using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO;
using System.Text;
using CodingMCP.Configuration;

namespace CodingMCP.Executors;

public class DccDelphiExecutor : CommandExecutor
{
    private string? _dprPath;
    private string _architecture = "Win32";

    public DccDelphiExecutor(ILogger<DccDelphiExecutor> logger, CodingSettings settings)
        : base(logger, settings, settings.Tools.MSBuildDelphi)
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

    protected override string? GetExecutablePath()
    {
        // Determine exe name based on architecture
        var exeName = _architecture.Equals("Win64", System.StringComparison.OrdinalIgnoreCase) || _architecture.Equals("x64", System.StringComparison.OrdinalIgnoreCase)
            ? "dcc64.exe"
            : "dcc32.exe";

        // 1) Check configured Delphi install paths for bin\{exeName}
        var delphiPaths = Settings.Tools.MSBuildDelphi?.DelphiInstallPaths;
        if (delphiPaths != null)
        {
            foreach (var p in delphiPaths)
            {
                if (string.IsNullOrWhiteSpace(p))
                    continue;

                try
                {
                    var candidate = Path.Combine(p, "bin", exeName);
                    if (File.Exists(candidate))
                        return candidate;

                    // sometimes the bin folder is under 'bin\x86' or similar; try searching common variants
                    var candidate2 = Path.Combine(p, "bin", "win32", exeName);
                    if (File.Exists(candidate2))
                        return candidate2;
                }
                catch { /* ignore malformed paths */ }
            }
        }

        // 2) Check configured tool path (ToolConfig.Path) like other tools
        if (!string.IsNullOrWhiteSpace(ToolConfig.Path))
        {
            var candidate = Path.Combine(ToolConfig.Path, exeName);
            if (File.Exists(candidate))
                return candidate;
        }

        // 3) Fallback to exe name (require it to be on PATH)
        return exeName;
    }

    protected override Task<string?> PreExecuteAsync(string command, string? workingDirectory)
    {
        if (string.IsNullOrWhiteSpace(_dprPath) || !File.Exists(_dprPath))
        {
            return Task.FromResult<string?>("DPR file not found or path not provided.");
        }

        return base.PreExecuteAsync(command, workingDirectory);
    }
}
