---
greenlit: true
implementationDone: true
completedDate: 2025-12-11
---

# Epic 00-003: NuGet Package Configuration

**Phase**: 00 - Foundation
**Status**: Complete
**Estimated Effort**: 1-2 days

## Overview

Configure all projects for NuGet packaging with proper metadata, versioning, dependencies, and packaging options. Ensure packages follow NuGet best practices and conventions.

**Link to Phase**: [Phase 00: Foundation](../../40-phases/00-foundation.md)

## Status Overview

- [x] Planning complete
- [x] Package metadata configured
- [x] Dependencies configured
- [x] Version strategy defined
- [x] Package icons/readme prepared
- [x] Local pack tested

## Implementation Tasks

### Shared Package Properties
- [x] Update `Directory.Build.props` with package defaults
  - [x] Add `<Company>LionFire</Company>`
  - [x] Add `<Authors>LionFire</Authors>`
  - [x] Add `<Copyright>Copyright (c) LionFire 2025</Copyright>`
  - [x] Add `<PackageLicenseExpression>MIT</PackageLicenseExpression>`
  - [x] Add `<PackageProjectUrl>https://github.com/LionFire/LionFire.AgUi.Blazor</PackageProjectUrl>`
  - [x] Add `<RepositoryUrl>https://github.com/LionFire/LionFire.AgUi.Blazor.git</RepositoryUrl>`
  - [x] Add `<RepositoryType>git</RepositoryType>`
  - [x] Add `<PackageTags>blazor;ai;agents;agui;mudblazor;chat;llm</PackageTags>`
  - [x] Add `<GeneratePackageOnBuild>false</GeneratePackageOnBuild>` (explicit pack only)
  - [x] Add `<IncludeSymbols>true</IncludeSymbols>`
  - [x] Add `<SymbolPackageFormat>snupkg</SymbolPackageFormat>`
  - [x] Add `<PublishRepositoryUrl>true</PublishRepositoryUrl>`
  - [x] Add `<EmbedUntrackedSources>true</EmbedUntrackedSources>`

### Versioning Strategy
- [x] Define versioning approach
  - [x] Decide on SemVer 2.0 compliance
  - [x] Set initial version to 0.1.0-preview.1
  - [x] Document version increment rules
  - [x] Add version to Directory.Build.props: `<Version>0.1.0-preview.1</Version>`

- [ ] Configure MinVer for automated versioning (optional)
  - [ ] Add MinVer package to Directory.Build.props
  - [ ] Configure tag prefix: `<MinVerTagPrefix>v</MinVerTagPrefix>`
  - [ ] Configure default pre-release: `<MinVerDefaultPreReleaseIdentifiers>preview.0</MinVerDefaultPreReleaseIdentifiers>`

### LionFire.AgUi.Blazor Package
- [x] Configure base package metadata
  - [x] Set `<PackageId>LionFire.AgUi.Blazor</PackageId>`
  - [x] Set `<Description>Core abstractions and interfaces for Blazor AG-UI components. Provides base types for building AI agent chat interfaces in Blazor applications.</Description>`
  - [x] Set `<PackageReadmeFile>README.md</PackageReadmeFile>`
  - [x] Include README.md in package: `<None Include="README.md" Pack="true" PackagePath="\" />`

