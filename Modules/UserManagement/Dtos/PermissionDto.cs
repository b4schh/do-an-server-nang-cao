namespace FootballField.API.Modules.UserManagement.Dtos;

public class PermissionDto
{
    public int Id { get; set; }
    public string PermissionKey { get; set; } = null!;
    public string? Description { get; set; }
    public string? Module { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreatePermissionDto
{
    public string PermissionKey { get; set; } = null!;
    public string? Description { get; set; }
    public string? Module { get; set; }
}

public class UpdatePermissionDto
{
    public string? PermissionKey { get; set; }
    public string? Description { get; set; }
    public string? Module { get; set; }
}

public class PermissionsByModuleDto
{
    public string Module { get; set; } = null!;
    public List<PermissionDto> Permissions { get; set; } = new();
}
