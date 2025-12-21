using FluentAssertions;
using LionFire.AgUi.Blazor.Abstractions;
using LionFire.AgUi.Blazor.Server.Configuration;
using LionFire.AgUi.Blazor.Server.Extensions;
using LionFire.AgUi.Blazor.Server.Services;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;

namespace LionFire.AgUi.Blazor.Server.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    #region AddAgUiBlazorServer Tests

    [Fact]
    public void AddAgUiBlazorServer_RegistersRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddAgUiBlazorServer();
        var provider = services.BuildServiceProvider();

        // Assert
        var factory = provider.GetService<IAgentClientFactory>();
        factory.Should().NotBeNull();
        factory.Should().BeOfType<DirectAgentClientFactory>();
    }

    [Fact]
    public void AddAgUiBlazorServer_RegistersDirectAgentClientFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddAgUiBlazorServer();
        var provider = services.BuildServiceProvider();

        // Assert
        var factory = provider.GetService<DirectAgentClientFactory>();
        factory.Should().NotBeNull();
    }

    [Fact]
    public void AddAgUiBlazorServer_WithNullServices_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act
        var act = () => services!.AddAgUiBlazorServer();

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("services");
    }

    [Fact]
    public void AddAgUiBlazorServer_WithConfigureOptions_AppliesConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddAgUiBlazorServer(options =>
        {
            options.RegisterAgent("configured-agent", "Pre-configured agent");
        });
        var provider = services.BuildServiceProvider();

        // Assert
        var options = provider.GetRequiredService<IOptions<AgentRegistryOptions>>();
        options.Value.Agents.Should().ContainKey("configured-agent");
    }

    [Fact]
    public void AddAgUiBlazorServer_ReturnsServiceCollectionForChaining()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddAgUiBlazorServer();

        // Assert
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddAgUiBlazorServer_CalledMultipleTimes_DoesNotDuplicateRegistrations()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddAgUiBlazorServer();
        services.AddAgUiBlazorServer();
        var provider = services.BuildServiceProvider();

        // Assert - Should not throw, and should have only one factory instance
        var factory1 = provider.GetService<IAgentClientFactory>();
        var factory2 = provider.GetService<IAgentClientFactory>();
        factory1.Should().BeSameAs(factory2);
    }

    #endregion

    #region AddAgent<TAgent> Tests

    [Fact]
    public void AddAgent_Generic_RegistersAgentAsKeyedService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAgUiBlazorServer();

        // Act
        services.AddAgent<MockChatClient>("test-agent", "Test agent");
        var provider = services.BuildServiceProvider();

        // Assert
        var agent = provider.GetKeyedService<IChatClient>("test-agent");
        agent.Should().NotBeNull();
        agent.Should().BeOfType<MockChatClient>();
    }

    [Fact]
    public void AddAgent_Generic_RegistersInOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAgUiBlazorServer();

        // Act
        services.AddAgent<MockChatClient>("test-agent", "Test description", "https://icon.url");
        var provider = services.BuildServiceProvider();

        // Assert
        var options = provider.GetRequiredService<IOptions<AgentRegistryOptions>>();
        options.Value.Agents.Should().ContainKey("test-agent");
        options.Value.Agents["test-agent"].Description.Should().Be("Test description");
        options.Value.Agents["test-agent"].IconUrl.Should().Be("https://icon.url");
    }

    [Fact]
    public void AddAgent_Generic_WithNullServices_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act
        var act = () => services!.AddAgent<MockChatClient>("test");

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("services");
    }

    [Fact]
    public void AddAgent_Generic_WithNullName_ThrowsArgumentException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddAgent<MockChatClient>(null!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("name");
    }

    [Fact]
    public void AddAgent_Generic_WithEmptyName_ThrowsArgumentException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddAgent<MockChatClient>(string.Empty);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("name");
    }

    [Fact]
    public void AddAgent_Generic_ReturnsServiceCollectionForChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddAgUiBlazorServer();

        // Act
        var result = services.AddAgent<MockChatClient>("test");

        // Assert
        result.Should().BeSameAs(services);
    }

    #endregion

    #region AddAgent with Factory Tests

    [Fact]
    public void AddAgent_Factory_RegistersAgentAsKeyedService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAgUiBlazorServer();
        var mockAgent = new MockChatClient();

        // Act
        services.AddAgent("factory-agent", _ => mockAgent, "Factory agent");
        var provider = services.BuildServiceProvider();

        // Assert
        var agent = provider.GetKeyedService<IChatClient>("factory-agent");
        agent.Should().BeSameAs(mockAgent);
    }

    [Fact]
    public void AddAgent_Factory_InvokesFactoryWithServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAgUiBlazorServer();
        IServiceProvider? capturedProvider = null;

        // Act
        services.AddAgent("factory-agent", sp =>
        {
            capturedProvider = sp;
            return new MockChatClient();
        });
        var provider = services.BuildServiceProvider();
        _ = provider.GetKeyedService<IChatClient>("factory-agent");

        // Assert
        capturedProvider.Should().NotBeNull();
    }

    [Fact]
    public void AddAgent_Factory_WithNullServices_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act
        var act = () => services!.AddAgent("test", _ => new MockChatClient());

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("services");
    }

    [Fact]
    public void AddAgent_Factory_WithNullFactory_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddAgent("test", (Func<IServiceProvider, IChatClient>)null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("factory");
    }

    [Fact]
    public void AddAgent_Factory_WithNullName_ThrowsArgumentException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddAgent(null!, _ => new MockChatClient());

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("name");
    }

    #endregion

    #region AddAgent with Instance Tests

    [Fact]
    public void AddAgent_Instance_RegistersAgentAsKeyedService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAgUiBlazorServer();
        var mockAgent = new MockChatClient();

        // Act
        services.AddAgent("instance-agent", mockAgent, "Instance agent");
        var provider = services.BuildServiceProvider();

        // Assert
        var agent = provider.GetKeyedService<IChatClient>("instance-agent");
        agent.Should().BeSameAs(mockAgent);
    }

    [Fact]
    public void AddAgent_Instance_WithNullInstance_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddAgent("test", (IChatClient)null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("instance");
    }

    [Fact]
    public void AddAgent_Instance_WithNullName_ThrowsArgumentException()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockAgent = new MockChatClient();

        // Act
        var act = () => services.AddAgent(null!, mockAgent);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("name");
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task Integration_RegisteredAgent_CanBeRetrievedViaFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAgUiBlazorServer();
        services.AddAgent<MockChatClient>("integration-agent", "Integration test agent");

        var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IAgentClientFactory>();

        // Act
        var agent = await factory.GetAgentAsync("integration-agent");

        // Assert
        agent.Should().NotBeNull();
        agent.Should().BeOfType<MockChatClient>();
    }

    [Fact]
    public async Task Integration_MultipleAgents_AllListedCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAgUiBlazorServer();
        services.AddAgent<MockChatClient>("agent1", "First agent");
        services.AddAgent<MockChatClient>("agent2", "Second agent");

        var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IAgentClientFactory>();

        // Act
        var agents = await factory.ListAgentsAsync();

        // Assert
        agents.Should().HaveCount(2);
        agents.Should().Contain(a => a.Name == "agent1" && a.Description == "First agent");
        agents.Should().Contain(a => a.Name == "agent2" && a.Description == "Second agent");
    }

    [Fact]
    public async Task Integration_UnregisteredAgent_ReturnsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAgUiBlazorServer();

        var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IAgentClientFactory>();

        // Act
        var agent = await factory.GetAgentAsync("nonexistent");

        // Assert
        agent.Should().BeNull();
    }

    #endregion
}

/// <summary>
/// Mock implementation of IChatClient for testing purposes.
/// </summary>
public class MockChatClient : IChatClient
{
    public ChatClientMetadata Metadata => new ChatClientMetadata();

    public Task<ChatCompletion> CompleteAsync(
        IList<ChatMessage> chatMessages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ChatCompletion(new ChatMessage(ChatRole.Assistant, "Mock response")));
    }

    public async IAsyncEnumerable<StreamingChatCompletionUpdate> CompleteStreamingAsync(
        IList<ChatMessage> chatMessages,
        ChatOptions? options = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        yield break;
    }

    public void Dispose()
    {
    }

    public TService? GetService<TService>(object? key = null) where TService : class
    {
        return null;
    }

    public object? GetService(Type serviceType, object? key = null)
    {
        return null;
    }
}
