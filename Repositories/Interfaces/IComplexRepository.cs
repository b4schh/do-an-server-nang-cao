using FootballField.API.Entities;

namespace FootballField.API.Repositories.Interfaces
{
    public interface IComplexRepository : IGenericRepository<Complex>
    {
        Task<IEnumerable<Complex>> GetByOwnerIdAsync(int ownerId);
        Task<IEnumerable<Complex>> GetActiveComplexesAsync();
        Task<Complex?> GetComplexWithFieldsAsync(int complexId);
<<<<<<< HEAD
        Task<Complex?> GetComplexWithFullDetailsAsync(int complexId);
=======
>>>>>>> origin/Vu
    }
}
