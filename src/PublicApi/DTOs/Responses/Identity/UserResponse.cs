namespace PublicApi.DTOs.Responses;

public sealed class UserResponse
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string AccessToken { get; init; } = string.Empty;
}
