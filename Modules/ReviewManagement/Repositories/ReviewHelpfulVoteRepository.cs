using FootballField.API.Database;
using FootballField.API.Modules.ReviewManagement.Entities;
using FootballField.API.Shared.Base;
using Microsoft.EntityFrameworkCore;

namespace FootballField.API.Modules.ReviewManagement.Repositories;

public class ReviewHelpfulVoteRepository : GenericRepository<ReviewHelpfulVote>, IReviewHelpfulVoteRepository
{
    public ReviewHelpfulVoteRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<ReviewHelpfulVote?> GetVoteAsync(int reviewId, int userId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(v => v.ReviewId == reviewId && v.UserId == userId);
    }

    public async Task<bool> HasVotedAsync(int reviewId, int userId)
    {
        return await _dbSet
            .AnyAsync(v => v.ReviewId == reviewId && v.UserId == userId);
    }

    public async Task<int> GetVoteCountAsync(int reviewId)
    {
        return await _dbSet
            .CountAsync(v => v.ReviewId == reviewId);
    }
}
