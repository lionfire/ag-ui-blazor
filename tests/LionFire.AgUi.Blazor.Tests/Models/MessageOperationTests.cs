using FluentAssertions;
using LionFire.AgUi.Blazor.Models;
using Xunit;

namespace LionFire.AgUi.Blazor.Tests.Models;

/// <summary>
/// Unit tests for the MessageOperationType and MessageOperationEventArgs classes.
/// </summary>
public class MessageOperationTests
{
    #region MessageOperationType Tests

    [Fact]
    public void MessageOperationType_HasExpectedValues()
    {
        // Assert
        Enum.GetValues<MessageOperationType>().Should().HaveCount(4);
        Enum.IsDefined(MessageOperationType.Edit).Should().BeTrue();
        Enum.IsDefined(MessageOperationType.Regenerate).Should().BeTrue();
        Enum.IsDefined(MessageOperationType.Copy).Should().BeTrue();
        Enum.IsDefined(MessageOperationType.Delete).Should().BeTrue();
    }

    #endregion

    #region MessageOperationEventArgs Constructor Tests

    [Fact]
    public void MessageOperationEventArgs_Constructor_SetsProperties()
    {
        // Arrange
        var messageIndex = 5;
        var operation = MessageOperationType.Edit;
        var newContent = "Updated content";

        // Act
        var args = new MessageOperationEventArgs(messageIndex, operation, newContent);

        // Assert
        args.MessageIndex.Should().Be(messageIndex);
        args.Operation.Should().Be(operation);
        args.NewContent.Should().Be(newContent);
    }

    [Fact]
    public void MessageOperationEventArgs_Constructor_WithoutNewContent()
    {
        // Arrange
        var messageIndex = 3;
        var operation = MessageOperationType.Regenerate;

        // Act
        var args = new MessageOperationEventArgs(messageIndex, operation);

        // Assert
        args.MessageIndex.Should().Be(messageIndex);
        args.Operation.Should().Be(operation);
        args.NewContent.Should().BeNull();
    }

    [Fact]
    public void MessageOperationEventArgs_Constructor_WithNullNewContent()
    {
        // Arrange
        var messageIndex = 2;
        var operation = MessageOperationType.Copy;

        // Act
        var args = new MessageOperationEventArgs(messageIndex, operation, null);

        // Assert
        args.MessageIndex.Should().Be(messageIndex);
        args.Operation.Should().Be(operation);
        args.NewContent.Should().BeNull();
    }

    [Fact]
    public void MessageOperationEventArgs_Constructor_WithZeroIndex()
    {
        // Arrange & Act
        var args = new MessageOperationEventArgs(0, MessageOperationType.Delete);

        // Assert
        args.MessageIndex.Should().Be(0);
    }

    [Fact]
    public void MessageOperationEventArgs_Constructor_WithNegativeIndex()
    {
        // Arrange & Act (negative index should still be allowed - validation is in component)
        var args = new MessageOperationEventArgs(-1, MessageOperationType.Edit, "content");

        // Assert
        args.MessageIndex.Should().Be(-1);
    }

    [Fact]
    public void MessageOperationEventArgs_Constructor_WithEmptyContent()
    {
        // Arrange & Act
        var args = new MessageOperationEventArgs(1, MessageOperationType.Edit, string.Empty);

        // Assert
        args.NewContent.Should().BeEmpty();
    }

    #endregion

    #region MessageOperationType Usage Scenarios

    [Fact]
    public void Edit_Operation_ShouldHaveNewContent()
    {
        // Arrange
        var args = new MessageOperationEventArgs(0, MessageOperationType.Edit, "Edited text");

        // Assert
        args.Operation.Should().Be(MessageOperationType.Edit);
        args.NewContent.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Regenerate_Operation_TypicallyHasNoNewContent()
    {
        // Arrange
        var args = new MessageOperationEventArgs(1, MessageOperationType.Regenerate);

        // Assert
        args.Operation.Should().Be(MessageOperationType.Regenerate);
        args.NewContent.Should().BeNull();
    }

    [Fact]
    public void Copy_Operation_HasNoNewContent()
    {
        // Arrange
        var args = new MessageOperationEventArgs(2, MessageOperationType.Copy);

        // Assert
        args.Operation.Should().Be(MessageOperationType.Copy);
        args.NewContent.Should().BeNull();
    }

    [Fact]
    public void Delete_Operation_HasNoNewContent()
    {
        // Arrange
        var args = new MessageOperationEventArgs(3, MessageOperationType.Delete);

        // Assert
        args.Operation.Should().Be(MessageOperationType.Delete);
        args.NewContent.Should().BeNull();
    }

    #endregion
}
