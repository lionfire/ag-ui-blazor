using Bunit;
using FluentAssertions;
using LionFire.AgUi.Blazor.MudBlazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using Xunit;

namespace LionFire.AgUi.Blazor.MudBlazor.Tests.Components;

/// <summary>
/// Unit tests for the MudCodeBlock component.
/// </summary>
public class MudCodeBlockTests : TestContext
{
    public MudCodeBlockTests()
    {
        // Add required services
        Services.AddMudServices();

        // Add JSInterop mocks
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void Component_Renders_Successfully()
    {
        // Act
        var cut = RenderComponent<MudCodeBlock>();

        // Assert
        cut.Should().NotBeNull();
        cut.Find(".mud-code-block").Should().NotBeNull();
    }

    [Fact]
    public void Component_Renders_CodeContent()
    {
        // Arrange
        var code = "var x = 1;";

        // Act
        var cut = RenderComponent<MudCodeBlock>(parameters => parameters
            .Add(p => p.Code, code));

        // Assert
        cut.Markup.Should().Contain("var x = 1;");
    }

    [Fact]
    public void Component_Renders_PreCodeElements()
    {
        // Arrange
        var code = "const y = 2;";

        // Act
        var cut = RenderComponent<MudCodeBlock>(parameters => parameters
            .Add(p => p.Code, code));

        // Assert
        cut.Markup.Should().Contain("<pre");
        cut.Markup.Should().Contain("<code");
    }

    [Fact]
    public void Component_ShowsLanguageLabel_WhenLanguageSet()
    {
        // Arrange
        var code = "var x = 1;";
        var language = "csharp";

        // Act
        var cut = RenderComponent<MudCodeBlock>(parameters => parameters
            .Add(p => p.Code, code)
            .Add(p => p.Language, language));

        // Assert
        cut.Find(".mud-code-block-header").Should().NotBeNull();
        cut.Find(".mud-code-block-language").TextContent.Should().Be("C#");
    }

    [Fact]
    public void Component_HidesHeader_WhenNoLanguageOrCopyButton()
    {
        // Arrange
        var code = "var x = 1;";

        // Act
        var cut = RenderComponent<MudCodeBlock>(parameters => parameters
            .Add(p => p.Code, code)
            .Add(p => p.ShowCopyButton, false)
            .Add(p => p.Language, null));

        // Assert
        cut.FindAll(".mud-code-block-header").Should().BeEmpty();
    }

    [Fact]
    public void Component_ShowsCopyButton_ByDefault()
    {
        // Arrange
        var code = "var x = 1;";

        // Act
        var cut = RenderComponent<MudCodeBlock>(parameters => parameters
            .Add(p => p.Code, code));

        // Assert
        cut.Instance.ShowCopyButton.Should().BeTrue();
        cut.FindComponent<MudIconButton>().Should().NotBeNull();
    }

    [Fact]
    public void Component_HidesCopyButton_WhenDisabled()
    {
        // Arrange
        var code = "var x = 1;";

        // Act
        var cut = RenderComponent<MudCodeBlock>(parameters => parameters
            .Add(p => p.Code, code)
            .Add(p => p.ShowCopyButton, false)
            .Add(p => p.Language, "csharp")); // Keep language to have header

        // Assert
        cut.FindAll(".mud-code-block-copy-button").Should().BeEmpty();
    }

    [Fact]
    public void Component_AppliesLanguageClass()
    {
        // Arrange
        var code = "var x = 1;";
        var language = "csharp";

        // Act
        var cut = RenderComponent<MudCodeBlock>(parameters => parameters
            .Add(p => p.Code, code)
            .Add(p => p.Language, language));

        // Assert
        var codeElement = cut.Find("code");
        codeElement.ClassList.Should().Contain("language-csharp");
    }

    [Fact]
    public void Component_AppliesCustomClass()
    {
        // Arrange
        var customClass = "my-custom-class";

        // Act
        var cut = RenderComponent<MudCodeBlock>(parameters => parameters
            .Add(p => p.Class, customClass));

        // Assert
        cut.Find(".mud-code-block").ClassList.Should().Contain(customClass);
    }

    [Fact]
    public void Component_AppliesAdditionalAttributes()
    {
        // Arrange
        var attributes = new Dictionary<string, object>
        {
            { "data-testid", "test-codeblock" }
        };

        // Act
        var cut = RenderComponent<MudCodeBlock>(parameters => parameters
            .Add(p => p.AdditionalAttributes, attributes));

        // Assert
        cut.Find(".mud-code-block").GetAttribute("data-testid").Should().Be("test-codeblock");
    }

    [Fact]
    public void Component_EnableHighlighting_DefaultsToTrue()
    {
        // Act
        var cut = RenderComponent<MudCodeBlock>();

        // Assert
        cut.Instance.EnableHighlighting.Should().BeTrue();
    }

    [Fact]
    public void Component_ShowLineNumbers_DefaultsToFalse()
    {
        // Act
        var cut = RenderComponent<MudCodeBlock>();

        // Assert
        cut.Instance.ShowLineNumbers.Should().BeFalse();
    }

    [Fact]
    public void Component_AppliesLineNumbersClass_WhenEnabled()
    {
        // Arrange
        var code = "var x = 1;";

        // Act
        var cut = RenderComponent<MudCodeBlock>(parameters => parameters
            .Add(p => p.Code, code)
            .Add(p => p.ShowLineNumbers, true));

        // Assert
        var preElement = cut.Find("pre");
        preElement.ClassList.Should().Contain("line-numbers");
    }

    [Theory]
    [InlineData("csharp", "C#")]
    [InlineData("cs", "C#")]
    [InlineData("javascript", "JavaScript")]
    [InlineData("js", "JavaScript")]
    [InlineData("typescript", "TypeScript")]
    [InlineData("ts", "TypeScript")]
    [InlineData("python", "Python")]
    [InlineData("py", "Python")]
    [InlineData("sql", "SQL")]
    [InlineData("json", "JSON")]
    [InlineData("xml", "XML")]
    [InlineData("bash", "Bash")]
    [InlineData("sh", "Bash")]
    [InlineData("yaml", "YAML")]
    [InlineData("yml", "YAML")]
    [InlineData("html", "HTML")]
    [InlineData("css", "CSS")]
    [InlineData("go", "Go")]
    [InlineData("rust", "Rust")]
    [InlineData("java", "Java")]
    [InlineData("powershell", "PowerShell")]
    [InlineData("ps1", "PowerShell")]
    [InlineData("unknown", "UNKNOWN")]
    public void Component_DisplaysCorrectLanguageName(string language, string expectedDisplay)
    {
        // Act
        var cut = RenderComponent<MudCodeBlock>(parameters => parameters
            .Add(p => p.Code, "code")
            .Add(p => p.Language, language));

        // Assert
        cut.Find(".mud-code-block-language").TextContent.Should().Be(expectedDisplay);
    }

    [Fact]
    public void Component_ReRendersOnCodeChange()
    {
        // Arrange
        var cut = RenderComponent<MudCodeBlock>(parameters => parameters
            .Add(p => p.Code, "initial code"));

        // Act
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.Code, "updated code"));

