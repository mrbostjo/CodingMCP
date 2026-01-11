namespace CodingMCP.Configuration;

public class CodingSettings
{
    public ToolsSettings Tools { get; set; } = new();
    public FeaturesSettings Features { get; set; } = new();
}

public class ToolsSettings
{
    public ToolConfig Dotnet { get; set; } = new();
    public ToolConfig Rust { get; set; } = new();
    public ToolConfig Python { get; set; } = new();
    public ToolConfig MSBuild { get; set; } = new();
}

public class ToolConfig
{
    public string Path { get; set; } = string.Empty;
    public string ExecutableName { get; set; } = string.Empty;
    
    public string FullPath => System.IO.Path.Combine(Path, ExecutableName);
}

public class FeaturesSettings
{
    public bool EnableLogging { get; set; } = true;
    public int DefaultTimeout { get; set; } = 30;
    public long MaxOutputSize { get; set; } = 10485760; // 10 MB
}
