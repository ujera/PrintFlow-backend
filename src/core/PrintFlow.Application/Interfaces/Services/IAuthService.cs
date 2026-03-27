using PrintFlow.Application.DTOs.Auth;
using PrintFlow.Application.DTOs.Common;

namespace PrintFlow.Application.Interfaces.Services;

public interface IAuthService
{
    Task<ApiResult<AuthResponse>> AdminLoginAsync(AdminLoginRequest request);
    Task<ApiResult<AuthResponse>> GoogleLoginAsync(GoogleAuthRequest request);
    Task<ApiResult<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request);
    Task<ApiResult<UserDto>> GetCurrentUserAsync(Guid userId);
}