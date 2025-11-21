

using FootballField.API.Modules.UserManagement.Entities;

namespace FootballField.API.Modules.UserManagement.Dtos
{
    public class UserProfileDto
    {
        public int Id { get; set; }
        public string LastName { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public UserRole Role { get; set; }
        public string? AvatarUrl { get; set; }
        public UserStatus Status { get; set; }
        public bool EmailVerified { get; set; }
    }
}
