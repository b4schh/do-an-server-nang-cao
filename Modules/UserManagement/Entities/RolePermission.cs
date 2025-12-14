namespace FootballField.API.Modules.UserManagement.Entities;

public class RolePermission
{
    public int RoleId { get; set; }
    public int PermissionId { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public Role Role { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}
