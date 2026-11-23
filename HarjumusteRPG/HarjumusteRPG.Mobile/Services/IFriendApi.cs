using HarjumusteRPG.Mobile.Models;

namespace HarjumusteRPG.Mobile.Services;

public interface IFriendApi
{
    Task<ApiResult<List<LeaderboardEntry>>> GetLeaderboardAsync();
    Task<ApiResult<List<FriendInfo>>> GetFriendsAsync();
    Task<ApiResult<List<FriendRequestInfo>>> GetRequestsAsync();
    Task<ApiResult<FriendRequestInfo>> SendRequestAsync(string username);
    Task<ApiResult<FriendInfo>> AcceptAsync(int requestId);
    Task<ApiResult<bool>> DeclineAsync(int requestId);
    Task<ApiResult<bool>> RemoveAsync(int friendId);
}