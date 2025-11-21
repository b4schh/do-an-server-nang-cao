using System.Linq.Expressions;
using FootballField.API.Database;
using FootballField.API.Shared.Utils;
using Microsoft.EntityFrameworkCore;

namespace FootballField.API.Shared.Base
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        // Query methods
        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> filter)
        {
            return await _dbSet.Where(filter).ToListAsync();
        }

        public virtual async Task<(IEnumerable<T> items, int totalCount)> GetPagedAsync(
            int pageIndex, 
            int pageSize, 
            Expression<Func<T, bool>>? filter = null)
        {
            IQueryable<T> query = _dbSet;

            if (filter != null)
                query = query.Where(filter);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> filter)
        {
            return await _dbSet.FirstOrDefaultAsync(filter);
        }

        // Command methods
        public virtual async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public virtual async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
            return entities;
        }

        public virtual async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task UpdateRangeAsync(IEnumerable<T> entities)
        {
            _dbSet.UpdateRange(entities);
            await _context.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task DeleteRangeAsync(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
            await _context.SaveChangesAsync();
        }

        // Soft delete
        public virtual async Task SoftDeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null) return;

            var isDeletedProperty = entity.GetType().GetProperty("IsDeleted");
            var deletedAtProperty = entity.GetType().GetProperty("DeletedAt");

            if (isDeletedProperty != null)
                isDeletedProperty.SetValue(entity, true);

            if (deletedAtProperty != null)
                deletedAtProperty.SetValue(entity, TimeZoneHelper.VietnamNow);

            await UpdateAsync(entity);
        }

        // Count
        public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? filter = null)
        {
            if (filter == null)
                return await _dbSet.CountAsync();

            return await _dbSet.CountAsync(filter);
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> filter)
        {
            return await _dbSet.AnyAsync(filter);
        }
        
    }
}
