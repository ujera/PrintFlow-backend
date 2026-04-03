using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrintFlow.Application.DTOs.Auth;
using PrintFlow.Application.Interfaces.Services;

namespace PrintFlow.API.Controllers;

[Route("api/auth")]
public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> AdminLogin([FromBody] AdminLoginRequest request)
    {
        var result = await _authService.AdminLoginAsync(request);
        return Ok(result);
    }

    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleAuthRequest request)
    {
        var result = await _authService.GoogleLoginAsync(request);
        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var result = await _authService.GetCurrentUserAsync(GetUserId());
        return Ok(result);
    }

    /// <summary>
    /// DEV ONLY — Register a test customer. Remove before production.
    /// </summary>
    [HttpPost("register-test")]
    public async Task<IActionResult> RegisterTestCustomer([FromBody] TestRegisterRequest request)
    {
        var result = await _authService.RegisterTestCustomerAsync(request);
        return Ok(result);
    }
}