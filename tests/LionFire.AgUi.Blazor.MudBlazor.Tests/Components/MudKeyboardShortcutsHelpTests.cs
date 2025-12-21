using Bunit;
using LionFire.AgUi.Blazor.Models;
using LionFire.AgUi.Blazor.MudBlazor.Components;
using MudBlazor;
using MudBlazor.Services;
using Xunit;

namespace LionFire.AgUi.Blazor.MudBlazor.Tests.Components;

public class MudKeyboardShortcutsHelpTests : TestContext
{
    public MudKeyboardShortcutsHelpTests()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
        RenderComponent<MudPopoverProvider>();
    }

    [Fact]
    public void ShortcutsByCategory_Property_IsSet()
    {
        // Arrange
        var shortcuts = CreateTestShortcuts();

        // Act
        var cut = RenderComponent<MudKeyboardShortcutsHelp>(parameters =>
            parameters.Add(p => p.ShortcutsByCategory, shortcuts));

        // Assert
        Assert.NotNull(cut.Instance.ShortcutsByCategory);
        Assert.Equal(2, cut.Instance.ShortcutsByCategory!.Count);
    }

    [Fact]
    public void IsMac_Property_DefaultsToFalse()
    {
        // Arrange
        var shortcuts = CreateTestShortcuts();

        // Act
        var cut = RenderComponent<MudKeyboardShortcutsHelp>(parameters =>
            parameters.Add(p => p.ShortcutsByCategory, shortcuts));

        // Assert
        Assert.False(cut.Instance.IsMac);
    }

    [Fact]
    public void IsMac_Property_CanBeSetToTrue()
    {
        // Arrange
        var shortcuts = CreateTestShortcuts();

        // Act
        var cut = RenderComponent<MudKeyboardShortcutsHelp>(parameters => parameters
            .Add(p => p.ShortcutsByCategory, shortcuts)
            .Add(p => p.IsMac, true));

        // Assert
        Assert.True(cut.Instance.IsMac);
    }

    [Fact]
    public void Component_Renders_MudDialog()
    {
        // Arrange
        var shortcuts = CreateTestShortcuts();

        // Act
        var cut = RenderComponent<MudKeyboardShortcutsHelp>(parameters =>
            parameters.Add(p => p.ShortcutsByCategory, shortcuts));

        // Assert - MudDialog should be in the tree
        var dialog = cut.FindComponent<MudDialog>();
        Assert.NotNull(dialog);
    }

    [Fact]
    public void Component_Renders_WithNullShortcuts()
    {
        // Act - Should not throw
        var cut = RenderComponent<MudKeyboardShortcutsHelp>(parameters =>
            parameters.Add(p => p.ShortcutsByCategory, null));

        // Assert
        Assert.NotNull(cut);
    }

    [Fact]
    public void GetCategoryOrder_ReturnsCorrectOrder()
    {
        // Test via reflection since GetCategoryOrder is protected
        var methodInfo = typeof(MudKeyboardShortcutsHelp)
            .GetMethod("GetCategoryOrder", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        Assert.NotNull(methodInfo);

        // Messages should be first (0)
        Assert.Equal(0, methodInfo.Invoke(null, new object[] { "Messages" }));

        // Chat should be second (1)
        Assert.Equal(1, methodInfo.Invoke(null, new object[] { "Chat" }));

        // Navigation should be third (2)
        Assert.Equal(2, methodInfo.Invoke(null, new object[] { "Navigation" }));

        // Help should be fourth (3)
        Assert.Equal(3, methodInfo.Invoke(null, new object[] { "Help" }));

        // Unknown categories should be at end (99)
        Assert.Equal(99, methodInfo.Invoke(null, new object[] { "Unknown" }));
    }

    [Fact]
    public void Component_ContainsExpectedShortcuts_InCategories()
    {
        // Arrange
        var shortcuts = CreateTestShortcuts();

        // Act
        var cut = RenderComponent<MudKeyboardShortcutsHelp>(parameters =>
            parameters.Add(p => p.ShortcutsByCategory, shortcuts));

        // Assert - Verify the shortcuts are accessible
        var byCategory = cut.Instance.ShortcutsByCategory!;

        Assert.True(byCategory.ContainsKey("Messages"));
        Assert.Single(byCategory["Messages"]);
        Assert.Equal("send", byCategory["Messages"][0].Action);

        Assert.True(byCategory.ContainsKey("Chat"));
        Assert.Equal(2, byCategory["Chat"].Count);
    }

    private static Dictionary<string, IReadOnlyList<KeyboardShortcut>> CreateTestShortcuts()
    {
        return new Dictionary<string, IReadOnlyList<KeyboardShortcut>>
        {
            ["Messages"] = new List<KeyboardShortcut>
            {
                new("Enter", KeyModifiers.Control, "send", "Send message", "Messages")
            }.AsReadOnly(),
            ["Chat"] = new List<KeyboardShortcut>
            {
                new("k", KeyModifiers.Control, "new-chat", "Start new chat", "Chat"),
                new("Escape", KeyModifiers.None, "cancel", "Cancel streaming", "Chat")
            }.AsReadOnly()
        };
    }
}
