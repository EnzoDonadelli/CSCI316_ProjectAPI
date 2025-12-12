using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VisaoAPI.DTOs;
using VisaoAPI.Models;
using VisaoAPI.Repositories;
using BCrypt.Net;

namespace VisaoAPI.Services
{
    public interface IAuthService
    {
        Task<AuthResultDto> RegisterAsync(RegisterDto registerDto);
        Task<AuthResultDto> LoginAsync(LoginDto loginDto);
        Task<UserDto?> GetUserByIdAsync(int id);
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<bool> UpdateUserAsync(int id, UpdateUserDto updateUserDto);
        Task<bool> DeleteUserAsync(int id);
        Task<AuthResultDto> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);
    }

    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IUserRepository userRepository, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<AuthResultDto> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                // Check if username exists
                if (await _userRepository.UsernameExistsAsync(registerDto.Username))
                {
                    return new AuthResultDto
                    {
                        Success = false,
                        Message = "Username already exists"
                    };
                }

                // Check if email exists
                if (await _userRepository.EmailExistsAsync(registerDto.Email))
                {
                    return new AuthResultDto
                    {
                        Success = false,
                        Message = "Email already exists"
                    };
                }

                // Hash password using BCrypt
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerDto.Password, BCrypt.Net.BCrypt.GenerateSalt(12));

                // Create user
                var user = new User
                {
                    Username = registerDto.Username,
                    Email = registerDto.Email,
                    PasswordHash = hashedPassword,
                    FullName = registerDto.FullName,
                    Bio = registerDto.Bio,
                    ProfilePic = registerDto.ProfilePic,
                    CreatedAt = DateTime.UtcNow
                };

                var createdUser = await _userRepository.CreateAsync(user);

                // Generate JWT token
                var token = GenerateJwtToken(createdUser);

                _logger.LogInformation("User {Username} registered successfully", registerDto.Username);

                return new AuthResultDto
                {
                    Success = true,
                    Message = "User registered successfully",
                    Token = token,
                    User = MapToUserDto(createdUser)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user {Username}", registerDto.Username);
                return new AuthResultDto
                {
                    Success = false,
                    Message = "An error occurred during registration"
                };
            }
        }

        public async Task<AuthResultDto> LoginAsync(LoginDto loginDto)
        {
            try
            {
                // Find user by username or email
                var user = await _userRepository.GetByUsernameAsync(loginDto.UsernameOrEmail) ??
                          await _userRepository.GetByEmailAsync(loginDto.UsernameOrEmail);

                if (user == null)
                {
                    _logger.LogWarning("Login attempt with invalid username/email: {UsernameOrEmail}", loginDto.UsernameOrEmail);
                    return new AuthResultDto
                    {
                        Success = false,
                        Message = "Invalid username/email or password"
                    };
                }

                // Verify password
                if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Login attempt with invalid password for user: {Username}", user.Username);
                    return new AuthResultDto
                    {
                        Success = false,
                        Message = "Invalid username/email or password"
                    };
                }

                // Generate JWT token
                var token = GenerateJwtToken(user);

                _logger.LogInformation("User {Username} logged in successfully", user.Username);

                return new AuthResultDto
                {
                    Success = true,
                    Message = "Login successful",
                    Token = token,
                    User = MapToUserDto(user)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for {UsernameOrEmail}", loginDto.UsernameOrEmail);
                return new AuthResultDto
                {
                    Success = false,
                    Message = "An error occurred during login"
                };
            }
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user != null ? MapToUserDto(user) : null;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.Select(MapToUserDto);
        }

        public async Task<bool> UpdateUserAsync(int id, UpdateUserDto updateUserDto)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null) return false;

                // Update only provided fields
                if (!string.IsNullOrEmpty(updateUserDto.FullName))
                    user.FullName = updateUserDto.FullName;
                
                if (updateUserDto.Bio != null)
                    user.Bio = updateUserDto.Bio;
                
                // Allow clearing profile picture by sending empty string explicitly
                if (updateUserDto.ProfilePic != null)
                    user.ProfilePic = updateUserDto.ProfilePic; // may be empty string to clear

                await _userRepository.UpdateAsync(user);
                _logger.LogInformation("User {UserId} updated successfully", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", id);
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            try
            {
                var result = await _userRepository.DeleteAsync(id);
                if (result)
                {
                    _logger.LogInformation("User {UserId} deleted successfully", id);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", id);
                return false;
            }
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
            var issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");
            var audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JWT Audience not configured");
            var expiryMinutesStr = jwtSettings["ExpiryMinutes"] ?? "60";
            var expiryMinutes = int.Parse(expiryMinutesStr);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Determine if user is admin based on configuration lists
            var adminSection = _configuration.GetSection("AdminUsers");
            var adminUsernames = adminSection.GetSection("Usernames").Get<string[]>() ?? Array.Empty<string>();
            var adminEmails = adminSection.GetSection("Emails").Get<string[]>() ?? Array.Empty<string>();
            var isAdmin = adminUsernames.Contains(user.Username, StringComparer.OrdinalIgnoreCase)
                          || adminEmails.Contains(user.Email, StringComparer.OrdinalIgnoreCase);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("userId", user.UserId.ToString()),
                new Claim("username", user.Username)
            };
            if (isAdmin)
            {
                // Add role claim to indicate admin privileges
                claims.Add(new Claim(ClaimTypes.Role, "admin"));
                claims.Add(new Claim("role", "admin"));
            }

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<AuthResultDto> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return new AuthResultDto
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                // Verify current password
                if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.PasswordHash))
                {
                    return new AuthResultDto
                    {
                        Success = false,
                        Message = "Current password is incorrect"
                    };
                }

                // Hash new password
                var hashedNewPassword = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);
                
                // Update password
                if (await _userRepository.UpdatePasswordAsync(userId, hashedNewPassword))
                {
                    return new AuthResultDto
                    {
                        Success = true,
                        Message = "Password changed successfully"
                    };
                }

                return new AuthResultDto
                {
                    Success = false,
                    Message = "Failed to update password"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", userId);
                return new AuthResultDto
                {
                    Success = false,
                    Message = "An error occurred while changing password"
                };
            }
        }

        private static UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Bio = user.Bio,
                ProfilePic = user.ProfilePic,
                CreatedAt = user.CreatedAt
            };
        }
    }
}