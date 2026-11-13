namespace HarjumusteRPG.API.Services;

public enum ServiceStatus
{
    Ok,
    NotFound,
    Conflict
}

public record ServiceResult<T>(ServiceStatus Status, T? Value = default);