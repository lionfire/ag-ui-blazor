---
greenlit: true
implementationDone: true
completedDate: 2025-12-11
---

# Epic 00-001: Repository Structure Setup

**Phase**: 00 - Foundation
**Status**: Complete
**Estimated Effort**: 2-3 days

## Overview

Create a well-organized repository structure that supports multiple packages, samples, tests, and documentation. Establish directory conventions and initial project files.

**Link to Phase**: [Phase 00: Foundation](../../40-phases/00-foundation.md)

## Status Overview

- [x] Planning complete
- [x] Directory structure created
- [x] Solution file configured
- [x] All projects created
- [x] Initial builds successful
- [x] Documentation complete

## Implementation Tasks

### Directory Structure
- [x] Create root directory structure
  - [x] Create `src/` directory for source code
  - [x] Create `samples/` directory for sample applications
  - [x] Create `tests/` directory for unit and integration tests
  - [x] Create `docs/` directory for documentation
  - [x] Create `.github/` directory for GitHub-specific files
  - [x] Create `artifacts/` directory for build outputs (add to .gitignore)

### Source Projects
- [x] Create `LionFire.AgUi.Blazor` base project
  - [x] Run `dotnet new classlib -n LionFire.AgUi.Blazor -o src/LionFire.AgUi.Blazor`
  - [x] Add multi-targeting to net8.0 and net9.0 in .csproj
  - [x] Add README.md placeholder
  - [x] Create `Abstractions/` folder for interfaces
  - [x] Create `Models/` folder for shared models

- [x] Create `LionFire.AgUi.Blazor.MudBlazor` project
  - [x] Run `dotnet new razorclasslib -n LionFire.AgUi.Blazor.MudBlazor -o src/LionFire.AgUi.Blazor.MudBlazor`
  - [x] Add project reference to base project
  - [x] Create `Components/` folder structure
  - [x] Add README.md placeholder

- [x] Create `LionFire.AgUi.Blazor.Server` project
  - [x] Run `dotnet new classlib -n LionFire.AgUi.Blazor.Server -o src/LionFire.AgUi.Blazor.Server`
  - [x] Add project reference to base project
  - [x] Create `Services/` and `Extensions/` folders
  - [x] Add README.md placeholder

- [x] Create `LionFire.AgUi.Blazor.Wasm` project
  - [x] Run `dotnet new classlib -n LionFire.AgUi.Blazor.Wasm -o src/LionFire.AgUi.Blazor.Wasm`
  - [x] Add project reference to base project
  - [x] Create `Services/` and `Extensions/` folders
  - [x] Add README.md placeholder

- [x] Create `LionFire.AgUi.Blazor.Testing` project
  - [x] Run `dotnet new classlib -n LionFire.AgUi.Blazor.Testing -o src/LionFire.AgUi.Blazor.Testing`
  - [x] Add project reference to base project
  - [x] Add README.md placeholder

### Test Projects
- [x] Create `LionFire.AgUi.Blazor.Tests` project
  - [x] Run `dotnet new xunit -n LionFire.AgUi.Blazor.Tests -o tests/LionFire.AgUi.Blazor.Tests`
  - [x] Add project reference to base project
  - [x] Add xUnit, FluentAssertions, Moq packages

- [x] Create `LionFire.AgUi.Blazor.MudBlazor.Tests` project
  - [x] Run `dotnet new xunit -n LionFire.AgUi.Blazor.MudBlazor.Tests -o tests/LionFire.AgUi.Blazor.MudBlazor.Tests`
  - [x] Add project reference to MudBlazor project
  - [x] Add bUnit package for Blazor component testing

- [x] Create `LionFire.AgUi.Blazor.Server.Tests` project
  - [x] Run `dotnet new xunit -n LionFire.AgUi.Blazor.Server.Tests -o tests/LionFire.AgUi.Blazor.Server.Tests`
  - [x] Add project reference to Server project

- [x] Create `LionFire.AgUi.Blazor.Wasm.Tests` project
  - [x] Run `dotnet new xunit -n LionFire.AgUi.Blazor.Wasm.Tests -o tests/LionFire.AgUi.Blazor.Wasm.Tests`
  - [x] Add project reference to Wasm project

### Solution File
- [x] Create solution file
  - [x] Run `dotnet new sln -n LionFire.AgUi.Blazor`
  - [x] Add all source projects to solution
  - [x] Add all test projects to solution
  - [x] Create solution folders (src, tests, samples) for organization

### Git Configuration
- [x] Create `.gitignore` file
  - [x] Add standard .NET ignores (bin/, obj/, *.user)
  - [x] Add IDE ignores (.vs/, .vscode/, .idea/)
  - [x] Add artifacts/ and build outputs
  - [x] Add OS-specific files (.DS_Store, Thumbs.db)

- [x] Create `.gitattributes` file
  - [x] Set line ending normalization
  - [x] Mark binary files appropriately

### Documentation Files
- [x] Create root README.md
  - [x] Add project title and description
  - [x] Add build instructions
  - [x] Add quick links to samples and docs
  - [x] Add license badge

- [x] Create LICENSE file
  - [x] Add MIT license text
  - [x] Set copyright to LionFire

- [x] Create CONTRIBUTING.md
  - [x] Add contribution guidelines
  - [x] Add code of conduct reference
  - [x] Add PR process

- [x] Create CHANGELOG.md
  - [x] Add initial entry for v0.1.0

### EditorConfig and Code Style
- [x] Create `.editorconfig` file
  - [x] Set C# coding style rules
  - [x] Set indentation (4 spaces)
  - [x] Set line endings (LF)
  - [x] Configure naming conventions

- [x] Create `Directory.Build.props`
  - [x] Set common properties (LangVersion, Nullable, TreatWarningsAsErrors)
  - [x] Set default package properties
  - [x] Configure code analysis

### Verification
- [x] Build entire solution
  - [x] Run `dotnet build` from root
  - [x] Verify all projects build without warnings
  - [x] Check that multi-targeting works (net8.0, net9.0)

- [x] Run all tests
  - [x] Run `dotnet test` from root
  - [x] Verify test projects execute (even if empty)

## Dependencies & Blockers

**Upstream Dependencies**: None (first epic)

**Blocks**:
- Epic 00-002 (needs project structure)
- Epic 00-003 (needs projects created)
- Epic 00-004 (needs solution file)

## Acceptance Criteria

- [x] Directory structure matches planned layout
- [x] All 5 source projects exist and build successfully
- [x] All 4 test projects exist and can run tests
- [x] Solution file includes all projects organized into folders
- [x] `dotnet build` succeeds from repository root
- [x] `dotnet test` runs from repository root
- [x] .gitignore prevents committing build artifacts
- [x] README.md explains project and how to build
- [x] All projects target net8.0 and net9.0
- [x] Directory.Build.props sets consistent properties

## Notes

- Keep initial projects minimal (empty classes are fine)
- Focus on structure, not implementation
- Ensure project references are correct (base <- MudBlazor, Server, Wasm, Testing)
- Use consistent naming (LionFire.AgUi.Blazor.*)
- Follow .NET project conventions
