using AutoMapper;
using FootballField.API.Entities;
using FootballField.API.Dtos.Field;
using FootballField.API.Repositories.Interfaces;
using FootballField.API.Services.Interfaces;
using FootballField.API.Utils;

namespace FootballField.API.Services.Implements
{
    public class FieldService : IFieldService
    {
        private readonly IFieldRepository _fieldRepository;
        private readonly IMapper _mapper;

        public FieldService(IFieldRepository fieldRepository, IMapper mapper)
        {
            _fieldRepository = fieldRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<FieldDto>> GetAllFieldsAsync()
        {
            var fields = await _fieldRepository.GetAllAsync(f => !f.IsDeleted);
            return _mapper.Map<IEnumerable<FieldDto>>(fields);
        }

        public async Task<(IEnumerable<FieldDto> fields, int totalCount)> GetPagedFieldsAsync(int pageIndex, int pageSize)
        {
            var (fields, totalCount) = await _fieldRepository.GetPagedAsync(pageIndex, pageSize, f => !f.IsDeleted);
            var fieldDtos = _mapper.Map<IEnumerable<FieldDto>>(fields);
            return (fieldDtos, totalCount);
        }

        public async Task<FieldDto?> GetFieldByIdAsync(int id)
        {
            var field = await _fieldRepository.GetByIdAsync(id);
            return field == null ? null : _mapper.Map<FieldDto>(field);
        }

        public async Task<FieldWithTimeSlotsDto?> GetFieldWithTimeSlotsAsync(int id)
        {
            var field = await _fieldRepository.GetFieldWithTimeSlotsAsync(id);
            return field == null ? null : _mapper.Map<FieldWithTimeSlotsDto>(field);
        }

        public async Task<IEnumerable<FieldDto>> GetFieldsByComplexIdAsync(int complexId)
        {
            var fields = await _fieldRepository.GetByComplexIdAsync(complexId);
            return _mapper.Map<IEnumerable<FieldDto>>(fields);
        }

        public async Task<FieldDto> CreateFieldAsync(CreateFieldDto createFieldDto)
        {
            var field = _mapper.Map<Field>(createFieldDto);
            // CreatedAt và UpdatedAt sẽ được set bởi ApplicationDbContext.UpdateTimestamps()
            
            var created = await _fieldRepository.AddAsync(field);
            return _mapper.Map<FieldDto>(created);
        }

        public async Task UpdateFieldAsync(int id, UpdateFieldDto updateFieldDto)
        {
            var existingField = await _fieldRepository.GetByIdAsync(id);
            if (existingField == null)
                throw new Exception("Field not found");

            _mapper.Map(updateFieldDto, existingField);
            // UpdatedAt sẽ được set bởi ApplicationDbContext.UpdateTimestamps()
            
            await _fieldRepository.UpdateAsync(existingField);
        }

        public async Task SoftDeleteFieldAsync(int id)
        {
            await _fieldRepository.SoftDeleteAsync(id);
        }
    }
}
