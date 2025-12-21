using FluentAssertions;
using LionFire.AgUi.Blazor.Models;
using LionFire.AgUi.Blazor.Server.Configuration;
using LionFire.AgUi.Blazor.Server.Services;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace LionFire.AgUi.Blazor.Server.Tests.Services;

public class DirectAgentClientFactoryTests
{
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly ILogger<DirectAgentClientFactory> _logger;

    public DirectAgentClientFactoryTests()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
        _logger = NullLogger<DirectAgentClientFactory>.Instance;
    }

    private DirectAgentClientFactory CreateFactory(AgentRegistryOptions? options = null)
    {
        options ??= new AgentRegistryOptions();
        var optionsWrapper = Options.Create(options);
        return new DirectAgentClientFactory(_serviceProviderMock.Object, optionsWrapper, _logger);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullServiceProvider_ThrowsArgumentNullException()
    {
        // Arrange
        var options = Options.Create(new AgentRegistryOptions());

        // Act
        var act = () => new DirectAgentClientFactory(null!, options, _logger);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("serviceProvider");
    }

    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new DirectAgentClientFactory(_serviceProviderMock.Object, null!, _logger);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("options");
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        var options = Options.Create(new AgentRegistryOptions());

        // Act
        var act = () => new DirectAgentClientFactory(_serviceProviderMock.Object, options, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public void Constructor_WithValidParameters_Succeeds()
    {
        // Act
        var factory = CreateFactory();

        // Assert
        factory.Should().NotBeNull();
    }

    #endregion

    #region GetAgentAsync Tests

    [Fact]
    public async Task GetAgentAsync_WithNullAgentName_ThrowsArgumentException()
    {
        // Arrange
        var factory = CreateFactory();

        // Act
        var act = () => factory.GetAgentAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("agentName");
    }

    [Fact]
    public async Task GetAgentAsync_WithEmptyAgentName_ThrowsArgumentException()
    {
        // Arrange
        var factory = CreateFactory();

        // Act
        var act = () => factory.GetAgentAsync(string.Empty);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("agentName");
    }

    [Fact]
    public async Task GetAgentAsync_WithWhitespaceAgentName_ThrowsArgumentException()
    {
        // Arrange
        var factory = CreateFactory();

        // Act
        var act = () => factory.GetAgentAsync("   ");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("agentName");
    }

    [Fact]
    public async Task GetAgentAsync_WhenAgentExists_ReturnsAgent()
    {
        // Arrange - Use real DI container since GetKeyedService is an extension method
        var agentName = "test-agent";
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddKeyedSingleton<IChatClient>(agentName, new TestMockChatClient());

        var options = new AgentRegistryOptions();
        options.RegisterAgent(agentName, "Test agent");
        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(options));
        var provider = services.BuildServiceProvider();

        var factory = new DirectAgentClientFactory(
            provider,
            provider.GetRequiredService<IOptions<AgentRegistryOptions>>(),
            NullLogger<DirectAgentClientFactory>.Instance);

        // Act
        var result = await factory.GetAgentAsync(agentName);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<TestMockChatClient>();
    }

    [Fact]
    public async Task GetAgentAsync_WhenAgentNotFound_ReturnsNull()
    {
        // Arrange - Use real DI container since GetKeyedService is an extension method
        var services = new ServiceCollection();
        services.AddLogging();
        var options = new AgentRegistryOptions();
        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(options));
        var provider = services.BuildServiceProvider();

        var factory = new DirectAgentClientFactory(
            provider,
            provider.GetRequiredService<IOptions<AgentRegistryOptions>>(),
            NullLogger<DirectAgentClientFactory>.Instance);

        // Act
        var result = await factory.GetAgentAsync("nonexistent-agent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAgentAsync_WithCancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var factory = CreateFactory();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var act = () => factory.GetAgentAsync("test-agent", cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion

    #region ListAgentsAsync Tests

    [Fact]
    public async Task ListAgentsAsync_WithNoAgents_ReturnsEmptyList()
    {
        // Arrange
        var factory = CreateFactory();

        // Act
        var result = await factory.ListAgentsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ListAgentsAsync_WithRegisteredAgents_ReturnsAgentList()
    {
        // Arrange
        var options = new AgentRegistryOptions();
        options.RegisterAgent("agent1", "First agent");
        options.RegisterAgent("agent2", "Second agent");
        var factory = CreateFactory(options);

        // Act
        var result = await factory.ListAgentsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(a => a.Name == "agent1");
        result.Should().Contain(a => a.Name == "agent2");
    }

    [Fact]
    public async Task ListAgentsAsync_ReturnsImmutableSnapshot()
    {
        // Arrange
        var options = new AgentRegistryOptions();
        options.RegisterAgent("agent1", "First agent");
        var factory = CreateFactory(options);

        // Act - get list twice
        var result1 = await factory.ListAgentsAsync();
        var result2 = await factory.ListAgentsAsync();

        // Assert - should return same reference (immutable)
        result1.Should().BeSameAs(result2);
    }

    [Fact]
    public async Task ListAgentsAsync_WithCancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var factory = CreateFactory();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var act = () => factory.ListAgentsAsync(cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task ListAgentsAsync_PreservesAgentMetadata()
    {
        // Arrange
        var options = new AgentRegistryOptions();
        options.RegisterAgent("test-agent", "Test description", "https://example.com/icon.png");
        var factory = CreateFactory(options);

        // Act
        var result = await factory.ListAgentsAsync();

        // Assert
        var agent = result.Single();
        agent.Name.Should().Be("test-agent");
        agent.Description.Should().Be("Test description");
        agent.IconUrl.Should().Be("https://example.com/icon.png");
        agent.IsAvailable.Should().BeTrue();
    }

    #endregion

    #region GetConnectionState Tests

    [Fact]
    public void GetConnectionState_AlwaysReturnsConnected()
    {
        // Arrange
        var factory = CreateFactory();

        // Act
        var result = factory.GetConnectionState();

        // Assert
        result.Should().Be(ConnectionState.Connected);
    }

    [Fact]
    public void GetConnectionState_ReturnsConnected_EvenWithNoAgents()
    {
        // Arrange
        var factory = CreateFactory();

        // Act
        var result = factory.GetConnectionState();

        // Assert
        result.Should().Be(ConnectionState.Connected);
    }

    #endregion
}

/// <summary>
/// Simple mock implementation of IChatClient for unit testing.
/// </summary>
internal class TestMockChatClient : IChatClient
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

    public void Dispose() { }

    public TService? GetService<TService>(object? key = null) where TService : class => null;

    public object? GetService(Type serviceType, object? key = null) => null;
}
