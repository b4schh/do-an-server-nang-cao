using AutoMapper;
using FootballField.API.Entities;
using FootballField.API.Dtos.User;
using FootballField.API.Repositories.Interfaces;
using FootballField.API.Services.Interfaces;
using FootballField.API.Utils;

namespace FootballField.API.Services.Implements
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        private readonly IAuthService _authService;

        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync(u => !u.IsDeleted);
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<(IEnumerable<UserDto> users, int totalCount)> GetPagedUsersAsync(int pageIndex, int pageSize)
        {
            var (users, totalCount) = await _userRepository.GetPagedAsync(pageIndex, pageSize, u => !u.IsDeleted);
            var userDtos = _mapper.Map<IEnumerable<UserDto>>(users);
            return (userDtos, totalCount);
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user == null ? null : _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto?> GetUserByEmailAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            return user == null ? null : _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
        {
            var user = _mapper.Map<User>(createUserDto);
            // CreatedAt và UpdatedAt sẽ được set bởi ApplicationDbContext.UpdateTimestamps()
            // Không cần set thủ công nữa

            var created = await _userRepository.AddAsync(user);
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
        }

        public async Task UpdateUserRoleAsync(int id, UpdateUserRoleDto updateUserRoleDto)
        {
            var existingUser = await _userRepository.GetByIdAsync(id);
            if (existingUser == null)
                throw new Exception("User not found");

            existingUser.Role = updateUserRoleDto.Role;
            // UpdatedAt sẽ được set bởi ApplicationDbContext.UpdateTimestamps()

            await _userRepository.UpdateAsync(existingUser);
        }

        public async Task SoftDeleteUserAsync(int id)
        {
            await _userRepository.SoftDeleteAsync(id);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _userRepository.EmailExistsAsync(email);
        }

        
        public async Task<UserResponseDto> UpdateAvatarAsync(int userId, string? avatarUrl)
        {
            var user = await _userRepository.FindByIdAsync(userId);
            if (user == null) throw new Exception("User not found");

            user.AvatarUrl = avatarUrl;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.SaveChangesAsync();

            return _mapper.Map<UserResponseDto>(user);
        }


        public async Task<UserResponseDto> UpdateUserProfileAsync(int id, UpdateUserProfileDto dto)
{
    // Lấy user từ DB (dùng FindByIdAsync để tránh filter IsDeleted nếu cần)
    var user = await _userRepository.FindByIdAsync(id);
    if (user == null || user.IsDeleted)
        throw new Exception("User not found");

    // Nếu client gửi phone khác, kiểm tra số điện thoại đã được dùng bởi user khác chưa
    if (!string.IsNullOrEmpty(dto.Phone))
    {
        var existingWithPhone = await _userRepository.GetByPhoneAsync(dto.Phone);
        if (existingWithPhone != null && existingWithPhone.Id != id)
            throw new Exception("Số điện thoại đã được sử dụng bởi người khác");
    }

    // Cập nhật có chủ đích: chỉ FullName và Phone
    if (dto.LastName != null  && dto.FirstName != null)  {

        user.LastName = dto.LastName.Trim();
        user.FirstName = dto.FirstName.Trim();

    }

    if (dto.Phone != null)
        user.Phone = dto.Phone.Trim();

    user.UpdatedAt = DateTime.UtcNow;

    // Lưu thay đổi. (UserRepository có SaveChangesAsync)
    await _userRepository.SaveChangesAsync();

    // Trả về DTO response (nếu bạn đã có UserResponseDto)
    return _mapper.Map<UserResponseDto>(user);
}


 public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _userRepository.FindByIdAsync(userId);
            if (user == null) throw new Exception("User not found");

            // Kiểm tra mật khẩu hiện tại bằng VerifyPassword từ AuthService
            if (!_authService.VerifyPassword(currentPassword, user.Password))
            {
                return false; // Mật khẩu hiện tại không đúng
            }

            // Hash mật khẩu mới trước khi lưu
            user.Password = _authService.HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.SaveChangesAsync();
            return true;
        }
    }
}