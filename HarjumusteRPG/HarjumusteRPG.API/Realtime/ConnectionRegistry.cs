using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace HarjumusteRPG.API.Realtime;

public interface IConnectionRegistry
{
    Guid Add(int userId, WebSocket socket);
    void Remove(int userId, Guid connectionId);
    Task SendAsync(int userId, string message);
}

// Keeps the open WebSocket connections of every user (one user can have several devices).
public class ConnectionRegistry : IConnectionRegistry
{
    private sealed record Connection(WebSocket Socket, SemaphoreSlim SendLock);

    private readonly ConcurrentDictionary<int, ConcurrentDictionary<Guid, Connection>> _connections = new();
    private readonly ILogger<ConnectionRegistry> _logger;

    public ConnectionRegistry(ILogger<ConnectionRegistry> logger) => _logger = logger;

    public Guid Add(int userId, WebSocket socket)
    {
        var id = Guid.NewGuid();
        var userConnections = _connections.GetOrAdd(userId, _ => new());
        userConnections[id] = new Connection(socket, new SemaphoreSlim(1, 1));
        return id;
    }

    public void Remove(int userId, Guid connectionId)
    {
        if (_connections.TryGetValue(userId, out var userConnections))
            userConnections.TryRemove(connectionId, out _);
    }

    public async Task SendAsync(int userId, string message)
    {
        if (!_connections.TryGetValue(userId, out var userConnections))
            return;

        var bytes = Encoding.UTF8.GetBytes(message);
        foreach (var (id, connection) in userConnections)
        {
            if (connection.Socket.State != WebSocketState.Open)
            {
                Remove(userId, id);
                continue;
            }

            // a WebSocket allows only one send at a time
            await connection.SendLock.WaitAsync();
            try
            {
                await connection.Socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not send a message to user {UserId}", userId);
                Remove(userId, id);
            }
            finally
            {
                connection.SendLock.Release();
            }
        }
    }
}