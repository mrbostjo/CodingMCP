using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CodingMCP.Configuration;

namespace CodingMCP.Executors;

public class CargoExecutor : CommandExecutor
{
    public CargoExecutor(ILogger<CargoExecutor> logger, IOptionsMonitor<CodingSettings> settingsMonitor)
        : base(logger, settingsMonitor, () => settingsMonitor.CurrentValue.Tools.Rust)
    {
    }

    protected override string GetToolName() => "cargo";
}
