using Microsoft.Extensions.Logging;
using CodingMCP.Configuration;

namespace CodingMCP.Executors;

public class PythonExecutor : CommandExecutor
{
    public PythonExecutor(ILogger<PythonExecutor> logger, CodingSettings settings)
        : base(logger, settings, settings.Tools.Python)
    {
    }

    protected override string GetToolName() => "python";
}
