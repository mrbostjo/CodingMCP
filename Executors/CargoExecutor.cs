using Microsoft.Extensions.Logging;
using CodingMCP.Configuration;

namespace CodingMCP.Executors;

public class CargoExecutor : CommandExecutor
{
    public CargoExecutor(ILogger<CargoExecutor> logger, CodingSettings settings)
        : base(logger, settings, settings.Tools.Rust)
    {
    }

    protected override string GetToolName() => "cargo";
}
