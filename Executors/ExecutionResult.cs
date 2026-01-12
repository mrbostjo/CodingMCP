using System.Text;

namespace CodingMCP.Executors;

public class ExecutionResult
{
    public int ExitCode { get; set; }
    public string Output { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
    public bool Success => ExitCode == 0;
    public bool TimedOut { get; set; }
    public string? ErrorMessage { get; set; }

    public override string ToString()
    {
        var result = new StringBuilder();
        
        if (!string.IsNullOrWhiteSpace(ErrorMessage))
        {
            result.AppendLine($"Error: {ErrorMessage}");
            return result.ToString();
        }

        result.AppendLine($"Exit Code: {ExitCode}");
        
        if (TimedOut)
        {
            result.AppendLine("\n=== Execution Timed Out ===");
        }
        
        if (!string.IsNullOrWhiteSpace(Output))
        {
            result.AppendLine("\n=== Output ===");
            result.AppendLine(Output);
        }

        if (!string.IsNullOrWhiteSpace(Error))
        {
            result.AppendLine("\n=== Errors/Warnings ===");
            result.AppendLine(Error);
        }

        return result.ToString();
    }
}
