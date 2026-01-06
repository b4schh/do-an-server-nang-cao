using DoAn.Core.Application.Interfaces.Base;

namespace DoAn.Core.Application.Interfaces.Review;

public interface IReviewHelpfulVoteRepository : IGenericRepository<ReviewHelpfulVote>
{
    Task<ReviewHelpfulVote?> GetVoteAsync(int reviewId, int userId);
    Task<bool> HasVotedAsync(int reviewId, int userId);
    Task<int> GetVoteCountAsync(int reviewId);
}
