using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrintFlow.Application.DTOs.Auth;
using PrintFlow.Application.Interfaces.Services;

namespace PrintFlow.API.Controllers;

/// <summary>
/// Authentication and user management
/// </summary>
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Admin login with email and password
    /// </summary>
    /// <param name="request">Admin credentials</param>
    /// <returns>JWT access token and refresh token</returns>
    /// <response code="200">Login successful</response>
    /// <response code="401">Invalid credentials or account locked</response>
    /// <response code="403">Not an admin account</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(Application.DTOs.Common.ApiResult<AuthResponse>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> AdminLogin([FromBody] AdminLoginRequest request)
    {
        var result = await _authService.AdminLoginAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Customer login via Google OAuth2
    /// </summary>
    /// <param name="request">Google ID token from frontend</param>
    /// <returns>JWT tokens. Creates account on first login.</returns>
    /// <response code="200">Login successful</response>
    /// <response code="401">Invalid Google token</response>
    [HttpPost("google")]
    [ProducesResponseType(typeof(Application.DTOs.Common.ApiResult<AuthResponse>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleAuthRequest request)
    {
        var result = await _authService.GoogleLoginAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Refresh an expired access token
    /// </summary>
    /// <param name="request">Valid refresh token</param>
    /// <returns>New JWT access and refresh tokens</returns>
    /// <response code="200">Token refreshed</response>
    /// <response code="401">Invalid or expired refresh token</response>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(Application.DTOs.Common.ApiResult<AuthResponse>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Get current authenticated user profile
    /// </summary>
    /// <returns>User details</returns>
    /// <response code="200">User profile</response>
    /// <response code="401">Not authenticated</response>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(Application.DTOs.Common.ApiResult<UserDto>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var result = await _authService.GetCurrentUserAsync(GetUserId());
        return Ok(result);
    }

    /// <summary>
    /// [DEV ONLY] Register a test customer account
    /// </summary>
    /// <param name="request">Customer details and password</param>
    /// <returns>JWT tokens for the new customer</returns>
    /// <response code="200">Customer registered</response>
    /// <response code="400">Validation errors or weak password</response>
    /// <response code="409">Email already exists</response>
    [HttpPost("register-test")]
    [ProducesResponseType(typeof(Application.DTOs.Common.ApiResult<AuthResponse>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> RegisterTestCustomer([FromBody] TestRegisterRequest request)
    {
        var result = await _authService.RegisterTestCustomerAsync(request);
        return Ok(result);
    }
}