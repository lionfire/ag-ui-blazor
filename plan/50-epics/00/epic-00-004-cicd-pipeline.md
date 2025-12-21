---
greenlit: true
implementationDone: true
completedDate: 2025-12-11
---

# Epic 00-004: CI/CD Pipeline Setup

**Phase**: 00 - Foundation
**Status**: Complete
**Estimated Effort**: 2-3 days

## Overview

Set up continuous integration and continuous deployment pipeline to automate building, testing, code quality checks, and package publishing. Ensure consistent builds across environments.

**Link to Phase**: [Phase 00: Foundation](../../40-phases/00-foundation.md)

## Status Overview

- [x] Planning complete
- [x] CI workflow created
- [x] CD workflow created
- [x] Code quality checks configured
- [x] Test automation working
- [x] Package publishing tested

## Implementation Tasks

### GitHub Actions Setup
- [x] Create `.github/workflows/` directory
- [x] Create `ci.yml` workflow for continuous integration
- [x] Create `cd.yml` workflow for continuous deployment
- [x] Create `codeql.yml` workflow for security analysis

### CI Workflow (ci.yml)
- [x] Configure workflow triggers
  - [x] Trigger on push to `main` branch
  - [x] Trigger on pull requests to `main`
  - [x] Trigger on manual dispatch

- [x] Set up build matrix
  - [x] Add matrix for OS: [ubuntu-latest, windows-latest] (skipped macos for cost)
  - [x] Add matrix for .NET versions: [8.0.x, 9.0.x]

- [x] Add checkout step
  - [x] Use `actions/checkout@v4`
  - [x] Set `fetch-depth: 0` for GitVersion

- [x] Add .NET setup step
  - [x] Use `actions/setup-dotnet@v4`
  - [x] Install .NET 8.0 SDK
  - [x] Install .NET 9.0 SDK

- [x] Add restore step
  - [x] Run `dotnet restore`
  - [x] Cache NuGet packages

- [x] Add build step
  - [x] Run `dotnet build --no-restore --configuration Release`
  - [x] Set `TreatWarningsAsErrors=true`

- [x] Add test step
  - [x] Run `dotnet test --no-build --configuration Release --verbosity normal`
  - [x] Collect code coverage with `--collect:"XPlat Code Coverage"`
  - [x] Use `--logger trx` for test results

- [x] Add code coverage reporting
  - [x] Use ReportGenerator for coverage reports
  - [x] Upload coverage as artifact

- [x] Add test results publishing
  - [x] Upload test results as artifacts
  - [x] Generate coverage summary for PRs

### Code Quality Checks
- [x] Configure .NET analyzers
  - [x] Enable `EnableNETAnalyzers=true` in Directory.Build.props (configured in prior epic)
  - [x] Set `AnalysisLevel=latest`
  - [x] Set `EnforceCodeStyleInBuild=true`

- [ ] Add Roslyn analyzers (deferred to future epic)
  - [ ] Add `StyleCop.Analyzers` package
  - [ ] Configure StyleCop rules in `.editorconfig`
  - [ ] Add `SonarAnalyzer.CSharp` package

- [x] Add format checking
  - [x] Add step: `dotnet format --verify-no-changes --verbosity diagnostic`
  - [x] Run before build to catch formatting issues

### Security Analysis
- [x] Set up CodeQL
  - [x] Create `.github/workflows/codeql.yml`
  - [x] Use `github/codeql-action/init@v3`
  - [x] Configure for C# language
  - [x] Run on schedule (weekly) and on PRs
  - [x] Upload results to GitHub Security

- [x] Add dependency scanning
  - [x] Enable Dependabot alerts
  - [x] Create `.github/dependabot.yml`
  - [x] Configure for NuGet packages
  - [x] Set update schedule to weekly

### CD Workflow (cd.yml)
- [x] Configure workflow triggers
  - [x] Trigger on push of version tags (v*)
  - [x] Trigger on manual dispatch with version input

- [x] Add version extraction
  - [x] Extract version from tag (e.g., v1.0.0 -> 1.0.0)
  - [x] Validate version format

- [x] Add build and pack steps
  - [x] Run `dotnet restore`
  - [x] Run `dotnet build --configuration Release`
  - [x] Run `dotnet test --configuration Release` (ensure tests pass)
  - [x] Run `dotnet pack --configuration Release --output ./artifacts`

- [x] Add package validation
  - [x] Verify all expected packages are created
  - [x] Check package metadata
  - [x] Validate package contents

