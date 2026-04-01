using AutoMapper;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PrintFlow.Application.DTOs.Auth;
using PrintFlow.Application.DTOs.Common;
using PrintFlow.Application.Exceptions;
using PrintFlow.Application.Interfaces.Services;
using PrintFlow.Domain.Entities;
using PrintFlow.Domain.Enums;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PrintFlow.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;

    public AuthService(UserManager<User> userManager, IMapper mapper, IConfiguration configuration)
    {
        _userManager = userManager;
        _mapper = mapper;
        _configuration = configuration;
    }

    public async Task<ApiResult<AuthResponse>> AdminLoginAsync(AdminLoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email)
            ?? throw new UnauthorizedException("Invalid email or password.");

        if (user.Role != UserRole.Admin)
            throw new ForbiddenException("Access denied. Admin only.");

        if (await _userManager.IsLockedOutAsync(user))
            throw new UnauthorizedException("Account is locked. Try again later.");

        var validPassword = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!validPassword)
        {
            await _userManager.AccessFailedAsync(user);
            throw new UnauthorizedException("Invalid email or password.");
        }

        await _userManager.ResetAccessFailedCountAsync(user);
        return ApiResult<AuthResponse>.Ok(await GenerateAuthResponse(user));
    }

    public async Task<ApiResult<AuthResponse>> GoogleLoginAsync(GoogleAuthRequest request)
    {
        var googleClientId = _configuration["Google:ClientId"]
                                    ?? throw new BadRequestException("Google OAuth is not configured.");

        GoogleJsonWebSignature.Payload payload;
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { googleClientId }
            };
            payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);
        }
        catch (InvalidJwtException)
        {
            throw new UnauthorizedException("Invalid Google token.");
        }

        var user = await FindUserByGoogleIdOrEmail(payload.Subject, payload.Email);

        if (user is null)
        {
            user = new User
            {
                UserName = payload.Email,
                Email = payload.Email,
                Name = payload.Name ?? payload.Email,
                Role = UserRole.Customer,
                GoogleId = payload.Subject,
                AvatarUrl = payload.Picture,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                var errors = createResult.Errors.Select(e => e.Description).ToList();
                throw new BadRequestException(errors);
            }

            await _userManager.AddToRoleAsync(user, "Customer");
        }
        else if (user.GoogleId is null)
        {
            user.GoogleId = payload.Subject;
            user.AvatarUrl ??= payload.Picture;
            await _userManager.UpdateAsync(user);
        }

        return ApiResult<AuthResponse>.Ok(await GenerateAuthResponse(user));
    }

    public async Task<ApiResult<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        // Validate the refresh token by extracting claims
        var principal = GetPrincipalFromExpiredToken(request.RefreshToken);
        var userId = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedException("Invalid refresh token.");

        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new UnauthorizedException("User not found.");

        return ApiResult<AuthResponse>.Ok(await GenerateAuthResponse(user));
    }

    public async Task<ApiResult<UserDto>> GetCurrentUserAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new NotFoundException("User", userId);

        return ApiResult<UserDto>.Ok(_mapper.Map<UserDto>(user));
    }

    // ── Private helpers ──

    private async Task<User?> FindUserByGoogleIdOrEmail(string googleId, string email)
    {
        var user = _userManager.Users.FirstOrDefault(u => u.GoogleId == googleId);
        if (user is not null) return user;
        return await _userManager.FindByEmailAsync(email);
    }

    private async Task<AuthResponse> GenerateAuthResponse(User user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var expirationMinutes = int.Parse(jwtSettings["AccessTokenExpirationMinutes"] ?? "60");
        var expiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);

        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, user.Name),
            new("role", user.Role.ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        var refreshDays = int.Parse(jwtSettings["RefreshTokenExpirationDays"] ?? "7");
        var refreshToken = GenerateRefreshToken(user, refreshDays);

        return new AuthResponse
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            User = _mapper.Map<UserDto>(user)
        };
    }

    private string GenerateRefreshToken(User user, int expirationDays)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim("token_type", "refresh")
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(expirationDays),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var tokenValidation = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = false, // allow expired tokens for refresh
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!))
        };

        var handler = new JwtSecurityTokenHandler();
        try
        {
            return handler.ValidateToken(token, tokenValidation, out _);
        }
        catch
        {
            throw new UnauthorizedException("Invalid refresh token.");
        }
    }
}