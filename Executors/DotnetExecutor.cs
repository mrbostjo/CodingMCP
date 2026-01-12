using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CodingMCP.Configuration;

namespace CodingMCP.Executors;

public class DotnetExecutor : CommandExecutor
{
    public DotnetExecutor(ILogger<DotnetExecutor> logger, IOptionsMonitor<CodingSettings> settingsMonitor)
        : base(logger, settingsMonitor, () => settingsMonitor.CurrentValue.Tools.Dotnet)
    {
    }

    protected override string GetToolName() => "dotnet";
}
