# Packaging Guide

This document describes how to create NuGet packages for the LionFire.AgUi.Blazor project.

## Prerequisites

- .NET 8.0 SDK or later
- Git (for version control integration)
- NuGet.org API key (for publishing)

## Package Structure

The solution produces the following NuGet packages:

| Package | Description |
|---------|-------------|
| LionFire.AgUi.Blazor | Core abstractions and interfaces |
| LionFire.AgUi.Blazor.MudBlazor | MudBlazor UI components |
| LionFire.AgUi.Blazor.Server | Blazor Server optimizations |
| LionFire.AgUi.Blazor.Wasm | WebAssembly client support |
| LionFire.AgUi.Blazor.Testing | Test utilities (development dependency) |

## Building Packages Locally

### Build All Packages

```bash
dotnet pack -c Release
```

This creates `.nupkg` and `.snupkg` files in each project's `bin/Release` directory.

### Build a Specific Package

```bash
dotnet pack src/LionFire.AgUi.Blazor/LionFire.AgUi.Blazor.csproj -c Release
```

### Specify Output Directory

```bash
dotnet pack -c Release -o ./artifacts
```

### Verify Package Contents

Use NuGet Package Explorer or the `dotnet nuget` command to inspect packages:

```bash
# List package contents
unzip -l artifacts/LionFire.AgUi.Blazor.0.1.0-preview.1.nupkg
```

## Versioning Strategy

This project follows [Semantic Versioning 2.0](https://semver.org/):

- **MAJOR**: Breaking changes to public APIs
- **MINOR**: New features (backward compatible)
- **PATCH**: Bug fixes (backward compatible)

### Version Formats

| Stage | Format | Example |
|-------|--------|---------|
| Development | `0.x.y-preview.z` | `0.1.0-preview.1` |
| Release Candidate | `x.y.z-rc.n` | `1.0.0-rc.1` |
| Stable | `x.y.z` | `1.0.0` |

### Updating Version

The version is defined in `Directory.Build.props`:

```xml
<Version>0.1.0-preview.1</Version>
```

To release a new version:

1. Update the `<Version>` element in `Directory.Build.props`
2. Update `RELEASENOTES.md` with changes
3. Commit changes
4. Create a git tag: `git tag v0.1.0-preview.2`
5. Push the tag: `git push origin v0.1.0-preview.2`

## Publishing to NuGet.org

### Prerequisites

1. Create an account on [NuGet.org](https://www.nuget.org/)
2. Generate an API key with push permissions
3. Store the API key securely

### Push Packages

```bash
# Push all packages
dotnet nuget push "artifacts/*.nupkg" --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json

# Push with symbol packages
dotnet nuget push "artifacts/*.nupkg" --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json --skip-duplicate
```

### Using GitHub Actions (Recommended)

For automated releases, set up a GitHub Actions workflow:

1. Add `NUGET_API_KEY` to repository secrets
2. Create a workflow that triggers on tag push
3. Build, pack, and push packages automatically

Example workflow trigger:

```yaml
on:
  push:
    tags:
      - 'v*'
```

## Local Testing

### Create a Local NuGet Source

```bash
# Create local feed directory
mkdir -p ~/.nuget/local-feed

# Pack to local feed
dotnet pack -c Release -o ~/.nuget/local-feed
```

### Configure Local Source

Add to `NuGet.config`:

```xml
<configuration>
  <packageSources>
    <add key="local" value="/home/user/.nuget/local-feed" />
  </packageSources>
</configuration>
```

### Test Installation

```bash
# Create test project
mkdir test-project && cd test-project
dotnet new console
dotnet add package LionFire.AgUi.Blazor --source ~/.nuget/local-feed
```

## Package Validation Checklist

Before publishing, verify:

- [ ] `dotnet build` succeeds with 0 warnings
- [ ] `dotnet pack -c Release` succeeds
- [ ] All 5 packages are created (.nupkg)
- [ ] Symbol packages are created (.snupkg)
- [ ] Package metadata is correct (description, tags, license)
- [ ] README.md is included in each package
- [ ] Icon is included in each package
- [ ] Dependencies are correctly specified
- [ ] Version numbers are consistent across all packages
- [ ] RELEASENOTES.md is updated

## Troubleshooting

### Package Not Found After Local Install

Clear the NuGet cache:

```bash
dotnet nuget locals all --clear
```

### Symbol Package Not Uploading

Ensure `IncludeSymbols` and `SymbolPackageFormat` are set in `Directory.Build.props`:

```xml
<IncludeSymbols>true</IncludeSymbols>
<SymbolPackageFormat>snupkg</SymbolPackageFormat>
```

### Missing README in Package

Ensure the README is included in the project file:

```xml
<ItemGroup>
  <None Include="README.md" Pack="true" PackagePath="\" />
</ItemGroup>
```

## Resources

- [NuGet Documentation](https://docs.microsoft.com/nuget/)
- [Creating and Publishing Packages](https://docs.microsoft.com/nuget/create-packages/overview-and-workflow)
- [Package Versioning](https://docs.microsoft.com/nuget/concepts/package-versioning)
- [Source Link](https://github.com/dotnet/sourcelink)
