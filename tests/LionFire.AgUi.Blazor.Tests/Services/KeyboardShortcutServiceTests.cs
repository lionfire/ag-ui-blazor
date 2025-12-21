using LionFire.AgUi.Blazor.Models;
using LionFire.AgUi.Blazor.Services;
using Xunit;

namespace LionFire.AgUi.Blazor.Tests.Services;

public class KeyboardShortcutServiceTests
{
    [Fact]
    public void Register_AddsShortcut()
    {
        // Arrange
        var service = new KeyboardShortcutService();
        var shortcut = new KeyboardShortcut("Enter", KeyModifiers.Control, "send", "Send");

        // Act
        service.Register(shortcut);

        // Assert
        Assert.Single(service.Shortcuts);
        Assert.Contains(shortcut, service.Shortcuts);
    }

    [Fact]
    public void Register_ReplacesExistingShortcutWithSameAction()
    {
        // Arrange
        var service = new KeyboardShortcutService();
        var shortcut1 = new KeyboardShortcut("Enter", KeyModifiers.Control, "send", "Send v1");
        var shortcut2 = new KeyboardShortcut("s", KeyModifiers.Control, "send", "Send v2");

        // Act
        service.Register(shortcut1);
        service.Register(shortcut2);

        // Assert
        Assert.Single(service.Shortcuts);
        Assert.Equal("Send v2", service.Shortcuts[0].Description);
    }

    [Fact]
    public void Unregister_RemovesShortcut()
    {
        // Arrange
        var service = new KeyboardShortcutService();
        var shortcut = new KeyboardShortcut("Enter", KeyModifiers.Control, "send", "Send");
        service.Register(shortcut);

        // Act
        service.Unregister("send");

        // Assert
        Assert.Empty(service.Shortcuts);
    }

    [Fact]
    public void Unregister_NonExistent_DoesNothing()
    {
        // Arrange
        var service = new KeyboardShortcutService();
        var shortcut = new KeyboardShortcut("Enter", KeyModifiers.Control, "send", "Send");
        service.Register(shortcut);

        // Act
        service.Unregister("non-existent");

        // Assert
        Assert.Single(service.Shortcuts);
    }

    [Fact]
    public void FindMatch_ReturnsMatchingShortcut()
    {
        // Arrange
        var service = new KeyboardShortcutService();
        var shortcut = new KeyboardShortcut("Enter", KeyModifiers.Control, "send", "Send");
        service.Register(shortcut);

        // Act
        var result = service.FindMatch("Enter", ctrlKey: true, altKey: false, shiftKey: false, metaKey: false);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("send", result.Action);
    }

    [Fact]
    public void FindMatch_NoMatch_ReturnsNull()
    {
        // Arrange
        var service = new KeyboardShortcutService();
        var shortcut = new KeyboardShortcut("Enter", KeyModifiers.Control, "send", "Send");
        service.Register(shortcut);

        // Act
        var result = service.FindMatch("k", ctrlKey: true, altKey: false, shiftKey: false, metaKey: false);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetDisplayString_ReturnsFormattedString()
    {
        // Arrange
        var service = new KeyboardShortcutService();
        var shortcut = new KeyboardShortcut("Enter", KeyModifiers.Control, "send", "Send");
        service.Register(shortcut);

        // Act
        var result = service.GetDisplayString("send");

        // Assert
        Assert.Equal("Ctrl+↵", result);
    }

    [Fact]
    public void GetDisplayString_WithMacPlatform_UsesSymbols()
    {
        // Arrange
        var service = new KeyboardShortcutService();
        service.SetPlatform(isMac: true);
        var shortcut = new KeyboardShortcut("Enter", KeyModifiers.Control, "send", "Send");
        service.Register(shortcut);

        // Act
        var result = service.GetDisplayString("send");

        // Assert
        Assert.Equal("⌃↵", result);
    }

    [Fact]
    public void GetDisplayString_NonExistentAction_ReturnsNull()
    {
        // Arrange
        var service = new KeyboardShortcutService();

        // Act
        var result = service.GetDisplayString("non-existent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ShortcutsByCategory_GroupsCorrectly()
    {
        // Arrange
        var service = new KeyboardShortcutService();
        service.Register(new KeyboardShortcut("Enter", KeyModifiers.Control, "send", "Send", "Messages"));
        service.Register(new KeyboardShortcut("k", KeyModifiers.Control, "new-chat", "New chat", "Chat"));
        service.Register(new KeyboardShortcut("Escape", KeyModifiers.None, "cancel", "Cancel", "Chat"));

        // Act
        var byCategory = service.ShortcutsByCategory;

        // Assert
        Assert.Equal(2, byCategory.Count);
        Assert.Single(byCategory["Messages"]);
        Assert.Equal(2, byCategory["Chat"].Count);
    }

    [Fact]
    public void SetPlatform_UpdatesIsMac()
    {
        // Arrange
        var service = new KeyboardShortcutService();

        // Act & Assert - Default
        Assert.False(service.IsMac);

        // Act & Assert - Set to Mac
        service.SetPlatform(isMac: true);
        Assert.True(service.IsMac);

        // Act & Assert - Set back to non-Mac
        service.SetPlatform(isMac: false);
        Assert.False(service.IsMac);
    }

    [Fact]
    public void RegisterDefaults_AddsExpectedShortcuts()
    {
        // Arrange
        var service = new KeyboardShortcutService();

        // Act
        service.RegisterDefaults();

        // Assert
        Assert.True(service.Shortcuts.Count >= 7);
        Assert.Contains(service.Shortcuts, s => s.Action == "send");
        Assert.Contains(service.Shortcuts, s => s.Action == "new-chat");
        Assert.Contains(service.Shortcuts, s => s.Action == "cancel");
        Assert.Contains(service.Shortcuts, s => s.Action == "show-shortcuts");
    }
}
