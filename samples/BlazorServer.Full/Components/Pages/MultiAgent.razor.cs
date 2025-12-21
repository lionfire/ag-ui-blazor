using System.Text;
using LionFire.AgUi.Blazor.Abstractions;
using LionFire.AgUi.Blazor.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using MudBlazor;

namespace BlazorServer.Full.Components.Pages;

/// <summary>
/// Multi-agent group chat page where multiple AI agents participate in the same conversation.
/// </summary>
public partial class MultiAgent : ComponentBase, IDisposable
{
    #region Agent Configuration

    /// <summary>
    /// Represents an agent's configuration for the UI.
    /// </summary>
    private record AgentConfig(string Name, string DisplayName, string Icon, Color Color);

    /// <summary>
    /// Available agents in the group chat.
    /// </summary>
    private readonly List<AgentConfig> _agents = new()
    {
        new AgentConfig("assistant", "Assistant", Icons.Material.Filled.Assistant, Color.Primary),
        new AgentConfig("coder", "Coder", Icons.Material.Filled.Code, Color.Secondary),
        new AgentConfig("researcher", "Researcher", Icons.Material.Filled.Science, Color.Tertiary)
    };

    #endregion

    #region Injected Services

    [Inject]
    private IAgentClientFactory AgentFactory { get; set; } = null!;

    [Inject]
    private ILogger<MultiAgent> Logger { get; set; } = null!;

    #endregion

    #region State

    private readonly List<ChatMessage> _messages = new();
    private readonly Dictionary<string, IChatClient?> _agentClients = new();
    private string? _selectedAgent;
    private IChatClient? _selectedAgentClient => _selectedAgent != null && _agentClients.TryGetValue(_selectedAgent, out var client) ? client : null;
    private bool _isStreaming;
    private string? _errorMessage;
    private CancellationTokenSource? _cts;
    private bool _disposed;

    #endregion

    #region Lifecycle

    protected override async Task OnInitializedAsync()
    {
        // Initialize all agents
        foreach (var agent in _agents)
        {
            try
            {
                var client = await AgentFactory.GetAgentAsync(agent.Name);
                _agentClients[agent.Name] = client;
                Logger.LogInformation("Initialized agent: {AgentName}", agent.Name);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to initialize agent: {AgentName}", agent.Name);
                _agentClients[agent.Name] = null;
            }
        }

        // Select first available agent
        _selectedAgent = _agents.FirstOrDefault(a => _agentClients.GetValueOrDefault(a.Name) != null)?.Name;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _cts?.Cancel();
        _cts?.Dispose();

        foreach (var client in _agentClients.Values)
        {
            if (client is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    #endregion

    #region Message Handling

    private async Task SendMessageAsync(string messageText)
    {
        if (string.IsNullOrWhiteSpace(messageText) || _selectedAgentClient == null || _selectedAgent == null)
        {
            return;
        }

        if (_isStreaming)
        {
            return; // Don't queue in group chat for simplicity
        }

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        try
        {
            _isStreaming = true;
            ClearError();

            // Add user message
            var userMsg = new ChatMessage(ChatRole.User, messageText);
            _messages.Add(userMsg);
            StateHasChanged();

            Logger.LogDebug("Sending message to agent {AgentName}: {MessagePreview}",
                _selectedAgent, messageText.Length > 50 ? messageText[..50] + "..." : messageText);

            // Create agent message placeholder with custom role for the agent name
            var agentRole = GetAgentRole(_selectedAgent);
            var agentMsg = new ChatMessage(agentRole, string.Empty);
            _messages.Add(agentMsg);
            StateHasChanged();

            // Stream response
            var contentBuilder = new StringBuilder();
            await foreach (var update in _selectedAgentClient.CompleteStreamingAsync(_messages, cancellationToken: _cts.Token))
            {
                if (update.Text != null)
                {
                    contentBuilder.Append(update.Text);
                    _messages[^1] = new ChatMessage(agentRole, contentBuilder.ToString());
                    StateHasChanged();
                }
            }

            Logger.LogDebug("Received complete response from agent {AgentName}, length: {ResponseLength}",
                _selectedAgent, contentBuilder.Length);
        }
        catch (OperationCanceledException)
        {
            Logger.LogInformation("Streaming response cancelled");
        }
        catch (Exception ex)
        {
            _errorMessage = $"Error: {ex.Message}";
            Logger.LogError(ex, "Error processing agent response");
        }
        finally
        {
            _isStreaming = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Gets a ChatRole for the agent. Uses custom role names so each agent is distinguishable.
    /// </summary>
    private static ChatRole GetAgentRole(string agentName)
    {
        // Use custom role names so the SenderTitleDisplayMode.Auto can detect multiple distinct senders
        return agentName switch
        {
            "assistant" => new ChatRole("Assistant"),
            "coder" => new ChatRole("Coder"),
            "researcher" => new ChatRole("Researcher"),
            _ => ChatRole.Assistant
        };
    }

    private Task HandleStopAsync()
    {
        _cts?.Cancel();
        return Task.CompletedTask;
    }

    private async Task HandleRegenerateAsync(int messageIndex)
    {
        if (_isStreaming || messageIndex <= 0 || messageIndex >= _messages.Count)
        {
            return;
        }

        // Find the user message before this agent message
        int userMessageIndex = messageIndex - 1;
        while (userMessageIndex >= 0 && _messages[userMessageIndex].Role == ChatRole.User)
        {
            userMessageIndex--;
        }
        userMessageIndex++; // Go back to the user message

        if (userMessageIndex < 0 || _messages[userMessageIndex].Role != ChatRole.User)
        {
            return;
        }

        var userMessage = _messages[userMessageIndex].Text ?? string.Empty;

        // Remove messages from the agent message onwards
        while (_messages.Count > messageIndex)
        {
            _messages.RemoveAt(_messages.Count - 1);
        }

        // Also remove the user message
        _messages.RemoveAt(userMessageIndex);
        StateHasChanged();

        // Resend
        await SendMessageAsync(userMessage);
    }

    private void ClearError() => _errorMessage = null;

    private string GetInputPlaceholder()
    {
        if (_selectedAgentClient == null)
        {
            return "No agent available...";
        }

        if (_isStreaming)
        {
            return "Agent is responding...";
        }

        var agentConfig = _agents.FirstOrDefault(a => a.Name == _selectedAgent);
        return $"Ask {agentConfig?.DisplayName ?? "the agent"}...";
    }

    #endregion
}
