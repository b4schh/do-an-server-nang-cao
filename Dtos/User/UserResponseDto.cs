using System.ComponentModel.DataAnnotations;
using FootballField.API.Entities;



namespace FootballField.API.Dtos.User  {
public class UserResponseDto
{

    // Các thuộc tính
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; 
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? AvatarUrl { get; set; } 
}

}