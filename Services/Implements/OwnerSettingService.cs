using FootballField.API.Dtos.OwnerSetting;
using FootballField.API.Entities;
using FootballField.API.Repositories.Interfaces;
using AutoMapper;
using FootballField.API.Services.Interfaces;

namespace FootballField.API.Services.Implements
{
    public class OwnerSettingService : IOwnerSettingService
    {
        private readonly IOwnerSettingRepository _repo;
        private readonly IMapper _mapper;

        public OwnerSettingService(IOwnerSettingRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<OwnerSettingDto>> GetAllAsync()
        {
            var items = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable<OwnerSettingDto>>(items);
        }

        public async Task<OwnerSettingDto?> GetByIdAsync(int id)
        {
            var item = await _repo.GetByIdAsync(id);
            return _mapper.Map<OwnerSettingDto>(item);
        }

        public async Task<OwnerSettingDto> CreateAsync(CreateOwnerSettingDto dto)
        {
            var entity = _mapper.Map<OwnerSetting>(dto);
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

            var result = await _repo.AddAsync(entity);
            return _mapper.Map<OwnerSettingDto>(result);
        }

        public async Task UpdateAsync(int id, UpdateOwnerSettingDto dto)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) throw new Exception("OwnerSetting not found");

            _mapper.Map(dto, entity);
            entity.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(entity);
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) throw new Exception("OwnerSetting not found");

            await _repo.DeleteAsync(entity);
        }
    }
}