        // Assert
        cut.Markup.Should().Contain("updated code");
        cut.Markup.Should().NotContain("initial code");
    }

    [Fact]
    public void Component_OnCopied_EventCanBeSet()
    {
        // Act
        var cut = RenderComponent<MudCodeBlock>(parameters => parameters
            .Add(p => p.Code, "test code")
            .Add(p => p.OnCopied, EventCallback.Factory.Create<string>(this, _ => { })));

        // Assert
        cut.Instance.Should().NotBeNull();
        // The event callback is properly set
    }

    [Fact]
    public void Component_OnCopyFailed_EventCanBeSet()
    {
        // Act
        var cut = RenderComponent<MudCodeBlock>(parameters => parameters
            .Add(p => p.Code, "test code")
            .Add(p => p.OnCopyFailed, EventCallback.Factory.Create<string>(this, _ => { })));

        // Assert
        cut.Instance.Should().NotBeNull();
        // The event callback is properly set
    }

    [Fact]
    public async Task Component_DisposeAsync_CompletesSuccessfully()
    {
        // Arrange
        var cut = RenderComponent<MudCodeBlock>(parameters => parameters
            .Add(p => p.Code, "test code"));

        // Act & Assert - Should not throw
        await cut.InvokeAsync(async () =>
        {
            await cut.Instance.DisposeAsync();
        });
    }

    [Fact]
    public void Component_PreservesWhitespace()
    {
        // Arrange
        var code = "if (true)\n{\n    Console.WriteLine();\n}";

        // Act
        var cut = RenderComponent<MudCodeBlock>(parameters => parameters
            .Add(p => p.Code, code));

        // Assert
        cut.Markup.Should().Contain("if (true)");
        cut.Markup.Should().Contain("Console.WriteLine()");
    }

    [Fact]
    public void Component_HandlesEmptyCode()
    {
        // Act
        var cut = RenderComponent<MudCodeBlock>(parameters => parameters
            .Add(p => p.Code, string.Empty));

        // Assert
        cut.Should().NotBeNull();
        cut.Find(".mud-code-block").Should().NotBeNull();
    }

    [Fact]
    public void Component_HandlesNullCode()
    {
        // Act
        var cut = RenderComponent<MudCodeBlock>(parameters => parameters
            .Add(p => p.Code, null));

        // Assert
        cut.Should().NotBeNull();
        cut.Find(".mud-code-block").Should().NotBeNull();
    }
}
