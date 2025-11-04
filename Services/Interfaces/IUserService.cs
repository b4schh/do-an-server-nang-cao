using FootballField.API.Dtos.User;

namespace FootballField.API.Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<(IEnumerable<UserDto> users, int totalCount)> GetPagedUsersAsync(int pageIndex, int pageSize);
        Task<UserDto?> GetUserByIdAsync(int id);
        Task<UserDto?> GetUserByEmailAsync(string email);
        Task<UserDto> CreateUserAsync(CreateUserDto createUserDto);
        Task UpdateUserAsync(int id, UpdateUserDto updateUserDto);
        Task SoftDeleteUserAsync(int id);
        Task<bool> EmailExistsAsync(string email);
    }
}