

using FootballField.API.Modules.UserManagement.Entities;

namespace FootballField.API.Modules.UserManagement.Dtos
{
    public class UserDto
    {
        public int Id { get; set; }
        public string LastName { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public List<string> RoleNames { get; set; } = new();
        public string? AvatarUrl { get; set; }
        public UserStatus Status { get; set; }
        public bool EmailVerified { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
