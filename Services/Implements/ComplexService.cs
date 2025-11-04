using AutoMapper;
using FootballField.API.Entities;
using FootballField.API.Dtos.Complex;
using FootballField.API.Repositories.Interfaces;
using FootballField.API.Services.Interfaces;

namespace FootballField.API.Services.Implements
{
    public class ComplexService : IComplexService
    {
        private readonly IComplexRepository _complexRepository;
        private readonly IMapper _mapper;

        public ComplexService(IComplexRepository complexRepository, IMapper mapper)
        {
            _complexRepository = complexRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ComplexDto>> GetAllComplexesAsync()
        {
            var complexes = await _complexRepository.GetAllAsync(c => !c.IsDeleted);
            return _mapper.Map<IEnumerable<ComplexDto>>(complexes);
        }

        public async Task<(IEnumerable<ComplexDto> complexes, int totalCount)> GetPagedComplexesAsync(int pageIndex, int pageSize)
        {
            var (complexes, totalCount) = await _complexRepository.GetPagedAsync(pageIndex, pageSize, c => !c.IsDeleted);
            var complexDtos = _mapper.Map<IEnumerable<ComplexDto>>(complexes);
            return (complexDtos, totalCount);
        }

        public async Task<ComplexDto?> GetComplexByIdAsync(int id)
        {
            var complex = await _complexRepository.GetByIdAsync(id);
            return complex == null ? null : _mapper.Map<ComplexDto>(complex);
        }

        public async Task<ComplexWithFieldsDto?> GetComplexWithFieldsAsync(int id)
        {
            var complex = await _complexRepository.GetComplexWithFieldsAsync(id);
            return complex == null ? null : _mapper.Map<ComplexWithFieldsDto>(complex);
        }

        public async Task<IEnumerable<ComplexDto>> GetComplexesByOwnerIdAsync(int ownerId)
        {
            var complexes = await _complexRepository.GetByOwnerIdAsync(ownerId);
            return _mapper.Map<IEnumerable<ComplexDto>>(complexes);
        }

        public async Task<ComplexDto> CreateComplexAsync(CreateComplexDto createComplexDto)
        {
            var complex = _mapper.Map<Complex>(createComplexDto);
            complex.CreatedAt = DateTime.Now;
            complex.UpdatedAt = DateTime.Now;
            
            var created = await _complexRepository.AddAsync(complex);
            return _mapper.Map<ComplexDto>(created);
        }

        public async Task UpdateComplexAsync(int id, UpdateComplexDto updateComplexDto)
        {
            var existingComplex = await _complexRepository.GetByIdAsync(id);
            if (existingComplex == null)
                throw new Exception("Complex not found");

            _mapper.Map(updateComplexDto, existingComplex);
            existingComplex.UpdatedAt = DateTime.Now;
            
            await _complexRepository.UpdateAsync(existingComplex);
        }

        public async Task SoftDeleteComplexAsync(int id)
        {
            await _complexRepository.SoftDeleteAsync(id);
        }
        
    }
}
