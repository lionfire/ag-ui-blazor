using LionFire.AgUi.Blazor.Models;
using Xunit;

namespace LionFire.AgUi.Blazor.Tests.Models;

public class KeyboardShortcutTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        // Arrange & Act
        var shortcut = new KeyboardShortcut(
            "Enter",
            KeyModifiers.Control,
            "send",
            "Send message",
            "Messages");

        // Assert
        Assert.Equal("Enter", shortcut.Key);
        Assert.Equal(KeyModifiers.Control, shortcut.Modifiers);
        Assert.Equal("send", shortcut.Action);
        Assert.Equal("Send message", shortcut.Description);
        Assert.Equal("Messages", shortcut.Category);
    }

    [Fact]
    public void Constructor_DefaultCategory_IsGeneral()
    {
        // Arrange & Act
        var shortcut = new KeyboardShortcut(
            "k",
            KeyModifiers.Control,
            "new-chat",
            "Start new chat");

        // Assert
        Assert.Equal("General", shortcut.Category);
    }

    [Theory]
    [InlineData("Enter", KeyModifiers.Control, false, "Ctrl+↵")]
    [InlineData("Enter", KeyModifiers.Control, true, "⌃↵")]
    [InlineData("k", KeyModifiers.Control, false, "Ctrl+K")]
    [InlineData("k", KeyModifiers.Control, true, "⌃K")]
    [InlineData("k", KeyModifiers.Meta, false, "Cmd+K")]
    [InlineData("k", KeyModifiers.Meta, true, "⌘K")]
    [InlineData("Escape", KeyModifiers.None, false, "Esc")]
    [InlineData("Escape", KeyModifiers.None, true, "Esc")]
    [InlineData("ArrowUp", KeyModifiers.Control, false, "Ctrl+↑")]
    [InlineData("ArrowDown", KeyModifiers.Control, true, "⌃↓")]
    [InlineData("/", KeyModifiers.Control | KeyModifiers.Shift, false, "Ctrl+Shift+/")]
    [InlineData("/", KeyModifiers.Control | KeyModifiers.Shift, true, "⌃⇧/")]
    public void GetDisplayString_FormatsCorrectly(string key, KeyModifiers modifiers, bool useMacSymbols, string expected)
    {
        // Arrange
        var shortcut = new KeyboardShortcut(key, modifiers, "test", "Test");

        // Act
        var result = shortcut.GetDisplayString(useMacSymbols);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Enter", true, false, false, false, true)]  // Ctrl+Enter matches
    [InlineData("Enter", false, false, false, true, true)]  // Cmd+Enter matches (Mac)
    [InlineData("Enter", false, false, false, false, false)] // Enter without Ctrl doesn't match
    [InlineData("Enter", true, true, false, false, false)]  // Ctrl+Alt+Enter doesn't match (extra modifier)
    [InlineData("enter", true, false, false, false, true)]  // Case insensitive
    [InlineData("ENTER", true, false, false, false, true)]  // Case insensitive
    public void Matches_CtrlEnter(string key, bool ctrl, bool alt, bool shift, bool meta, bool expected)
    {
        // Arrange
        var shortcut = new KeyboardShortcut("Enter", KeyModifiers.Control, "send", "Send");

        // Act
        var result = shortcut.Matches(key, ctrl, alt, shift, meta);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Escape", false, false, false, false, true)]   // Escape alone matches
    [InlineData("Escape", true, false, false, false, false)]   // Ctrl+Escape doesn't match
    [InlineData("escape", false, false, false, false, true)]   // Case insensitive
    public void Matches_EscapeNoModifiers(string key, bool ctrl, bool alt, bool shift, bool meta, bool expected)
    {
        // Arrange
        var shortcut = new KeyboardShortcut("Escape", KeyModifiers.None, "cancel", "Cancel");

        // Act
        var result = shortcut.Matches(key, ctrl, alt, shift, meta);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Matches_DifferentKey_ReturnsFalse()
    {
        // Arrange
        var shortcut = new KeyboardShortcut("Enter", KeyModifiers.Control, "send", "Send");

        // Act
        var result = shortcut.Matches("k", true, false, false, false);

        // Assert
        Assert.False(result);
    }
}
