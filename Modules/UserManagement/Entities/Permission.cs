namespace FootballField.API.Modules.UserManagement.Entities;

public class Permission
{
    public int Id { get; set; }
    public string PermissionKey { get; set; } = null!;
    public string? Description { get; set; }
    public string? Module { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
