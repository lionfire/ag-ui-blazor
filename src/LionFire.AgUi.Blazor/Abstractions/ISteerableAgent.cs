namespace LionFire.AgUi.Blazor.Abstractions;

/// <summary>
/// Represents an agent that supports "steering" - the ability to receive additional
/// user messages while a response is being generated. This allows for more dynamic
/// interaction where users can provide clarifications, corrections, or additional
/// context mid-response.
/// </summary>
/// <remarks>
/// <para>
/// Steering is an advanced capability that some AI providers support (e.g., Claude Code).
/// When an agent supports steering, queued messages may be sent immediately to influence
/// the ongoing response, rather than waiting for the response to complete.
/// </para>
/// <para>
/// <strong>Implementation Notes:</strong>
/// <list type="bullet">
/// <item>Not all providers support steering - check <see cref="SupportsSteering"/> first</item>
/// <item>Steering messages may cancel/abort the current response in some implementations</item>
/// <item>The exact behavior depends on the underlying provider</item>
/// </list>
/// </para>
/// </remarks>
public interface ISteerableAgent
{
    /// <summary>
    /// Gets whether this agent supports steering (sending messages during response generation).
    /// </summary>
    bool SupportsSteering { get; }

    /// <summary>
    /// Sends a steering message to the agent while a response is being generated.
    /// </summary>
    /// <param name="message">The steering message to send.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that completes when the steering message has been acknowledged.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the agent is not currently generating a response or does not support steering.
    /// </exception>
    Task SteerAsync(string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Event raised when the agent acknowledges a steering message.
    /// </summary>
    event EventHandler<SteeringEventArgs>? SteeringAcknowledged;
}

/// <summary>
/// Event arguments for steering events.
/// </summary>
public class SteeringEventArgs : EventArgs
{
    /// <summary>
    /// Creates a new instance of <see cref="SteeringEventArgs"/>.
    /// </summary>
    /// <param name="message">The steering message that was acknowledged.</param>
    /// <param name="responseModified">Whether the response was modified as a result.</param>
    public SteeringEventArgs(string message, bool responseModified)
    {
        Message = message;
        ResponseModified = responseModified;
        Timestamp = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Gets the steering message that was acknowledged.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets whether the agent's response was modified as a result of the steering message.
    /// </summary>
    public bool ResponseModified { get; }

    /// <summary>
    /// Gets when the steering was acknowledged.
    /// </summary>
    public DateTimeOffset Timestamp { get; }
}
