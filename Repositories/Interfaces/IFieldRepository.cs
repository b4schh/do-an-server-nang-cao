using FootballField.API.Entities;

namespace FootballField.API.Repositories.Interfaces
{
    public interface IFieldRepository : IGenericRepository<Field>
    {
        Task<IEnumerable<Field>> GetByComplexIdAsync(int complexId);
        Task<IEnumerable<Field>> GetActiveFieldsAsync();
        Task<Field?> GetFieldWithTimeSlotsAsync(int fieldId);
        Task<Field?> GetFieldWithComplexAsync(int fieldId);
    }
}
