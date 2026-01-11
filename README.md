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
