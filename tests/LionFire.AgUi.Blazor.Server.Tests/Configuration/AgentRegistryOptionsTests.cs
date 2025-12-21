using FluentAssertions;
using LionFire.AgUi.Blazor.Server.Configuration;

namespace LionFire.AgUi.Blazor.Server.Tests.Configuration;

public class AgentRegistryOptionsTests
{
    [Fact]
    public void Constructor_InitializesEmptyAgentsDictionary()
    {
        // Act
        var options = new AgentRegistryOptions();

        // Assert
        options.Agents.Should().NotBeNull();
        options.Agents.Should().BeEmpty();
    }

    [Fact]
    public void RegisterAgent_WithValidName_AddsAgentToDictionary()
    {
        // Arrange
        var options = new AgentRegistryOptions();

        // Act
        options.RegisterAgent("test-agent", "Test description");

        // Assert
        options.Agents.Should().ContainKey("test-agent");
        options.Agents["test-agent"].Name.Should().Be("test-agent");
        options.Agents["test-agent"].Description.Should().Be("Test description");
    }

    [Fact]
    public void RegisterAgent_WithIconUrl_StoresIconUrl()
    {
        // Arrange
        var options = new AgentRegistryOptions();

        // Act
        options.RegisterAgent("test-agent", "Test description", "https://example.com/icon.png");

        // Assert
        options.Agents["test-agent"].IconUrl.Should().Be("https://example.com/icon.png");
    }

    [Fact]
    public void RegisterAgent_SetsIsAvailableToTrue()
    {
        // Arrange
        var options = new AgentRegistryOptions();

        // Act
        options.RegisterAgent("test-agent");

        // Assert
        options.Agents["test-agent"].IsAvailable.Should().BeTrue();
    }

    [Fact]
    public void RegisterAgent_WithNullName_ThrowsArgumentException()
    {
        // Arrange
        var options = new AgentRegistryOptions();

        // Act
        var act = () => options.RegisterAgent(null!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("name");
    }

    [Fact]
    public void RegisterAgent_WithEmptyName_ThrowsArgumentException()
    {
        // Arrange
        var options = new AgentRegistryOptions();

        // Act
        var act = () => options.RegisterAgent(string.Empty);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("name");
    }

    [Fact]
    public void RegisterAgent_WithWhitespaceName_ThrowsArgumentException()
    {
        // Arrange
        var options = new AgentRegistryOptions();

        // Act
        var act = () => options.RegisterAgent("   ");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("name");
    }

    [Fact]
    public void RegisterAgent_WithDuplicateName_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new AgentRegistryOptions();
        options.RegisterAgent("test-agent");

        // Act
        var act = () => options.RegisterAgent("test-agent");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*'test-agent'*already registered*");
    }

    [Fact]
    public void RegisterAgent_IsCaseSensitive()
    {
        // Arrange
        var options = new AgentRegistryOptions();

        // Act - Register with different cases
        options.RegisterAgent("TestAgent");
        options.RegisterAgent("testagent");
        options.RegisterAgent("TESTAGENT");

        // Assert - All should be registered separately
        options.Agents.Should().HaveCount(3);
        options.Agents.Should().ContainKey("TestAgent");
        options.Agents.Should().ContainKey("testagent");
        options.Agents.Should().ContainKey("TESTAGENT");
    }

    [Fact]
    public void RegisterAgent_WithNullDescription_StoresNullDescription()
    {
        // Arrange
        var options = new AgentRegistryOptions();

        // Act
        options.RegisterAgent("test-agent", description: null);

        // Assert
        options.Agents["test-agent"].Description.Should().BeNull();
    }

    [Fact]
    public void RegisterAgent_WithNullIconUrl_StoresNullIconUrl()
    {
        // Arrange
        var options = new AgentRegistryOptions();

        // Act
        options.RegisterAgent("test-agent", iconUrl: null);

        // Assert
        options.Agents["test-agent"].IconUrl.Should().BeNull();
    }

    [Fact]
    public void RegisterAgent_MultipleAgents_AllRegisteredCorrectly()
    {
        // Arrange
        var options = new AgentRegistryOptions();

        // Act
        options.RegisterAgent("agent1", "First agent");
        options.RegisterAgent("agent2", "Second agent");
        options.RegisterAgent("agent3", "Third agent");

        // Assert
        options.Agents.Should().HaveCount(3);
        options.Agents["agent1"].Description.Should().Be("First agent");
        options.Agents["agent2"].Description.Should().Be("Second agent");
        options.Agents["agent3"].Description.Should().Be("Third agent");
    }
}
