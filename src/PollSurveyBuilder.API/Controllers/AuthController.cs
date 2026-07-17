using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PollSurveyBuilder.Application.DTOs;
using PollSurveyBuilder.Application.IServices;
using PollSurveyBuilder.Domain.Entities.Identity;

namespace PollSurveyBuilder.API.Controllers
{
    /// <summary>
    /// Creator accounts only - voters never authenticate. A login is only needed
    /// to see the "my polls" dashboard and to close a poll early.
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    [EnableRateLimiting("vote")] // reuse the tight limiter to slow down credential stuffing
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;

        public AuthController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResultDTO>> Register(RegisterDTO dto)
        {
            var user = new AppUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                DisplayName = string.IsNullOrWhiteSpace(dto.DisplayName) ? dto.Email : dto.DisplayName,
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.Select(e => e.Description));
            }

            return Ok(BuildAuthResult(user));
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResultDTO>> Login(LoginDTO dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user is null)
                return Unauthorized(new { message = "Invalid email or password." });

            var check = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);
            if (!check.Succeeded)
                return Unauthorized(new { message = "Invalid email or password." });

            return Ok(BuildAuthResult(user));
        }

        private AuthResultDTO BuildAuthResult(AppUser user)
        {
            var (token, expiresAt) = _tokenService.CreateJwt(user);
            return new AuthResultDTO
            {
                Token = token,
                Email = user.Email!,
                DisplayName = user.DisplayName ?? user.Email!,
                ExpiresAt = expiresAt,
            };
        }
    }
}
