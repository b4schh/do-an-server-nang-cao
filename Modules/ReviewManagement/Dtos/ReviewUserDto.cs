namespace FootballField.API.Modules.ReviewManagement.Dtos;

public class ReviewUserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Avatar { get; set; }
    public string Role { get; set; } = null!; // "Khách hàng thường xuyên" hoặc "Khách hàng mới"
}
