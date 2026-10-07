using ApplicationCore.Entities;
using ApplicationCore.Exceptions;
using ApplicationCore.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PublicApi.DTOs.Requests;
using PublicApi.DTOs.Responses;
using System.Security.Claims;

namespace PublicApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IToken _token;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IWebHostEnvironment _environment;
    private readonly TimeSpan _refreshTokenLifetime;
    private const string RefreshCookieName = "ministore_refresh";

    public UserController(
        IUserService userService,
        IToken token,
        IRefreshTokenService refreshTokenService,
        IWebHostEnvironment environment,
        IConfiguration configuration)
    {
        _userService = userService;
        _token = token;
        _refreshTokenService = refreshTokenService;
        _environment = environment;
        _refreshTokenLifetime = TimeSpan.FromDays(configuration.GetValue<int?>("RefreshToken:ExpirationDays") ?? 30);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var user = await _userService.RegisterAsync(request.Email, request.Password);
        return StatusCode(StatusCodes.Status201Created, new ApiResponse<object>
        {
            StatusCode = StatusCodes.Status201Created,
            Message = "Registration successful. Check your email for the verification code.",
            Data = null
        });
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail(VerifyEmailRequest request)
    {
        var user = await _userService.VerifyEmailAsync(request.Email, request.Code);
        var response = await CreateUserResponseAsync(user);

        return Ok(new ApiResponse<UserResponse>
        {
            StatusCode = StatusCodes.Status200OK,
            Message = "Email verified successfully.",
            Data = response
        });
    }

    [HttpPost("resend-verification")]
    public async Task<IActionResult> ResendVerification(ResendVerificationRequest request)
    {
        await _userService.SendEmailVerificationAsync(request.Email);

        return Ok(new ApiResponse<object>
        {
            StatusCode = StatusCodes.Status200OK,
            Message = "If the account needs verification, a code has been sent.",
            Data = null
        });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
    {
        await _userService.RequestPasswordResetAsync(request.Email);

        return Ok(new ApiResponse<object>
        {
            StatusCode = StatusCodes.Status200OK,
            Message = "If the account exists, a password reset code has been sent.",
            Data = null
        });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
    {
        await _userService.ResetPasswordAsync(request.Email, request.Code, request.NewPassword);

        return Ok(new ApiResponse<object>
        {
            StatusCode = StatusCodes.Status200OK,
            Message = "Password reset successfully.",
            Data = null
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _userService.LoginAsync(request.Email, request.Password);
        var response = await CreateUserResponseAsync(user);

        return Ok(new ApiResponse<UserResponse>
        {
            StatusCode = StatusCodes.Status200OK,
            Message = "Login successful.",
            Data = response
        });
    }
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken()
    {
        var refreshToken = Request.Cookies[RefreshCookieName]
            ?? throw new UnauthorizedException("Refresh token is required.");
        var (user, nextRefreshToken) = await _refreshTokenService.RotateAsync(refreshToken);
        SetRefreshCookie(nextRefreshToken);
        var response = CreateUserResponse(user);

        return Ok(new ApiResponse<UserResponse>
        {
            StatusCode = StatusCodes.Status200OK,
            Message = "Token refreshed successfully.",
            Data = response
        });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies[RefreshCookieName];
        if (!string.IsNullOrWhiteSpace(refreshToken))
            await _refreshTokenService.RevokeAsync(refreshToken);

        DeleteRefreshCookie();
        return Ok(new ApiResponse<object?>
        {
            StatusCode = StatusCodes.Status200OK,
            Message = "Logged out successfully.",
            Data = null
        });
    }
    [Authorize]
    [HttpPatch("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordReq request)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedException("Chưa xác thực");
        }
        await _userService.ChangePasswordAsync(userId, request.OldPassword, request.NewPassword);
        return Ok(new ApiResponse<object>
        {
            StatusCode = StatusCodes.Status200OK,
            Message = "ChangePassword successful",
            Data = null
        });
    }
    private async Task<UserResponse> CreateUserResponseAsync(User user)
    {
        var refreshToken = await _refreshTokenService.CreateAsync(user.Id);
        SetRefreshCookie(refreshToken);
        return CreateUserResponse(user);
    }

    private UserResponse CreateUserResponse(User user) => new()
    {
        UserId = user.Id,
        Email = user.Email,
        AccessToken = _token.CreateAccessToken(user)
    };

    private void SetRefreshCookie(string refreshToken)
    {
        Response.Cookies.Append(RefreshCookieName, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = !_environment.IsDevelopment(),
            SameSite = SameSiteMode.Lax,
            Path = "/api/user",
            Expires = DateTimeOffset.UtcNow.Add(_refreshTokenLifetime),
            IsEssential = true
        });
    }

    private void DeleteRefreshCookie()
    {
        Response.Cookies.Delete(RefreshCookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = !_environment.IsDevelopment(),
            SameSite = SameSiteMode.Lax,
            Path = "/api/user",
            IsEssential = true
        });
    }
}
