namespace LionFire.AgUi.Blazor.Models;

/// <summary>
/// Represents the connection state of an agent client.
/// </summary>
public enum ConnectionState
{
    /// <summary>
    /// The client is actively connected and able to communicate with the agent.
    /// </summary>
    Connected,

    /// <summary>
    /// The client is in the process of establishing a connection.
    /// </summary>
    Connecting,

    /// <summary>
    /// The client is not connected to the agent.
    /// </summary>
    Disconnected,

    /// <summary>
    /// The client lost connection and is attempting to reconnect.
    /// </summary>
    Reconnecting,

    /// <summary>
    /// The client encountered an error and cannot connect.
    /// </summary>
    Error
}
