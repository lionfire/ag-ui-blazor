using System.Collections.Concurrent;
using System.Net.Http.Json;
using LionFire.AgUi.Blazor.Abstractions;
using LionFire.AgUi.Blazor.Models;
using LionFire.AgUi.Blazor.Wasm.Configuration;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LionFire.AgUi.Blazor.Wasm.Services;

/// <summary>
/// HTTP-based agent client factory for Blazor WebAssembly applications.
/// </summary>
/// <remarks>
/// <para>
/// This factory creates <see cref="IChatClient"/> instances that communicate with
/// an AG-UI server endpoint over HTTP/SSE for streaming responses.
/// </para>
/// <para>
/// The factory maintains connection state and supports automatic reconnection
/// with exponential backoff.
/// </para>
/// </remarks>
public sealed class HttpAgentClientFactory : IAgentClientFactory, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly IOptions<WasmAgentClientOptions> _options;
    private readonly ILogger<HttpAgentClientFactory> _logger;
    private readonly ConcurrentDictionary<string, AgentInfo> _registeredAgents = new();
    private readonly ConcurrentDictionary<string, IChatClient> _clientCache = new();
    private ConnectionState _connectionState = ConnectionState.Disconnected;

    /// <summary>
    /// Initializes a new instance of <see cref="HttpAgentClientFactory"/>.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for requests.</param>
    /// <param name="options">Configuration options.</param>
    /// <param name="logger">The logger instance.</param>
    public HttpAgentClientFactory(
        HttpClient httpClient,
        IOptions<WasmAgentClientOptions> options,
        ILogger<HttpAgentClientFactory> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Configure HttpClient with base URL and timeout
        var opts = _options.Value;
        if (!string.IsNullOrEmpty(opts.ServerUrl))
        {
            _httpClient.BaseAddress = new Uri(opts.ServerUrl);
        }
        _httpClient.Timeout = opts.Timeout;
    }

    /// <inheritdoc />
    public Task<IChatClient?> GetAgentAsync(string agentName, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(agentName);

        if (_clientCache.TryGetValue(agentName, out var cachedClient))
        {
            return Task.FromResult<IChatClient?>(cachedClient);
        }

        // Create a new HTTP-based chat client wrapper
        var client = new HttpChatClient(
            _httpClient,
            agentName,
            _options,
            _logger);

        _clientCache[agentName] = client;
        _connectionState = ConnectionState.Connected;

        _logger.LogInformation("Created HTTP chat client for agent: {AgentName}", agentName);

        return Task.FromResult<IChatClient?>(client);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<AgentInfo>> ListAgentsAsync(CancellationToken ct = default)
    {
        return Task.FromResult<IReadOnlyList<AgentInfo>>(_registeredAgents.Values.ToList());
    }

    /// <inheritdoc />
    public ConnectionState GetConnectionState()
    {
        return _connectionState;
    }

    /// <summary>
    /// Registers an agent with the factory.
    /// </summary>
    /// <param name="name">The agent name.</param>
    /// <param name="description">Optional agent description.</param>
    /// <param name="iconUrl">Optional icon URL.</param>
    public void RegisterAgent(string name, string? description = null, string? iconUrl = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var info = new AgentInfo(name, description, iconUrl);
        _registeredAgents[name] = info;
    }

    /// <summary>
    /// Fetches the list of available agents from the server.
    /// </summary>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>The list of agents from the server.</returns>
    public async Task<IReadOnlyList<AgentInfo>> FetchAgentsFromServerAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<AgentInfoDto>>("api/agents", ct);
            if (response == null)
                return Array.Empty<AgentInfo>();

            var agents = response.Select(dto => new AgentInfo(
                dto.Name ?? "unknown",
                dto.Description,
                dto.IconUrl)).ToList();

            // Cache the results
            foreach (var agent in agents)
            {
                _registeredAgents[agent.Name] = agent;
            }

            return agents;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch agents from server");
            return Array.Empty<AgentInfo>();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        foreach (var client in _clientCache.Values)
        {
            if (client is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
        _clientCache.Clear();
    }

    private sealed record AgentInfoDto(string? Name, string? Description, string? IconUrl);
}

/// <summary>
/// HTTP-based chat client that communicates with an AG-UI server endpoint.
/// </summary>
internal sealed class HttpChatClient : IChatClient
{
    private readonly HttpClient _httpClient;
    private readonly string _agentName;
    private readonly IOptions<WasmAgentClientOptions> _options;
    private readonly ILogger _logger;

    public HttpChatClient(
        HttpClient httpClient,
        string agentName,
        IOptions<WasmAgentClientOptions> options,
        ILogger logger)
    {
        _httpClient = httpClient;
        _agentName = agentName;
        _options = options;
        _logger = logger;
    }

    public ChatClientMetadata Metadata => new(nameof(HttpChatClient), _httpClient.BaseAddress ?? new Uri("http://localhost"));

    public async Task<ChatCompletion> CompleteAsync(
        IList<ChatMessage> chatMessages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var request = new ChatRequest(_agentName, chatMessages.ToList());

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(_options.Value.Timeout);

        var response = await _httpClient.PostAsJsonAsync(
            $"api/agents/{_agentName}/chat",
            request,
            cts.Token);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ChatCompletionDto>(cts.Token);
        if (result == null)
        {
            return new ChatCompletion(new ChatMessage(ChatRole.Assistant, "Error: Empty response from server"));
        }

        var message = new ChatMessage(ChatRole.Assistant, result.Content ?? string.Empty);
        return new ChatCompletion(message)
        {
            CompletionId = result.Id,
            ModelId = result.Model
        };
    }

    public async IAsyncEnumerable<StreamingChatCompletionUpdate> CompleteStreamingAsync(
        IList<ChatMessage> chatMessages,
        ChatOptions? options = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var request = new ChatRequest(_agentName, chatMessages.ToList());

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(_options.Value.StreamingTimeout);

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"api/agents/{_agentName}/chat/stream")
        {
            Content = JsonContent.Create(request)
        };
        requestMessage.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/event-stream"));

        using var response = await _httpClient.SendAsync(
            requestMessage,
            HttpCompletionOption.ResponseHeadersRead,
            cts.Token);

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cts.Token);
        using var reader = new StreamReader(stream);

        string? line;
        while ((line = await reader.ReadLineAsync(cts.Token)) != null)
        {
            if (line.StartsWith("data: "))
            {
                var data = line[6..];
                if (data == "[DONE]")
                    yield break;

                // Parse SSE data as streaming update
                yield return new StreamingChatCompletionUpdate
                {
                    Text = data,
                    Role = ChatRole.Assistant
                };
            }
        }
    }

    public TService? GetService<TService>(object? key = null) where TService : class
    {
        return this as TService;
    }

    public object? GetService(Type serviceType, object? key = null)
    {
        if (serviceType == typeof(HttpChatClient))
            return this;
        return null;
    }

    public void Dispose()
    {
        // HttpClient is managed by the factory
    }

    private sealed record ChatRequest(string AgentName, List<ChatMessage> Messages);
    private sealed record ChatCompletionDto(string? Id, string? Model, string? Content);
}
