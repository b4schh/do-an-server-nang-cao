using DoAn.Core.Application.Interfaces.Review;
using DoAn.Infrastructure.Data;
using DoAn.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Infrastructure.Repositories.Review;

public class ReviewHelpfulVoteRepository : GenericRepository<ReviewHelpfulVoteEntity>, IReviewHelpfulVoteRepository
{
    public ReviewHelpfulVoteRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<ReviewHelpfulVoteEntity?> GetVoteAsync(int reviewId, int userId)
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
