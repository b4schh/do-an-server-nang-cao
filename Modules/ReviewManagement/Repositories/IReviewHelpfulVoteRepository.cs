using FootballField.API.Shared.Base;
using FootballField.API.Modules.ReviewManagement.Entities;

namespace FootballField.API.Modules.ReviewManagement.Repositories;

public interface IReviewHelpfulVoteRepository : IGenericRepository<ReviewHelpfulVote>
{
    Task<ReviewHelpfulVote?> GetVoteAsync(int reviewId, int userId);
    Task<bool> HasVotedAsync(int reviewId, int userId);
    Task<int> GetVoteCountAsync(int reviewId);
}
