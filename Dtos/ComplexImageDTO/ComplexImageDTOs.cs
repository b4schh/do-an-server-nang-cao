using System;

namespace FootballField.API.Dtos;

public class ComplexImageDto
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }

    // FK
    public int ComplexId { get; set; }
}

public class ComplexImageCreateDto
{
    public int ComplexId { get; set; }
    public string ImageUrl { get; set; } = null!;

    public bool IsMain { get; set; }
    public string? Description { get; set; }
}

public class ComplexImageResponseDto
{
    public long Id { get; set; }
    public int ComplexId { get; set; }
    public string ImageUrl { get; set; } = null!;

    public bool IsMain { get; set; }
}
