# LionFire.AgUi.Blazor

[![CI](https://github.com/lionfire/ag-ui-blazor/actions/workflows/ci.yml/badge.svg)](https://github.com/lionfire/ag-ui-blazor/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0-blue.svg)](https://dotnet.microsoft.com/)
[![NuGet](https://img.shields.io/nuget/v/LionFire.AgUi.Blazor.svg)](https://www.nuget.org/packages/LionFire.AgUi.Blazor)

A Blazor component library for integrating with the AG-UI (Agent User Interface) protocol, providing real-time streaming UI components for AI agent interactions.

## Overview

LionFire.AgUi.Blazor provides Blazor components and services for displaying and interacting with AG-UI protocol streams. It supports both Blazor Server and Blazor WebAssembly hosting models.

## Status

Early WIP.

## Packages

| Package | Description |
|---------|-------------|
| `LionFire.AgUi.Blazor` | Core abstractions and models |
| `LionFire.AgUi.Blazor.MudBlazor` | MudBlazor-based UI components |
| `LionFire.AgUi.Blazor.Server` | Server-side services and extensions |
| `LionFire.AgUi.Blazor.Wasm` | WebAssembly services and extensions |
| `LionFire.AgUi.Blazor.Testing` | Test utilities and helpers |

## Getting Started

### Prerequisites

- .NET 8.0 or .NET 9.0 SDK
- A Blazor Server or Blazor WebAssembly project

### Installation

```bash
# For Blazor Server applications
dotnet add package LionFire.AgUi.Blazor.Server
dotnet add package LionFire.AgUi.Blazor.MudBlazor

# For Blazor WebAssembly applications
dotnet add package LionFire.AgUi.Blazor.Wasm
dotnet add package LionFire.AgUi.Blazor.MudBlazor
```

### Basic Usage

Documentation coming soon.

## Building from Source

### Prerequisites

- .NET 8.0 SDK or later
- Git

### Build

```bash
# Clone the repository
git clone https://github.com/lionfire/ag-ui-blazor.git
cd ag-ui-blazor

# Restore dependencies and build
dotnet restore
dotnet build

# Run tests
dotnet test
```

### Using Build Scripts

**Linux/macOS:**
```bash
# Full build (restore, build, test, pack)
./build.sh

# Individual commands
./build.sh restore
./build.sh build
./build.sh test
./build.sh pack
./build.sh clean
```

**Windows (PowerShell):**
```powershell
# Full build (restore, build, test, pack)
.\build.ps1

# Individual commands
.\build.ps1 restore
.\build.ps1 build
.\build.ps1 test
.\build.ps1 pack
.\build.ps1 clean
```

## Project Structure

```
ag-ui-blazor/
  src/
    LionFire.AgUi.Blazor/           # Core abstractions and models
    LionFire.AgUi.Blazor.MudBlazor/ # MudBlazor UI components
    LionFire.AgUi.Blazor.Server/    # Server-side implementation
    LionFire.AgUi.Blazor.Wasm/      # WebAssembly implementation
    LionFire.AgUi.Blazor.Testing/   # Test utilities
  tests/
    LionFire.AgUi.Blazor.Tests/
    LionFire.AgUi.Blazor.MudBlazor.Tests/
    LionFire.AgUi.Blazor.Server.Tests/
    LionFire.AgUi.Blazor.Wasm.Tests/
  samples/                           # Sample applications
  docs/                              # Documentation
```

## Contributing

Contributions are welcome! Please read our [Contributing Guidelines](CONTRIBUTING.md) before submitting a pull request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Related Projects

- [AG-UI Protocol](https://github.com/ag-ui-org/ag-ui) - The AG-UI protocol specification