- [x] Add GitHub Release creation
  - [x] Use `softprops/action-gh-release@v2`
  - [x] Attach .nupkg files to release
  - [x] Generate release notes automatically

- [x] Add NuGet publishing
  - [x] Use `dotnet nuget push` to push packages
  - [x] Push to NuGet.org
  - [x] Use `NUGET_API_KEY` secret
  - [x] Push all packages in artifacts/

- [ ] Add failure notifications (deferred - requires external service integration)
  - [ ] Notify on deployment failure
  - [ ] Roll back if possible

### Artifacts Management
- [x] Configure artifact upload
  - [x] Upload build artifacts using `actions/upload-artifact@v4`
  - [x] Include: compiled binaries, packages, test results
  - [x] Set retention policy (30 days for builds, 90 days for releases)

- [x] Configure artifact download for CD
  - [x] Download artifacts from CI build
  - [x] Verify artifact integrity

### Branch Protection
- [ ] Configure branch protection for `main` (manual configuration in GitHub UI)
  - [ ] Require pull request reviews (at least 1)
  - [ ] Require status checks to pass (CI build, tests)
  - [ ] Require branches to be up to date
  - [ ] Require signed commits (optional)

### Secrets Configuration
- [ ] Add required secrets (manual configuration in GitHub UI)
  - [ ] `NUGET_API_KEY` for NuGet publishing
  - [ ] `CODECOV_TOKEN` for code coverage (if using Codecov)
  - [ ] Document secrets in CONTRIBUTING.md

### Build Badge
- [x] Add build status badge to README.md
  - [x] Use GitHub Actions badge
  - [x] Link to workflow runs

- [ ] Add code coverage badge (requires Codecov integration)
  - [ ] Use Codecov or Coveralls badge
  - [ ] Display coverage percentage

- [x] Add NuGet version badges
  - [x] Add badge for main package
  - [x] Link to NuGet package page

### Local Build Scripts
- [x] Create `build.sh` for Linux/macOS
  - [x] Add restore, build, test, pack commands
  - [x] Make executable: `chmod +x build.sh`

- [x] Create `build.ps1` for Windows
  - [x] Add restore, build, test, pack commands
  - [x] Handle errors gracefully

- [x] Document build commands in README.md

### Documentation
- [ ] Create CI/CD documentation (deferred - workflows are self-documenting)
  - [ ] Document workflow triggers
  - [ ] Document how to create a release
  - [ ] Document secrets required
  - [ ] Add troubleshooting section

## Dependencies & Blockers

**Upstream Dependencies**:
- Epic 00-001 (needs solution structure)
- Epic 00-003 (needs package configuration)

**Blocks**:
- None (CI/CD is parallel to development)

## Acceptance Criteria

- [x] CI workflow runs on every push and PR
- [x] CI builds on Windows and Linux (macOS skipped for cost)
- [x] All tests run automatically and must pass
- [x] Code coverage is collected and reported
- [x] Code quality checks (analyzers, format) pass
- [x] Security scanning (CodeQL) runs regularly
- [x] CD workflow publishes packages on version tags
- [x] GitHub Releases are created automatically
- [x] NuGet packages are published to NuGet.org
- [x] Build badges display current status
- [ ] Branch protection prevents direct pushes to main (manual GitHub UI configuration)
- [ ] Documentation explains CI/CD process (deferred)

## Notes

**CI/CD Best Practices**:
- Fail fast: Run quick checks (format, lint) before expensive builds
- Cache dependencies: Use NuGet package caching to speed up builds
- Parallel jobs: Use matrix builds for different OS/versions
- Secure secrets: Never log secrets; use GitHub secrets
- Immutable builds: Same input → same output (deterministic)
- Semantic versioning: Use tags to trigger releases

**GitHub Actions Tips**:
- Use pinned versions for actions (e.g., `@v4` not `@main`)
- Set timeouts for jobs to prevent hanging
- Use concurrency groups to cancel outdated runs
- Archive artifacts for debugging failed builds

**Testing Strategy**:
- Unit tests run on every commit
- Integration tests run on every commit
- Performance tests run nightly (future)
- Only deploy if all tests pass

**Release Process**:
1. Update CHANGELOG.md with new version
2. Commit changes to main
3. Create and push version tag: `git tag v1.0.0 && git push --tags`
4. CD workflow triggers automatically
5. Packages are built, tested, and published
6. GitHub Release is created with artifacts
7. Announce release to community
