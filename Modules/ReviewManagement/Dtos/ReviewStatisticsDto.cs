namespace FootballField.API.Modules.ReviewManagement.Dtos;

public class ReviewStatisticsDto
{
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public Dictionary<int, int> RatingCounts { get; set; } = new Dictionary<int, int>
    {
        { 5, 0 },
        { 4, 0 },
        { 3, 0 },
        { 2, 0 },
        { 1, 0 }
    };
}
