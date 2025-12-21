# LionFire.AgUi.Blazor.Testing

Test utilities and mocks for LionFire.AgUi.Blazor. Provides fake implementations and helpers for testing Blazor applications that use AG-UI components.

## Installation

```bash
dotnet add package LionFire.AgUi.Blazor.Testing
```

> **Note**: This is a development dependency and will not be included in your application's runtime dependencies.

## Overview

This package provides test utilities, mock implementations, and helpers for writing unit and integration tests for applications using AG-UI components.

## Features

- Mock AI service implementations
- Fake conversation state providers
- Test helpers for component rendering
- Assertion helpers for chat messages
- Pre-configured test scenarios

## Quick Start

1. Add the package to your test project:

```bash
dotnet add package LionFire.AgUi.Blazor.Testing
```

2. Use mock services in your tests:

```csharp
using LionFire.AgUi.Blazor.Testing;
using Xunit;

public class ChatComponentTests
{
    [Fact]
    public async Task SendMessage_DisplaysResponse()
    {
        // Arrange
        var mockAiService = new MockAiChatService();
        mockAiService.SetupResponse("Hello!", "Hi there! How can I help?");

        // Use with your test framework (e.g., bUnit)
        // ...
    }
}
```

3. Use test builders for complex scenarios:

```csharp
var conversation = new ConversationBuilder()
    .WithUserMessage("What is 2+2?")
    .WithAssistantMessage("2+2 equals 4.")
    .Build();
```

## Documentation

For full documentation, visit the [GitHub repository](https://github.com/LionFire/LionFire.AgUi.Blazor).

## Dependencies

- [LionFire.AgUi.Blazor](https://www.nuget.org/packages/LionFire.AgUi.Blazor)

## License

MIT License - see [LICENSE](https://github.com/LionFire/LionFire.AgUi.Blazor/blob/main/LICENSE) for details.
