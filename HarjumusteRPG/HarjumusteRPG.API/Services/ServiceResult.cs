namespace HarjumusteRPG.API.Services;

public enum ServiceStatus
{
    Ok,
    NotFound,
    Conflict,
    Invalid
}

public record ServiceResult<T>(ServiceStatus Status, T? Value = default, string? Error = null);