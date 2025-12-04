using AutoMapper;
using FootballField.API.Shared.Utils;
using FootballField.API.Modules.AuthManagement.Dtos;
using FootballField.API.Modules.UserManagement.Repositories;
using FootballField.API.Modules.UserManagement.Entities;
using FootballField.API.Modules.UserManagement.Dtos;
using System.Security.Cryptography;

namespace FootballField.API.Modules.AuthManagement.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtHelper _jwtHelper;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository, 
            JwtHelper jwtHelper, 
            IConfiguration configuration, 
            IMapper mapper,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _jwtHelper = jwtHelper;
            _configuration = configuration;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<LoginResponse?> RegisterAsync(RegisterRequest request)
        {
            // Kiểm tra email đã tồn tại
            if (await _userRepository.EmailExistsAsync(request.Email))
                return null;

            // Kiểm tra số điện thoại đã tồn tại
            var existingPhone = await _userRepository.GetByPhoneAsync(request.Phone);
            if (existingPhone != null)
                return null;

            // Tạo user mới
            var user = new User
            {
                LastName = request.LastName,
                FirstName = request.FirstName,
                Email = request.Email,
                Phone = request.Phone,
                Password = HashPassword(request.Password),
                Status = UserStatus.Active
                // CreatedAt và UpdatedAt sẽ được set bởi ApplicationDbContext.UpdateTimestamps()
            };

            // Lưu user vào database
            await _userRepository.AddAsync(user);
            
            // Assign Customer role via USER_ROLE table
            var customerRole = await _userRepository.GetRoleByNameAsync("Customer");
            if (customerRole != null)
            {
                await _userRepository.AddUserRoleAsync(user.Id, customerRole.Id);
            }

            // Reload user with UserRoles for JWT token generation
            user = await _userRepository.GetByIdWithRolesAsync(user.Id);
            if (user == null)
                throw new InvalidOperationException("Failed to reload user after registration");

            // Generate JWT token and refresh token
            var token = _jwtHelper.GenerateToken(user);
            var refreshToken = GenerateRefreshToken();
            var expiryMinutes = int.Parse(_configuration["JwtSettings:ExpiryMinutes"] ?? "60");
            var refreshTokenExpiryDays = int.Parse(_configuration["JwtSettings:RefreshTokenExpiryDays"] ?? "7");
            var vietnamNow = TimeZoneHelper.VietnamNow;

            // Revoke all existing refresh tokens for this user (ensure only 1 active token)
            await _userRepository.RevokeAllUserRefreshTokensAsync(user.Id);

            // Save refresh token to database
            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiresAt = vietnamNow.AddDays(refreshTokenExpiryDays),
                CreatedAt = vietnamNow
            };
            await _userRepository.AddRefreshTokenAsync(refreshTokenEntity);

            // Log successful registration
            _logger.LogInformation("Đăng ký tài khoản thành công - User ID: {UserId}, Email: {Email}", user.Id, user.Email ?? "");

            return new LoginResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                User = _mapper.Map<UserDto>(user),
                ExpiresAt = vietnamNow.AddMinutes(expiryMinutes),
                RefreshTokenExpiresAt = vietnamNow.AddDays(refreshTokenExpiryDays)
            };
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            // Tìm user theo email với UserRoles
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null || user.IsDeleted || user.Status != UserStatus.Active)
            {
                _logger.LogWarning("Đăng nhập thất bại - Email: {Email}, Lý do: User không tồn tại hoặc bị vô hiệu hóa", request.Email);
                return null;
            }

            // Validate password với BCrypt
            if (!await ValidatePasswordAsync(request.Password, user.Password ?? ""))
            {
                _logger.LogWarning("Đăng nhập thất bại - Email: {Email}, Lý do: Mật khẩu không đúng", request.Email);
                return null;
            }
            
            // Reload user with UserRoles for JWT token generation
            user = await _userRepository.GetByIdWithRolesAsync(user.Id);
            if (user == null)
                return null;
           
            await _userRepository.UpdateAsync(user);

            // Generate JWT token and refresh token
            var token = _jwtHelper.GenerateToken(user);
            var refreshToken = GenerateRefreshToken();
            var expiryMinutes = int.Parse(_configuration["JwtSettings:ExpiryMinutes"] ?? "60");
            var refreshTokenExpiryDays = int.Parse(_configuration["JwtSettings:RefreshTokenExpiryDays"] ?? "7");
            var vietnamNow = TimeZoneHelper.VietnamNow;

            // Revoke all existing refresh tokens for this user (ensure only 1 active token)
            await _userRepository.RevokeAllUserRefreshTokensAsync(user.Id);

            // Save refresh token to database
            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiresAt = vietnamNow.AddDays(refreshTokenExpiryDays),
                CreatedAt = vietnamNow
            };
            await _userRepository.AddRefreshTokenAsync(refreshTokenEntity);

            // Log successful login
            _logger.LogInformation("Đăng nhập thành công - User ID: {UserId}, Email: {Email}", user.Id, user.Email ?? "");

            return new LoginResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                User = _mapper.Map<UserDto>(user),
                ExpiresAt = vietnamNow.AddMinutes(expiryMinutes),
                RefreshTokenExpiresAt = vietnamNow.AddDays(refreshTokenExpiryDays)
            };
        }

        public async Task<UserDto?> GetCurrentUserAsync(int userId)
        {
            var user = await _userRepository.GetByIdWithRolesAsync(userId);
            return user == null ? null : _mapper.Map<UserDto>(user);
        }

        public async Task<RefreshTokenResponse?> RefreshTokenAsync(RefreshTokenRequest request)
        {
            // Validate refresh token
            var refreshTokenEntity = await _userRepository.GetRefreshTokenAsync(request.RefreshToken);
            
            if (refreshTokenEntity == null)
            {
                var tokenPreview = request.RefreshToken.Length > 20 ? request.RefreshToken.Substring(0, 20) + "..." : request.RefreshToken;
                _logger.LogWarning("Refresh token thất bại - Lý do: Token không tồn tại - Token: {TokenPreview}", tokenPreview);
                return null;
            }

            var vietnamNow = TimeZoneHelper.VietnamNow;

            // Check if token is revoked
            if (refreshTokenEntity.IsRevoked)
            {
                var tokenPreview = request.RefreshToken.Length > 20 ? request.RefreshToken.Substring(0, 20) + "..." : request.RefreshToken;
                _logger.LogWarning("Refresh token thất bại - Lý do: Token đã bị thu hồi - Token: {TokenPreview}", tokenPreview);
                return null;
            }

            // Check if token is expired
            if (refreshTokenEntity.ExpiresAt < vietnamNow)
            {
                var tokenPreview = request.RefreshToken.Length > 20 ? request.RefreshToken.Substring(0, 20) + "..." : request.RefreshToken;
                _logger.LogWarning("Refresh token thất bại - Lý do: Token đã hết hạn - Token: {TokenPreview}", tokenPreview);
                return null;
            }

            var user = refreshTokenEntity.User;
            if (user == null || user.IsDeleted || user.Status != UserStatus.Active)
            {
                var tokenPreview = request.RefreshToken.Length > 20 ? request.RefreshToken.Substring(0, 20) + "..." : request.RefreshToken;
                _logger.LogWarning("Refresh token thất bại - Lý do: User không hợp lệ - Token: {TokenPreview}", tokenPreview);
                return null;
            }

            // Revoke old refresh token (token rotation)
            await _userRepository.RevokeRefreshTokenAsync(request.RefreshToken);

            // Generate new access token and refresh token
            var newAccessToken = _jwtHelper.GenerateToken(user);
            var newRefreshToken = GenerateRefreshToken();
            var expiryMinutes = int.Parse(_configuration["JwtSettings:ExpiryMinutes"] ?? "60");
            var refreshTokenExpiryDays = int.Parse(_configuration["JwtSettings:RefreshTokenExpiryDays"] ?? "7");

            // Save new refresh token to database
            var newRefreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = newRefreshToken,
                ExpiresAt = vietnamNow.AddDays(refreshTokenExpiryDays),
                CreatedAt = vietnamNow
            };
            await _userRepository.AddRefreshTokenAsync(newRefreshTokenEntity);

            // Log successful refresh
            _logger.LogInformation("Refresh token thành công - User ID: {UserId}, Email: {Email}", user.Id, user.Email ?? "");

            return new RefreshTokenResponse
            {
                Token = newAccessToken,
                RefreshToken = newRefreshToken,
                User = _mapper.Map<UserDto>(user),
                ExpiresAt = vietnamNow.AddMinutes(expiryMinutes),
                RefreshTokenExpiresAt = vietnamNow.AddDays(refreshTokenExpiryDays)
            };
        }

        /// <summary>
        /// Generate secure refresh token using cryptographically strong random bytes
        /// </summary>
        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[128];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        public Task<bool> ValidatePasswordAsync(string password, string hashedPassword)
        {
            try
            {
                // Sử dụng BCrypt để verify password
                return Task.FromResult(BCrypt.Net.BCrypt.Verify(password, hashedPassword));
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public string HashPassword(string password)
        {
            // Sử dụng BCrypt để hash password
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                // Sử dụng BCrypt để verify password
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch
            {
                return false;
            }
        }
    }
}
