using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CodingMCP.Configuration;

namespace CodingMCP.Executors;

public class PythonExecutor : CommandExecutor
{
    public PythonExecutor(ILogger<PythonExecutor> logger, IOptionsMonitor<CodingSettings> settingsMonitor)
        : base(logger, settingsMonitor, () => settingsMonitor.CurrentValue.Tools.Python)
    {
    }

    protected override string GetToolName() => "python";
}
