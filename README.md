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

<!-- START: axi-plan status -->
## Plan Status

- **Specs:** 1
- **Phases:** 7
- **Epics:** 39
- **Progress:** 76.8% (922/1200 tasks)
- **Greenlit:** 25
- **Done:** 0
- **Needs Attention:** 0


### Specifications

| ID | Name | Status | Phases |
|:---|:-----|:-------|-------:|
| PRP | Blazor AG-UI Components - Product Requirements Prompt | Draft | 0 |


### Phases

| Phase | Name | Epics | Completion |
|------:|:-----|------:|-----------:|
| 0 | Foundation | 4 | 92% |
| 1 | MudBlazor MVP | 6 | 92% |
| 2 | Production Hardening | 7 | 93% |
| 3 | Advanced Features | 8 | 59% |
| 4 | Polish & Extensibility | 6 | 0% |
| 5 | Documentation & Launch | 5 | 0% |
| 6 | Post-Launch | 3 | 0% |


### Epics

| Greenlit | Phase | Epic | Title | Tasks | Completion |
|:--------:|------:|-----:|:------|------:|-----------:|
| ✅ | 0 | 1 | Repository Structure Setup | 105/105 | 100% |
| ✅ | 0 | 2 | Core Abstractions Design | 126/129 | 98% |
| ✅ | 0 | 3 | NuGet Package Configuration | 108/119 | 91% |
| ✅ | 0 | 4 | CI/CD Pipeline Setup | 112/138 | 81% |
| ✅ | 1 | 1 | MudAgentChat Component | 113/127 | 89% |
| ✅ | 1 | 2 | MudMessageList Component | 42/43 | 98% |
| ✅ | 1 | 3 | MudMessageInput Component | 15/16 | 94% |
| ✅ | 1 | 4 | Markdown and Syntax Highlighting | 27/27 | 100% |
| ✅ | 1 | 5 | DirectAgentClientFactory Implementation | 36/45 | 80% |
| ✅ | 1 | 6 | BlazorServer.Basic Sample | 36/36 | 100% |
| ✅ | 2 | 1 | State Persistence Infrastructure | 11/15 | 73% |
| ✅ | 2 | 2 | WASM Package Implementation | 14/18 | 78% |
| ✅ | 2 | 3 | Offline Support and Connection Monitoring | 19/22 | 86% |
| ✅ | 2 | 4 | Tool Approval UI | 23/23 | 100% |
| ✅ | 2 | 5 | Error Handling and Retry Logic | 23/23 | 100% |
| ✅ | 2 | 6 | Token and Cost Tracking | 20/20 | 100% |
| ✅ | 2 | 7 | BlazorWasm.Basic Sample | 25/25 | 100% |
| ✅ | 3 | 1 | Message Operations | 14/14 | 100% |
| ✅ | 3 | 2 | Conversation History UI | 15/15 | 100% |
| ✅ | 3 | 3 | Conversation Management | 0/15 | 0% |
| ✅ | 3 | 4 | Advanced MudBlazor Components | 0/16 | 0% |
| ✅ | 3 | 5 | Mobile Responsive Layout | 0/14 | 0% |
| ✅ | 3 | 6 | Keyboard Shortcuts | 14/14 | 100% |
| ✅ | 3 | 7 | BlazorServer.Full Sample | 11/13 | 85% |
| ✅ | 3 | 8 | BlazorWasm.Full Sample | 13/13 | 100% |
|  | 4 | 1 | Testing Package | 0/12 | 0% |
|  | 4 | 2 | Performance Virtualization | 0/11 | 0% |
|  | 4 | 3 | Component Extensibility | 0/11 | 0% |
|  | 4 | 4 | Accessibility Improvements | 0/13 | 0% |
|  | 4 | 5 | Hybrid Sample | 0/11 | 0% |
|  | 4 | 6 | API Documentation | 0/11 | 0% |
|  | 5 | 1 | Getting Started Guide | 0/10 | 0% |
|  | 5 | 2 | Platform Guides | 0/11 | 0% |
|  | 5 | 3 | Video Tutorials | 0/11 | 0% |
|  | 5 | 4 | NuGet Publication | 0/12 | 0% |
|  | 5 | 5 | Community Launch | 0/12 | 0% |
|  | 6 | 1 | Community Support | 0/10 | 0% |
|  | 6 | 2 | Bug Fixes and Maintenance | 0/10 | 0% |
|  | 6 | 3 | Success Metrics Tracking | 0/10 | 0% |
<!-- END: axi-plan status -->

## Contributing

Contributions are welcome! Please read our [Contributing Guidelines](CONTRIBUTING.md) before submitting a pull request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Related Projects

- [AG-UI Protocol](https://github.com/ag-ui-org/ag-ui) - The AG-UI protocol specification
