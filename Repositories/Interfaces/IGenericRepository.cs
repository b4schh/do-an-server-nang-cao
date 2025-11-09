using System.Linq.Expressions;

namespace FootballField.API.Repositories.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        // Query methods
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> filter);
        Task<(IEnumerable<T> items, int totalCount)> GetPagedAsync(int pageIndex, int pageSize, Expression<Func<T, bool>>? filter = null);
        Task<T?> GetByIdAsync(int id);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> filter);

        // Command methods
        Task<T> AddAsync(T entity);
        Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);
        Task UpdateAsync(T entity);
        Task UpdateRangeAsync(IEnumerable<T> entities); 
        Task DeleteAsync(T entity);
        Task DeleteRangeAsync(IEnumerable<T> entities);

        // Soft delete
        Task SoftDeleteAsync(int id);

        // Count
        Task<int> CountAsync(Expression<Func<T, bool>>? filter = null);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> filter);
    }
}
