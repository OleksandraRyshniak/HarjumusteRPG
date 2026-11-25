using HarjumusteRPG.API.Dtos;

namespace HarjumusteRPG.API.Services;

public interface IFriendService
{
    Task<List<FriendDto>> GetFriendsAsync(int userId);
    Task<List<FriendRequestDto>> GetIncomingRequestsAsync(int userId);
    Task<ServiceResult<FriendRequestDto>> SendRequestAsync(int userId, string username);
    Task<ServiceResult<FriendDto>> AcceptAsync(int requestId, int userId);
    Task<ServiceResult<bool>> DeclineAsync(int requestId, int userId);
    Task<ServiceResult<bool>> RemoveFriendAsync(int userId, int friendId);
    Task<List<LeaderboardEntryDto>> GetLeaderboardAsync(int userId);
}