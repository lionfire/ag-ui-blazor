using FluentAssertions;
using LionFire.AgUi.Blazor.Abstractions;
using LionFire.AgUi.Blazor.Models;

namespace LionFire.AgUi.Blazor.Wasm.Tests.Services;

public class ConnectionStateChangedEventArgsTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var previousState = ConnectionState.Disconnected;
        var newState = ConnectionState.Connected;
        var reason = "Test reason";

        var args = new ConnectionStateChangedEventArgs(previousState, newState, reason);

        args.PreviousState.Should().Be(previousState);
        args.NewState.Should().Be(newState);
        args.Reason.Should().Be(reason);
        args.Timestamp.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Constructor_AllowsNullReason()
    {
        var args = new ConnectionStateChangedEventArgs(
            ConnectionState.Connected,
            ConnectionState.Disconnected,
            null);

        args.Reason.Should().BeNull();
    }

    [Theory]
    [InlineData(ConnectionState.Disconnected, ConnectionState.Connecting)]
    [InlineData(ConnectionState.Connecting, ConnectionState.Connected)]
    [InlineData(ConnectionState.Connected, ConnectionState.Disconnected)]
    [InlineData(ConnectionState.Disconnected, ConnectionState.Reconnecting)]
    [InlineData(ConnectionState.Reconnecting, ConnectionState.Connected)]
    [InlineData(ConnectionState.Reconnecting, ConnectionState.Error)]
    public void Constructor_AcceptsValidStateTransitions(
        ConnectionState previousState,
        ConnectionState newState)
    {
        var args = new ConnectionStateChangedEventArgs(previousState, newState);

        args.PreviousState.Should().Be(previousState);
        args.NewState.Should().Be(newState);
    }
}
