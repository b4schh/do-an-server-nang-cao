using AutoMapper;
using Microsoft.Extensions.Logging;
using FootballField.API.Modules.FieldManagement.Dtos;
using FootballField.API.Modules.FieldManagement.Entities;
using FootballField.API.Modules.FieldManagement.Repositories;

namespace FootballField.API.Modules.FieldManagement.Services
{
    public class FieldService : IFieldService
    {
        private readonly IFieldRepository _fieldRepository;
        private readonly ITimeSlotRepository _timeSlotRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<FieldService> _logger;

        public FieldService(
            IFieldRepository fieldRepository, 
            ITimeSlotRepository timeSlotRepository,
            IMapper mapper,
            ILogger<FieldService> logger)
        {
            _fieldRepository = fieldRepository;
            _timeSlotRepository = timeSlotRepository;
            _mapper = mapper;
            _logger = logger;
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

        public async Task<FieldDto> CloneFieldAsync(int fieldId, CloneFieldDto cloneFieldDto)
        {
            _logger.LogInformation("Cloning Field {FieldId} with new name: {NewName}, IncludeTimeSlots: {IncludeTimeSlots}", 
                fieldId, cloneFieldDto.NewFieldName, cloneFieldDto.IncludeTimeSlots);

            var originalField = await _fieldRepository.GetFieldWithTimeSlotsAsync(fieldId);
            if (originalField == null)
            {
                throw new Exception($"Field with ID {fieldId} not found");
            }

            // Create new field with same properties
            var newField = new Field
            {
                Name = cloneFieldDto.NewFieldName,
                ComplexId = originalField.ComplexId,
                FieldSize = originalField.FieldSize,
                SurfaceType = originalField.SurfaceType,
                IsDeleted = false
            };

            var createdField = await _fieldRepository.AddAsync(newField);
            _logger.LogInformation("Created cloned Field with ID: {FieldId}", createdField.Id);

            // Clone timeslots if requested
            if (cloneFieldDto.IncludeTimeSlots && originalField.TimeSlots != null && originalField.TimeSlots.Any())
            {
                var newTimeSlots = originalField.TimeSlots.Select(ts => new TimeSlot
                {
                    FieldId = createdField.Id,
                    StartTime = ts.StartTime,
                    EndTime = ts.EndTime,
                    Price = ts.Price,
                    IsActive = ts.IsActive
                }).ToList();

                await _timeSlotRepository.AddRangeAsync(newTimeSlots);
                _logger.LogInformation("Cloned {Count} timeslots for Field {FieldId}", newTimeSlots.Count, createdField.Id);
            }

            return _mapper.Map<FieldDto>(createdField);
        }

        public async Task BatchAddTimeSlotsAsync(BatchAddTimeSlotsDto batchAddTimeSlotsDto)
        {
            _logger.LogInformation("Batch adding timeslots to {FieldCount} fields", batchAddTimeSlotsDto.FieldIds.Count);

            var allNewTimeSlots = new List<TimeSlot>();

            foreach (var fieldId in batchAddTimeSlotsDto.FieldIds)
            {
                var field = await _fieldRepository.GetByIdAsync(fieldId);
                if (field == null)
                {
                    _logger.LogWarning("Field {FieldId} not found, skipping", fieldId);
                    continue;
                }

                foreach (var template in batchAddTimeSlotsDto.TimeSlots)
                {
                    var newTimeSlot = new TimeSlot
                    {
                        FieldId = fieldId,
                        StartTime = template.StartTime,
                        EndTime = template.EndTime,
                        Price = template.Price,
                        IsActive = true
                    };
                    allNewTimeSlots.Add(newTimeSlot);
                }
            }

            if (allNewTimeSlots.Any())
            {
                await _timeSlotRepository.AddRangeAsync(allNewTimeSlots);
                _logger.LogInformation("Successfully added {Count} timeslots across {FieldCount} fields", 
                    allNewTimeSlots.Count, batchAddTimeSlotsDto.FieldIds.Count);
            }
        }
    }
}
