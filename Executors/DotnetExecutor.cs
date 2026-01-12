using Microsoft.Extensions.Logging;
using CodingMCP.Configuration;

namespace CodingMCP.Executors;

public class DotnetExecutor : CommandExecutor
{
    public DotnetExecutor(ILogger<DotnetExecutor> logger, CodingSettings settings)
        : base(logger, settings, settings.Tools.Dotnet)
    {
    }

    protected override string GetToolName() => "dotnet";
}
