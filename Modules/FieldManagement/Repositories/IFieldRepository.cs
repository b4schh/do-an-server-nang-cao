using FootballField.API.Shared.Base;
using FootballField.API.Modules.FieldManagement.Entities;

namespace FootballField.API.Modules.FieldManagement.Repositories
{
    public interface IFieldRepository : IGenericRepository<Field>
    {
        Task<IEnumerable<Field>> GetByComplexIdAsync(int complexId);
        Task<IEnumerable<Field>> GetActiveFieldsAsync();
        Task<Field?> GetFieldWithTimeSlotsAsync(int fieldId);
        Task<Field?> GetFieldWithComplexAsync(int fieldId);
    }
}
