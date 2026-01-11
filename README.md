# CodingMCP Server

A Model Context Protocol (MCP) server that provides tools for executing commands across multiple programming language toolchains: .NET, Rust, Python, and Delphi.

## Features

- **Code .NET Tool**: Execute .NET CLI commands for building, running, and managing .NET projects
- **Code Rust Tool**: Execute Rust cargo commands for building, running, and managing Rust projects
- **Code Python Tool**: Execute Python scripts and commands using the Python interpreter
- **Code MSBuild Tool**: Build projects using MSBuild - supports Delphi (.dproj), C/C++ (.vcxproj), C# (.csproj), and other MSBuild-compatible projects

## Architecture

This server follows the same architectural patterns as MyMCP:
- **Dependency Injection**: All tools are registered in the DI container
- **Configuration**: Settings loaded from `config.json`
- **Tool Organization**: Each tool is implemented in a separate file under the `Tools` directory
- **MCP Integration**: Uses `ModelContextProtocol` NuGet package for MCP server functionality

## Configuration

Edit `config.json` to configure the paths to your development tools:

```json
{
  "tools": {
    "dotnet": {
      "path": "C:\\Program Files\\dotnet",
      "executableName": "dotnet.exe"
    },
    "rust": {
      "path": "C:\\Users\\YourUser\\.cargo\\bin",
      "executableName": "cargo.exe"
    },
    "python": {
      "path": "C:\\Python312",
      "executableName": "python.exe"
    },
    "msbuild": {
      "path": "C:\\Program Files (x86)\\MSBuild\\Current\\Bin",
      "executableName": "msbuild.exe"
    }
  },
  "features": {
    "enableLogging": true,
    "defaultTimeout": 30,
    "maxOutputSize": 10485760
  }
}
```

## Building

```bash
dotnet build
```

## Running

```bash
dotnet run
```

## Using with Claude Desktop

To use this MCP server with Claude Desktop, you need to configure it in Claude's configuration file.

### Step 1: Build the Project

First, build the project to generate the executable:

```bash
cd C:\MyProjects\CodingMCP
dotnet build
```

### Step 2: Configure Claude Desktop

Edit the Claude Desktop configuration file:

**Windows:** `%APPDATA%\Claude\claude_desktop_config.json`

**macOS:** `~/Library/Application Support/Claude/claude_desktop_config.json`

**Linux:** `~/.config/Claude/claude_desktop_config.json`

### Step 3: Add CodingMCP Server

Add the following configuration to the `mcpServers` section:

```json
{
  "mcpServers": {
    "coding-mcp": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "C:\\MyProjects\\CodingMCP\\CodingMCP.csproj"
      ]
    }
  }
}
```

Or, if you prefer to use the compiled executable:

```json
{
  "mcpServers": {
    "coding-mcp": {
      "command": "C:\\MyProjects\\CodingMCP\\bin\\Debug\\net8.0\\CodingMCP.exe",
      "args": []
    }
  }
}
```

### Step 4: Restart Claude Desktop

Restart Claude Desktop to load the new MCP server configuration.

### Step 5: Verify Connection

In Claude Desktop, you should see the CodingMCP tools available. Try asking:
- "Can you build my .NET project?"
- "Run this Python script for me"
- "Compile my C++ project"

### Troubleshooting

- **Logs**: Check Claude Desktop logs for MCP connection issues
- **Paths**: Ensure all paths in `config.json` are correct for your system
- **Permissions**: Make sure the executable has proper permissions to run
- **config.json**: Verify that `config.json` exists in the output directory (it should be copied automatically during build)

## Tools

### code_dotnet
Execute .NET CLI commands.

**Parameters:**
- `command` (string): The dotnet CLI command (e.g., "build", "run", "test")
- `workingDirectory` (string, optional): Working directory for execution

**Example:**
```
command: "build --configuration Release"
workingDirectory: "C:\\MyProjects\\MyApp"
```

### code_rust
Execute Rust cargo commands.

**Parameters:**
- `command` (string): The cargo command (e.g., "build", "run", "test")
- `workingDirectory` (string, optional): Working directory for execution

**Example:**
```
command: "build --release"
workingDirectory: "C:\\MyProjects\\rust-app"
```

### code_python
Execute Python scripts or commands.

**Parameters:**
- `command` (string): The Python command (e.g., "script.py", "-m pip install package")
- `workingDirectory` (string, optional): Working directory for execution

**Example:**
```
command: "main.py"
workingDirectory: "C:\\MyProjects\\python-app"
```

### code_msbuild
Build projects using MSBuild.

**Parameters:**
- `projectPath` (string): Path to the project file (.dproj for Delphi, .vcxproj for C/C++, .csproj for C#, or .sln for solution)
- `buildOptions` (string, optional): Additional MSBuild options

**Examples:**

*Delphi project:*
```
projectPath: "C:\\MyProjects\\DelphiApp\\MyApp.dproj"
buildOptions: "/t:Rebuild /p:Configuration=Release /p:Platform=Win32"
```

*C++ project:*
```
projectPath: "C:\\MyProjects\\CppApp\\MyApp.vcxproj"
buildOptions: "/p:Configuration=Release /p:Platform=x64"
```

*C# project:*
```
projectPath: "C:\\MyProjects\\CSharpApp\\MyApp.csproj"
buildOptions: "/t:Rebuild"
```

## Project Structure

```
CodingMCP/
├── Configuration/
│   ├── ConfigurationLoader.cs    # Configuration loading logic
│   └── CodingSettings.cs          # Configuration data models
├── Tools/
│   ├── CodeDotnetTool.cs          # .NET CLI tool
│   ├── CodeRustTool.cs            # Rust cargo tool
│   ├── CodePythonTool.cs          # Python tool
│   └── CodeMSBuildTool.cs         # MSBuild tool (Delphi/C++/C#)
├── Program.cs                     # Main entry point with DI setup
├── CodingMCP.csproj              # Project file
└── config.json                    # Configuration file
```

## Dependencies

- .NET 8.0
- Microsoft.Extensions.Hosting
- ModelContextProtocol (0.5.0-preview.1)

## License

Same as parent project.
