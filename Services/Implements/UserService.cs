using AutoMapper;
using FootballField.API.Entities;
using FootballField.API.Dtos.User;
using FootballField.API.Repositories.Interfaces;
using FootballField.API.Services.Interfaces;

namespace FootballField.API.Services.Implements
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

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
            user.CreatedAt = DateTime.Now;
            user.UpdatedAt = DateTime.Now;
            
            var created = await _userRepository.AddAsync(user);
            return _mapper.Map<UserDto>(created);
        }

        public async Task UpdateUserAsync(int id, UpdateUserDto updateUserDto)
        {
            var existingUser = await _userRepository.GetByIdAsync(id);
            if (existingUser == null)
                throw new Exception("User not found");

            _mapper.Map(updateUserDto, existingUser);
            existingUser.UpdatedAt = DateTime.Now;
            
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
    }
}