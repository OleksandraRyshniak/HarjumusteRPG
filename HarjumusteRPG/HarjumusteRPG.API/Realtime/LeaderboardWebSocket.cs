using System.Net.WebSockets;
using System.Security.Claims;

namespace HarjumusteRPG.API.Realtime;

public static class LeaderboardWebSocket
{
    // WebSocket endpoint /ws/leaderboard. Needs a JWT (access_token in the query string).
    public static void MapLeaderboardWebSocket(this IEndpointRouteBuilder app)
    {
        app.Map("/ws/leaderboard", async Task<IResult> (
            HttpContext context, IConnectionRegistry registry, ILoggerFactory loggerFactory) =>
        {
            if (!context.WebSockets.IsWebSocketRequest)
                return Results.Problem("This endpoint accepts WebSocket connections only.",
                    statusCode: StatusCodes.Status400BadRequest);

            var logger = loggerFactory.CreateLogger("LeaderboardWebSocket");
            var userId = int.Parse(context.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            using var socket = await context.WebSockets.AcceptWebSocketAsync();
            var connectionId = registry.Add(userId, socket);
            logger.LogInformation("WebSocket connected: user {UserId}", userId);

            try
            {
                await ListenAsync(socket, context.RequestAborted);
            }
            finally
            {
                registry.Remove(userId, connectionId);
                logger.LogInformation("WebSocket closed: user {UserId}", userId);
            }

            return Results.Empty;
        }).RequireAuthorization();
    }

    // The client sends nothing important, so we only wait until it closes the connection.
    private static async Task ListenAsync(WebSocket socket, CancellationToken ct)
    {
        var buffer = new byte[256];
        try
        {
            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(buffer, ct);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Bye", CancellationToken.None);
                    break;
                }
            }
        }
        catch (OperationCanceledException) { }
        catch (WebSocketException) { }
    }
}