# Contributing to LionFire.AgUi.Blazor

Thank you for your interest in contributing to LionFire.AgUi.Blazor! This document provides guidelines and information about contributing to this project.

## Code of Conduct

By participating in this project, you agree to maintain a respectful and inclusive environment for everyone.

## How to Contribute

### Reporting Issues

- Check if the issue already exists in the [Issues](https://github.com/lionfire/ag-ui-blazor/issues) section
- Use a clear and descriptive title
- Provide detailed steps to reproduce the issue
- Include relevant code snippets, error messages, and screenshots
- Specify your environment (OS, .NET version, Blazor hosting model)

### Suggesting Enhancements

- Open an issue with the "enhancement" label
- Clearly describe the proposed feature and its use case
- Explain why this enhancement would be useful

### Pull Requests

1. **Fork the repository** and create your branch from `main`
2. **Follow the coding standards** (see below)
3. **Write tests** for new functionality
4. **Ensure all tests pass** by running `dotnet test`
5. **Update documentation** if needed
6. **Submit your pull request** with a clear description

## Development Setup

### Prerequisites

- .NET 8.0 SDK or later
- Git
- Your preferred IDE (Visual Studio, VS Code, Rider)

### Getting Started

```bash
# Clone your fork
git clone https://github.com/YOUR_USERNAME/ag-ui-blazor.git
cd ag-ui-blazor

# Add upstream remote
git remote add upstream https://github.com/lionfire/ag-ui-blazor.git

# Create a feature branch
git checkout -b feature/your-feature-name

# Restore and build
dotnet restore
dotnet build

# Run tests
dotnet test
```

## Coding Standards

### General Guidelines

- Follow C# coding conventions
- Use meaningful names for variables, methods, and classes
- Write self-documenting code with comments where necessary
- Keep methods focused and concise
- Use async/await for asynchronous operations

### Code Style

This project uses EditorConfig for consistent code formatting. Your IDE should automatically pick up the settings from `.editorconfig`.

Key style points:
- Use 4 spaces for indentation
- Use `var` when the type is obvious
- Prefer expression-bodied members for simple operations
- Use nullable reference types (`#nullable enable`)

### Testing

- Write unit tests for new functionality
- Use xUnit as the testing framework
- Use FluentAssertions for assertions
- Use Moq for mocking dependencies
- Use bUnit for Blazor component testing

### Commit Messages

- Use clear, descriptive commit messages
- Start with a verb in the imperative mood (e.g., "Add", "Fix", "Update")
- Reference issues when applicable (e.g., "Fix #123: ...")

## Project Structure

- `src/` - Source code for NuGet packages
- `tests/` - Unit and integration tests
- `samples/` - Sample applications
- `docs/` - Documentation

## Questions?

If you have questions, feel free to:
- Open an issue with the "question" label
- Start a discussion in the Discussions tab

Thank you for contributing!