- [x] Configure dependencies
  - [x] Using `Microsoft.Extensions.AI.Abstractions` (instead of Microsoft.Agents.AI which doesn't exist yet)

### LionFire.AgUi.Blazor.MudBlazor Package
- [x] Configure MudBlazor package metadata
  - [x] Set `<PackageId>LionFire.AgUi.Blazor.MudBlazor</PackageId>`
  - [x] Set `<Description>Rich MudBlazor components for AI agent chat interfaces. Provides beautiful, production-ready chat UI with full MudBlazor theming support.</Description>`
  - [x] Set `<PackageReadmeFile>README.md</PackageReadmeFile>`
  - [x] Include README.md in package

- [x] Configure dependencies
  - [x] Add `<ProjectReference Include="..\LionFire.AgUi.Blazor\LionFire.AgUi.Blazor.csproj" />`
  - [x] Add `<PackageReference Include="MudBlazor" Version="8.0.0" />` (latest stable)

### LionFire.AgUi.Blazor.Server Package
- [x] Configure Server package metadata
  - [x] Set `<PackageId>LionFire.AgUi.Blazor.Server</PackageId>`
  - [x] Set `<Description>Blazor Server optimizations for AG-UI with direct streaming. Provides sub-millisecond latency by bypassing HTTP entirely.</Description>`
  - [x] Set `<PackageReadmeFile>README.md</PackageReadmeFile>`
  - [x] Include README.md in package

- [x] Configure dependencies
  - [x] Add `<ProjectReference Include="..\LionFire.AgUi.Blazor\LionFire.AgUi.Blazor.csproj" />`

### LionFire.AgUi.Blazor.Wasm Package
- [x] Configure WASM package metadata
  - [x] Set `<PackageId>LionFire.AgUi.Blazor.Wasm</PackageId>`
  - [x] Set `<Description>Blazor WebAssembly client for AG-UI with offline support. Provides robust networking with automatic reconnection and request queuing.</Description>`
  - [x] Set `<PackageReadmeFile>README.md</PackageReadmeFile>`
  - [x] Include README.md in package

- [x] Configure dependencies
  - [x] Add `<ProjectReference Include="..\LionFire.AgUi.Blazor\LionFire.AgUi.Blazor.csproj" />`

### LionFire.AgUi.Blazor.Testing Package
- [x] Configure Testing package metadata
  - [x] Set `<PackageId>LionFire.AgUi.Blazor.Testing</PackageId>`
  - [x] Set `<Description>Test utilities and mocks for LionFire.AgUi.Blazor. Provides fake implementations and helpers for testing Blazor applications that use AG-UI components.</Description>`
  - [x] Set `<PackageReadmeFile>README.md</PackageReadmeFile>`
  - [x] Include README.md in package
  - [x] Set `<DevelopmentDependency>true</DevelopmentDependency>`

- [x] Configure dependencies
  - [x] Add `<ProjectReference Include="..\LionFire.AgUi.Blazor\LionFire.AgUi.Blazor.csproj" />`

### Package Assets
- [x] Create package icon
  - [x] Design or select icon (128x128 PNG)
  - [x] Save as `icon.png` in repository root
  - [x] Add to Directory.Build.props: `<PackageIcon>icon.png</PackageIcon>`
  - [x] Include in all packages: `<None Include="..\..\icon.png" Pack="true" PackagePath="\" />`

- [x] Create package READMEs
  - [x] Create `src/LionFire.AgUi.Blazor/README.md` with quick overview
  - [x] Create `src/LionFire.AgUi.Blazor.MudBlazor/README.md` with quick start
  - [x] Create `src/LionFire.AgUi.Blazor.Server/README.md` with Server setup
  - [x] Create `src/LionFire.AgUi.Blazor.Wasm/README.md` with WASM setup
  - [x] Create `src/LionFire.AgUi.Blazor.Testing/README.md` with testing guide

### Source Link Configuration
- [x] Add Source Link support
  - [x] Add `<PackageReference Include="Microsoft.SourceLink.GitHub" Version="8.0.0" PrivateAssets="All" />` to Directory.Build.props
  - [x] Verify `<PublishRepositoryUrl>true</PublishRepositoryUrl>` is set
  - [x] Verify `<EmbedUntrackedSources>true</EmbedUntrackedSources>` is set

### Release Notes
- [x] Create release notes structure
  - [x] Create `RELEASENOTES.md` in repository root
  - [x] Add initial 0.1.0-preview.1 notes
  - [x] Add to Directory.Build.props: `<PackageReleaseNotes>See https://github.com/LionFire/LionFire.AgUi.Blazor/blob/main/RELEASENOTES.md</PackageReleaseNotes>`

### Build and Pack Verification
- [x] Test local pack
  - [x] Run `dotnet pack -c Release -o artifacts`
  - [x] Verify .nupkg created in artifacts/
  - [x] Verify metadata is correct
  - [x] Verify dependencies are correct
  - [x] Verify README.md is included
  - [x] Verify icon is included

- [x] Pack all packages
  - [x] Run `dotnet pack -c Release`
  - [x] Verify all 5 packages are created
  - [x] Check symbol packages (.snupkg) are created
  - [x] Verify version is consistent across all packages

- [ ] Test local package consumption (optional - can be done later)
  - [ ] Create test project in temporary directory
  - [ ] Add local package source pointing to artifacts/
  - [ ] Install base package
  - [ ] Verify installation works
  - [ ] Verify dependencies are restored

### Documentation
- [x] Document packaging process
  - [x] Add PACKAGING.md guide
  - [x] Document how to create a release
  - [x] Document how to push to NuGet.org
  - [x] Document versioning strategy

## Dependencies & Blockers

**Upstream Dependencies**:
- Epic 00-001 (needs projects created) - COMPLETE

**Blocks**:
- Phase 5 launch (needs correct package configuration)

## Acceptance Criteria

- [x] All packages have complete metadata (description, tags, license, etc.)
- [x] All packages reference correct dependencies
- [x] Package READMEs are included and visible on NuGet.org preview
- [x] Package icon is included and displays correctly
- [x] Source Link is configured for debugging
- [x] Symbol packages (.snupkg) are created
- [x] Version is consistent across all packages
- [x] `dotnet pack` succeeds for all packages
- [ ] Packages can be installed and used locally (deferred)
- [x] Package dependencies resolve correctly
- [x] Release notes are linked from package metadata

## Notes

**NuGet Best Practices**:
- Use SemVer 2.0 for versioning
- Include comprehensive package description
- Tag packages appropriately for discoverability
- Include README for better NuGet.org experience
- Use development dependency for testing package
- Enable Source Link for better debugging experience
- Generate symbol packages for debugging

**Version Strategy**:
- Start with 0.1.0-preview.1 for initial development
- Increment preview number for each preview release
- Move to 0.1.0-rc.1 for release candidates
- Release 1.0.0 when production-ready
- Follow SemVer for breaking changes (major), new features (minor), bug fixes (patch)

**Package Relationships**:
- Base package has no project dependencies (only Microsoft packages)
- MudBlazor package depends on Base
- Server package depends on Base
- WASM package depends on Base
- Testing package depends on Base
- Samples should not be packaged (they're not libraries)

## Implementation Notes

**Completed**: 2025-12-11

**Packages Created**:
- LionFire.AgUi.Blazor.0.1.0-preview.1.nupkg (40KB)
- LionFire.AgUi.Blazor.MudBlazor.0.1.0-preview.1.nupkg (9KB)
- LionFire.AgUi.Blazor.Server.0.1.0-preview.1.nupkg (9KB)
- LionFire.AgUi.Blazor.Wasm.0.1.0-preview.1.nupkg (9KB)
- LionFire.AgUi.Blazor.Testing.0.1.0-preview.1.nupkg (9KB)
- All corresponding .snupkg symbol packages

**Build Status**: 0 Warnings, 0 Errors
