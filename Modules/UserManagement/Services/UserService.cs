using AutoMapper;
using FootballField.API.Modules.AuthManagement.Services;
using FootballField.API.Modules.UserManagement.Dtos;
using FootballField.API.Modules.UserManagement.Entities;
using FootballField.API.Modules.UserManagement.Repositories;
using FootballField.API.Shared.Storage;

namespace FootballField.API.Modules.UserManagement.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IAuthService _authService;
        private readonly IStorageService _storageService;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IUserRepository userRepository, 
            IMapper mapper, 
            IAuthService authService,
            IStorageService storageService,
            ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _authService = authService;
            _storageService = storageService;
            _logger = logger;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllUsersWithRolesAsync();
            var userDtos = _mapper.Map<IEnumerable<UserDto>>(users);
            
            // Map AvatarUrl to full URL
            foreach (var dto in userDtos)
            {
                if (!string.IsNullOrEmpty(dto.AvatarUrl))
                {
                    dto.AvatarUrl = _storageService.GetFullUrl(dto.AvatarUrl);
                }
            }
            
            return userDtos;
        }

        public async Task<(IEnumerable<UserDto> users, int totalCount)> GetPagedUsersAsync(int pageIndex, int pageSize)
        {
            var (users, totalCount) = await _userRepository.GetPagedAsync(pageIndex, pageSize, u => !u.IsDeleted);
            var userDtos = _mapper.Map<IEnumerable<UserDto>>(users).ToList();
            
            // Map AvatarUrl to full URL
            foreach (var dto in userDtos)
            {
                if (!string.IsNullOrEmpty(dto.AvatarUrl))
                {
                    dto.AvatarUrl = _storageService.GetFullUrl(dto.AvatarUrl);
                }
            }
            
            return (userDtos, totalCount);
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdWithRolesAsync(id);
            if (user == null) return null;
            
            var dto = _mapper.Map<UserDto>(user);
            
            // Map AvatarUrl to full URL
            if (!string.IsNullOrEmpty(dto.AvatarUrl))
            {
                dto.AvatarUrl = _storageService.GetFullUrl(dto.AvatarUrl);
            }
            
            return dto;
        }

        public async Task<UserDto?> GetUserByEmailAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null) return null;
            
            var dto = _mapper.Map<UserDto>(user);
            
            // Map AvatarUrl to full URL
            if (!string.IsNullOrEmpty(dto.AvatarUrl))
            {
                dto.AvatarUrl = _storageService.GetFullUrl(dto.AvatarUrl);
            }
            
            return dto;
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
        {
            var user = _mapper.Map<User>(createUserDto);
            // CreatedAt và UpdatedAt sẽ được set bởi ApplicationDbContext.UpdateTimestamps()
            // Không cần set thủ công nữa

            var created = await _userRepository.AddAsync(user);
            
            _logger.LogInformation("Tạo user mới thành công - User ID: {UserId}, Email: {Email}, LastName: {LastName}, FirstName: {FirstName}",
                created.Id, created.Email, created.LastName, created.FirstName);
            
            return _mapper.Map<UserDto>(created);
        }

        public async Task UpdateUserAsync(int id, UpdateUserDto updateUserDto)
        {
            var existingUser = await _userRepository.GetByIdAsync(id);
            if (existingUser == null)
                throw new Exception("User not found");

            _mapper.Map(updateUserDto, existingUser);
            // UpdatedAt sẽ được set bởi ApplicationDbContext.UpdateTimestamps()

            await _userRepository.UpdateAsync(existingUser);
            
            _logger.LogInformation("Cập nhật thông tin user - User ID: {UserId}, Email: {Email}",
                id, existingUser.Email);
        }

        public async Task UpdateUserRoleAsync(int id, UpdateUserRoleDto updateUserRoleDto)
        {
            var existingUser = await _userRepository.GetByIdAsync(id);
            if (existingUser == null)
                throw new Exception("User not found");

            // Remove old roles
            await _userRepository.RemoveUserRolesAsync(id);
            
            // Add new role
            await _userRepository.AddUserRoleAsync(id, updateUserRoleDto.RoleId);
            
            _logger.LogInformation("Cập nhật role cho user - User ID: {UserId}, New Role ID: {RoleId}",
                id, updateUserRoleDto.RoleId);
        }

        public async Task SoftDeleteUserAsync(int id)
        {
            await _userRepository.SoftDeleteAsync(id);
            
            _logger.LogWarning("Xóa mềm user - User ID: {UserId}", id);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _userRepository.EmailExistsAsync(email);
        }

        
        public async Task<UserResponseDto> UpdateAvatarAsync(int userId, string? avatarUrl)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new Exception("User not found");

            user.AvatarUrl = avatarUrl;
            // UpdatedAt sẽ được set bởi ApplicationDbContext.UpdateTimestamps()

            await _userRepository.UpdateAsync(user);
            
            _logger.LogInformation("Cập nhật avatar - User ID: {UserId}, Avatar URL: {AvatarUrl}",
                userId, avatarUrl ?? "(removed)");

            var dto = _mapper.Map<UserResponseDto>(user);
            
            // Map AvatarUrl to full URL
            if (!string.IsNullOrEmpty(dto.AvatarUrl))
            {
                dto.AvatarUrl = _storageService.GetFullUrl(dto.AvatarUrl);
            }
            
            return dto;
        }

        public async Task<UserResponseDto> UpdateUserProfileAsync(int id, UpdateUserProfileDto dto)
        {
            // Lấy user từ DB
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null || user.IsDeleted)
                throw new Exception("Không tìm thấy người dùng");

            // Nếu client gửi phone khác, kiểm tra số điện thoại đã được dùng bởi user khác chưa
            if (!string.IsNullOrEmpty(dto.Phone) && dto.Phone != user.Phone)
            {
                var existingWithPhone = await _userRepository.GetByPhoneAsync(dto.Phone);
                if (existingWithPhone != null && existingWithPhone.Id != id)
                    throw new Exception("Số điện thoại đã được sử dụng bởi người khác");
            }

            // Cập nhật thông tin
            if (!string.IsNullOrWhiteSpace(dto.FirstName))
                user.FirstName = dto.FirstName.Trim();

            if (!string.IsNullOrWhiteSpace(dto.LastName))
                user.LastName = dto.LastName.Trim();

            if (!string.IsNullOrWhiteSpace(dto.Phone))
                user.Phone = dto.Phone.Trim();

            // UpdatedAt sẽ được set bởi ApplicationDbContext.UpdateTimestamps()

            await _userRepository.UpdateAsync(user);

            var responseDto = _mapper.Map<UserResponseDto>(user);
            
            // Map AvatarUrl to full URL
            if (!string.IsNullOrEmpty(responseDto.AvatarUrl))
            {
                responseDto.AvatarUrl = _storageService.GetFullUrl(responseDto.AvatarUrl);
            }
            
            return responseDto;
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || user.IsDeleted)
                throw new Exception("Không tìm thấy người dùng");

            // Kiểm tra mật khẩu mới không được trùng với mật khẩu cũ
            if (currentPassword == newPassword)
                throw new Exception("Mật khẩu mới không được trùng với mật khẩu hiện tại");

            // Kiểm tra mật khẩu hiện tại
            if (!_authService.VerifyPassword(currentPassword, user.Password))
            {
                return false;
            }

            // Hash mật khẩu mới và lưu
            user.Password = _authService.HashPassword(newPassword);
            // UpdatedAt sẽ được set bởi ApplicationDbContext.UpdateTimestamps()

            await _userRepository.UpdateAsync(user);
            return true;
        }

        public async Task UpdateUserStatusAsync(int userId, byte status)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || user.IsDeleted)
                throw new Exception("Không tìm thấy người dùng");

            user.Status = (UserStatus)status;
            await _userRepository.UpdateAsync(user);
        }

        public async Task<Dictionary<string, int>> GetUserStatisticsByRoleAsync()
        {
            var users = await _userRepository.GetAllUsersWithRolesAsync();
            var stats = users
                .SelectMany(u => u.UserRoles)
                .GroupBy(ur => ur.Role.Name)
                .ToDictionary(g => g.Key, g => g.Count());
            
            return stats;
        }
    }
}