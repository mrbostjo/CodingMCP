using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using CodingMCP.Configuration;
using CodingMCP.Executors;

namespace CodingMCP.Tools;

[McpServerToolType]
public class CodeDccDelphiTool
{
    private readonly DccDelphiExecutor _executor;

    public CodeDccDelphiTool(ILogger<CodeDccDelphiTool> logger, ILoggerFactory loggerFactory, CodingSettings settings)
    {
        _executor = new DccDelphiExecutor(loggerFactory.CreateLogger<DccDelphiExecutor>(), settings);
    }

    [McpServerTool, Description("Compile Delphi .dpr project using dcc32/dcc64")]
    public async Task<string> CompileDelphiDpr(
        [Description("Target architecture (Win32 or Win64)")] string architecture = "Win32",
        [Description("Path to the .dpr file")] string dprPath = "")
    {
        var result = await _executor.CompileDprAsync(dprPath, architecture);
        return result.ToString();
    }
}
