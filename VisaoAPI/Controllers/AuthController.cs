using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VisaoAPI.DTOs;
using VisaoAPI.Services;
using System.Security.Claims;

namespace VisaoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Register a new user account
        /// </summary>
        /// <param name="registerDto">User registration data</param>
        /// <returns>Authentication result with JWT token</returns>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(AuthResultDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(AuthResultDto))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(AuthResultDto))]
        public async Task<ActionResult<AuthResultDto>> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new AuthResultDto
                    {
                        Success = false,
                        Message = "Invalid input data"
                    });
                }

                var result = await _authService.RegisterAsync(registerDto);
                
                if (result.Success)
                {
                    _logger.LogInformation("User {Username} registered successfully", registerDto.Username);
                    return CreatedAtAction(nameof(Register), result);
                }

                if (result.Message.Contains("already exists"))
                {
                    return Conflict(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration");
                return StatusCode(500, new AuthResultDto
                {
                    Success = false,
                    Message = "An error occurred during registration"
                });
            }
        }

        /// <summary>
        /// Login with username/email and password
        /// </summary>
        /// <param name="loginDto">Login credentials</param>
        /// <returns>Authentication result with JWT token</returns>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthResultDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(AuthResultDto))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(AuthResultDto))]
        public async Task<ActionResult<AuthResultDto>> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new AuthResultDto
                    {
                        Success = false,
                        Message = "Invalid input data"
                    });
                }

                var result = await _authService.LoginAsync(loginDto);
                
                if (result.Success)
                {
                    _logger.LogInformation("User {UsernameOrEmail} logged in successfully", loginDto.UsernameOrEmail);
                    return Ok(result);
                }

                return Unauthorized(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user login");
                return StatusCode(500, new AuthResultDto
                {
                    Success = false,
                    Message = "An error occurred during login"
                });
            }
        }

        /// <summary>
        /// Get current user profile (requires authentication)
        /// </summary>
        /// <returns>Current user information</returns>
        [HttpGet("profile")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDto))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserDto>> GetProfile()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized();
                }

                var user = await _authService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile");
                return StatusCode(500, new { message = "An error occurred while retrieving profile" });
            }
        }

        /// <summary>
        /// Change user password (requires authentication)
        /// </summary>
        /// <param name="changePasswordDto">Password change data</param>
        /// <returns>Result of password change operation</returns>
        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = "Invalid input data" });
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized();
                }

                var result = await _authService.ChangePasswordAsync(userId, changePasswordDto);
                
                if (result.Success)
                {
                    _logger.LogInformation("Password changed successfully for user {UserId}", userId);
                    return Ok(new { message = "Password changed successfully" });
                }

                if (result.Message.Contains("not found"))
                {
                    return NotFound(new { message = result.Message });
                }

                return BadRequest(new { message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user");
                return StatusCode(500, new { message = "An error occurred while changing password" });
            }
        }

        /// <summary>
        /// Validate JWT token (useful for frontend token validation)
        /// </summary>
        /// <returns>Token validation result</returns>
        [HttpGet("validate-token")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult ValidateToken()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var usernameClaim = User.FindFirst(ClaimTypes.Name)?.Value;
                
                return Ok(new 
                { 
                    valid = true, 
                    userId = userIdClaim,
                    username = usernameClaim,
                    message = "Token is valid" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                return StatusCode(500, new { message = "An error occurred while validating token" });
            }
        }
    }
}