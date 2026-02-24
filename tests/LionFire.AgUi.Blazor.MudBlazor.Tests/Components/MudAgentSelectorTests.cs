using Bunit;
using FluentAssertions;
using LionFire.AgUi.Blazor.MudBlazor.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;

namespace LionFire.AgUi.Blazor.MudBlazor.Tests.Components;

public class MudAgentSelectorTests : BunitContext, IAsyncLifetime
{
    public MudAgentSelectorTests()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
        Render<MudPopoverProvider>();
    }

    #region Rendering Tests

    [Fact]
    public void Renders_WithDefaultParameters()
    {
        // Act
        var cut = Render<MudAgentSelector>();

        // Assert
        cut.Markup.Should().Contain("mud-agent-selector");
    }

    [Fact]
    public void Renders_WithCustomLabel()
    {
        // Act
        var cut = Render<MudAgentSelector>(parameters => parameters
            .Add(p => p.Label, "Choose Agent"));

        // Assert
        cut.Markup.Should().Contain("Choose Agent");
    }

    [Fact]
    public void Stores_AgentOptions()
    {
        // Arrange
        var agents = new List<AgentInfo>
        {
            new("Agent1", "Description 1"),
            new("Agent2", "Description 2")
        };

        // Act
        var cut = Render<MudAgentSelector>(parameters => parameters
            .Add(p => p.Agents, agents));

        // Assert - verify the agents are stored (dropdown options are rendered lazily)
        cut.Instance.Agents.Should().HaveCount(2);
        cut.Instance.Agents[0].Name.Should().Be("Agent1");
        cut.Instance.Agents[1].Name.Should().Be("Agent2");
    }

    #endregion

    #region Parameter Tests

    [Fact]
    public void Label_DefaultsToSelectAgent()
    {
        // Act
        var cut = Render<MudAgentSelector>();

        // Assert
        cut.Instance.Label.Should().Be("Select Agent");
    }

    [Fact]
    public void Disabled_DefaultsToFalse()
    {
        // Act
        var cut = Render<MudAgentSelector>();

        // Assert
        cut.Instance.Disabled.Should().BeFalse();
    }

    [Fact]
    public void ReadOnly_DefaultsToFalse()
    {
        // Act
        var cut = Render<MudAgentSelector>();

        // Assert
        cut.Instance.ReadOnly.Should().BeFalse();
    }

    [Fact]
    public void Agents_DefaultsToEmpty()
    {
        // Act
        var cut = Render<MudAgentSelector>();

        // Assert
        cut.Instance.Agents.Should().BeEmpty();
    }

    #endregion

    #region AgentInfo Tests

    [Fact]
    public void AgentInfo_Record_StoresProperties()
    {
        // Arrange & Act
        var agent = new AgentInfo("TestAgent", "Test Description", "https://icon.url");

        // Assert
        agent.Name.Should().Be("TestAgent");
        agent.Description.Should().Be("Test Description");
        agent.IconUrl.Should().Be("https://icon.url");
    }

    [Fact]
    public void AgentInfo_Record_SupportsNullOptionalProperties()
    {
        // Arrange & Act
        var agent = new AgentInfo("TestAgent");

        // Assert
        agent.Name.Should().Be("TestAgent");
        agent.Description.Should().BeNull();
        agent.IconUrl.Should().BeNull();
    }

    #endregion

    Task IAsyncLifetime.InitializeAsync() => Task.CompletedTask;
    async Task IAsyncLifetime.DisposeAsync() => await base.DisposeAsync();
}
