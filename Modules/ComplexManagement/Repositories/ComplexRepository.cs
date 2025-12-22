using FootballField.API.Database;
using FootballField.API.Modules.ComplexManagement.Entities;
using FootballField.API.Shared.Base;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FootballField.API.Modules.ComplexManagement.Repositories
{
    public class ComplexRepository : GenericRepository<Complex>, IComplexRepository
    {
        public ComplexRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<(IEnumerable<Complex> items, int totalCount)> GetPagedAsync(
            int pageIndex,
            int pageSize,
            Expression<Func<Complex, bool>>? filter = null)
        {
            // Load complexes
            var complexes = await _dbSet
                .Include(c => c.ComplexImages)
                .Where(c => !c.IsDeleted)
                .ToListAsync();

            // Lọc theo bank info - chỉ hiển thị complex có bank account
            var ownerIds = complexes.Select(c => c.OwnerId).Distinct().ToList();
            var ownerIdsWithBank = await _context.OwnerSettings
                .Where(os => ownerIds.Contains(os.OwnerId) && !string.IsNullOrEmpty(os.BankAccountNumber))
                .Select(os => os.OwnerId)
                .ToListAsync();

            var filteredComplexes = complexes
                .Where(c => c.Status == ComplexStatus.Approved && c.IsActive && ownerIdsWithBank.Contains(c.OwnerId))
                .AsQueryable();

            // Apply custom filter nếu có
            if (filter != null)
                filteredComplexes = filteredComplexes.Where(filter);

            var totalCount = filteredComplexes.Count();
            var items = filteredComplexes
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (items, totalCount);
        }

        public async Task<IEnumerable<Complex>> GetByOwnerIdAsync(int ownerId)
        {
            return await _dbSet
                .Where(c => c.OwnerId == ownerId && !c.IsDeleted)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Complex> complexes, int totalCount)> GetByOwnerIdPagedAsync(int ownerId, int pageIndex, int pageSize)
        {
            var query = _dbSet
                .Where(c => c.OwnerId == ownerId && !c.IsDeleted);

            var totalCount = await query.CountAsync();
            var complexes = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (complexes, totalCount);
        }

        public async Task<IEnumerable<Complex>> GetActiveComplexesAsync()
        {
            return await _dbSet
                .Where(c => c.IsActive && !c.IsDeleted && c.Status == ComplexStatus.Approved)
                .ToListAsync();
        }

        public async Task<Complex?> GetComplexWithFieldsAsync(int complexId)
        {
            var complex = await _dbSet
                .Include(c => c.Fields)
                .Include(c => c.ComplexImages)
                .FirstOrDefaultAsync(c => c.Id == complexId && !c.IsDeleted);

            if (complex == null)
                return null;

            // Kiểm tra bank info
            var hasBankInfo = await _context.OwnerSettings
                .AnyAsync(os => os.OwnerId == complex.OwnerId && !string.IsNullOrEmpty(os.BankAccountNumber));

            // Chỉ trả về nếu complex approved, active và có bank info
            if (complex.Status == ComplexStatus.Approved && complex.IsActive && hasBankInfo)
                return complex;

            return null;
        }

        public async Task<Complex?> GetComplexWithFullDetailsAsync(int complexId)
        {
            var complex = await _dbSet
                .Include(c => c.Fields.Where(f => !f.IsDeleted && f.IsActive))
                    .ThenInclude(f => f.TimeSlots.Where(ts => ts.IsActive))
                .Include(c => c.ComplexImages)
                .FirstOrDefaultAsync(c => c.Id == complexId && !c.IsDeleted);

            if (complex == null)
                return null;

            // Kiểm tra bank info
            var hasBankInfo = await _context.OwnerSettings
                .AnyAsync(os => os.OwnerId == complex.OwnerId && !string.IsNullOrEmpty(os.BankAccountNumber));

            // Chỉ trả về nếu complex approved, active và có bank info
            if (complex.Status == ComplexStatus.Approved && complex.IsActive && hasBankInfo)
                return complex;

            return null;
        }

        public async Task<IEnumerable<(Complex Complex, bool HasBankInfo)>> GetComplexesWithDetailsForSearchAsync()
        {
            // Load complexes với details
            var complexes = await _dbSet
                .Include(c => c.ComplexImages)
                .Include(c => c.Fields.Where(f => !f.IsDeleted))
                    .ThenInclude(f => f.TimeSlots.Where(ts => ts.IsActive))
                .Include(c => c.Fields)
                    .ThenInclude(f => f.Bookings)
                        .ThenInclude(b => b.Reviews.Where(r => !r.IsDeleted && r.IsVisible))
                .Where(c => !c.IsDeleted)
                .ToListAsync();

            // Load owner settings
            var ownerIds = complexes.Select(c => c.OwnerId).Distinct().ToList();
            var ownerSettings = await _context.OwnerSettings
                .Where(os => ownerIds.Contains(os.OwnerId) && !string.IsNullOrEmpty(os.BankAccountNumber))
                .Select(os => os.OwnerId)
                .ToListAsync();

            // Map kết quả
            return complexes.Select(c => (c, ownerSettings.Contains(c.OwnerId)));
        }
    }
}
