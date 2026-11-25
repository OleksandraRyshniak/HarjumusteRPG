using HarjumusteRPG.Mobile.Models;

namespace HarjumusteRPG.Mobile.Services;

public record ApiResult<T>(T? Value, string? Error = null)
{
    public bool Success => Error is null;
}

// A habit scheduled for today and whether it is already done.
public record TodayHabit(int Id, string Name, bool Done);

public interface IGameApi
{
    Task<ApiResult<ProgressInfo>> GetProgressAsync();
    Task<ApiResult<List<TodayHabit>>> GetTodayAsync();
    Task<ApiResult<CompleteResult>> CompleteAsync(int habitId);
    Task<ApiResult<List<CompletionInfo>>> GetHistoryAsync(int habitId, int days);
}