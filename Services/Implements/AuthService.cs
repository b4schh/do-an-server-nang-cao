using AutoMapper;
using FootballField.API.Entities;
using FootballField.API.Dtos.User;
using FootballField.API.Repositories.Interfaces;
using FootballField.API.Services.Interfaces;
using FootballField.API.Utils;
using FootballField.API.Dtos.Auth;
using BCrypt.Net;

namespace FootballField.API.Services.Implements
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtHelper _jwtHelper;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public AuthService(IUserRepository userRepository, JwtHelper jwtHelper, IConfiguration configuration, IMapper mapper)
        {
            _userRepository = userRepository;
            _jwtHelper = jwtHelper;
            _configuration = configuration;
            _mapper = mapper;
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
                Role = UserRole.Customer,
                Status = UserStatus.Active,              
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now               
            };

            // Lưu user vào database
            await _userRepository.AddAsync(user);

            // Generate JWT token
            var token = _jwtHelper.GenerateToken(user);
            var expiryMinutes = int.Parse(_configuration["JwtSettings:ExpiryMinutes"] ?? "60");

            return new LoginResponse
            {
                Token = token,
                User = _mapper.Map<UserDto>(user),
                ExpiresAt = DateTime.Now.AddMinutes(expiryMinutes)
            };
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            // Tìm user theo email
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null || user.IsDeleted || user.Status != UserStatus.Active)
                return null;

            // Validate password với BCrypt
            if (!await ValidatePasswordAsync(request.Password, user.Password ?? ""))
                return null;

            
           
            await _userRepository.UpdateAsync(user);

            // Generate JWT token
            var token = _jwtHelper.GenerateToken(user);
            var expiryMinutes = int.Parse(_configuration["JwtSettings:ExpiryMinutes"] ?? "60");

            return new LoginResponse
            {
                Token = token,
                User = _mapper.Map<UserDto>(user),
                ExpiresAt = DateTime.Now.AddMinutes(expiryMinutes)
            };
        }

        public async Task<UserDto?> GetCurrentUserAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            return user == null ? null : _mapper.Map<UserDto>(user);
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
    }
}
