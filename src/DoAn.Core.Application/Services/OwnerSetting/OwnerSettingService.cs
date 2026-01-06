using AutoMapper;
using DoAn.Core.Domain.Entities;
using DoAn.Core.Application.Interfaces.OwnerSetting;
using DoAn.Core.Application.Interfaces.User;
using DoAn.Core.Application.Interfaces.SystemConfig;
using DoAn.Core.Application.DTOs.OwnerSetting;
using DoAn.Core.Application.Interfaces.Storage;
using DoAn.Core.Application.Common.Utils;

namespace DoAn.Core.Application.Services.OwnerSetting
{
    public class OwnerSettingService : IOwnerSettingService
    {
        private readonly IOwnerSettingRepository _repo;
        private readonly ISystemConfigService _systemConfigService;
        private readonly IStorageService _storageService;
        private readonly IMapper _mapper;

        public OwnerSettingService(
            IOwnerSettingRepository repo,
            ISystemConfigService systemConfigService,
            IStorageService storageService,
            IMapper mapper)
        {
            _repo = repo;
            _systemConfigService = systemConfigService;
            _storageService = storageService;
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

        public async Task<OwnerSettingDto?> GetByOwnerIdAsync(int ownerId)
        {
            var item = await _repo.GetByOwnerIdAsync(ownerId);
            return _mapper.Map<OwnerSettingDto>(item);
        }

        public async Task<OwnerSettingResponseDto> GetSettingsWithDefaultsAsync(int ownerId)
        {
            var ownerSetting = await _repo.GetByOwnerIdAsync(ownerId);
            
            // Get system defaults
            var defaultDepositRate = await _systemConfigService.GetConfigValueAsync<decimal?>("DEFAULT_DEPOSIT_RATE") ?? 0.30m;
            var defaultMinBookingNotice = await _systemConfigService.GetConfigValueAsync<int?>("MIN_BOOKING_NOTICE_HOURS") ?? 2;
            var defaultAllowReview = await _systemConfigService.GetConfigValueAsync<bool?>("ENABLE_REVIEW_SYSTEM") ?? true;

            return new OwnerSettingResponseDto
            {
                Data = _mapper.Map<OwnerSettingDto>(ownerSetting),
                SystemDefaults = new SystemDefaultsDto
                {
                    DepositRate = defaultDepositRate,
                    MinBookingNotice = defaultMinBookingNotice,
                    AllowReview = defaultAllowReview
                }
            };
        }

        public async Task<OwnerSettingDto> CreateAsync(CreateOwnerSettingDto dto)
        {
            var entity = _mapper.Map<OwnerSettingEntity>(dto);
            entity.CreatedAt = TimeZoneHelper.VietnamNow;
            entity.UpdatedAt = TimeZoneHelper.VietnamNow;

            var result = await _repo.AddAsync(entity);
            return _mapper.Map<OwnerSettingDto>(result);
        }

        public async Task UpdateAsync(int id, UpdateOwnerSettingDto dto)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) throw new Exception("OwnerSetting not found");

            _mapper.Map(dto, entity);
            entity.UpdatedAt = TimeZoneHelper.VietnamNow;

            await _repo.UpdateAsync(entity);
        }

        public async Task UpdateSettingsAsync(int ownerId, UpdateOwnerSettingDto dto)
        {
            var entity = await _repo.GetByOwnerIdAsync(ownerId);

            if (entity == null)
            {
                // Lazy creation
                entity = new OwnerSettingEntity
                {
                    OwnerId = ownerId,
                    CreatedAt = TimeZoneHelper.VietnamNow,
                    UpdatedAt = TimeZoneHelper.VietnamNow
                };
                
                _mapper.Map(dto, entity);
                await _repo.AddAsync(entity);
            }
            else
            {
                _mapper.Map(dto, entity);
                entity.UpdatedAt = TimeZoneHelper.VietnamNow;
                await _repo.UpdateAsync(entity);
            }
        }

        public async Task UpdateBankInfoAsync(int ownerId, UpdateBankInfoDto dto)
        {
            var entity = await _repo.GetByOwnerIdAsync(ownerId);

            if (entity == null)
            {
                // Lazy creation
                entity = new OwnerSettingEntity
                {
                    OwnerId = ownerId,
                    CreatedAt = TimeZoneHelper.VietnamNow,
                    UpdatedAt = TimeZoneHelper.VietnamNow
                };
                await _repo.AddAsync(entity);
            }

            // Update bank info
            entity.BankName = dto.BankName;
            entity.BankAccountNumber = dto.BankAccountNumber;
            entity.BankAccountName = dto.BankAccountName;

            // Upload QR code if provided
            if (dto.QrCodeImage != null)
            {
                // Delete old QR code if exists
                if (!string.IsNullOrEmpty(entity.BankQrCodeUrl))
                {
                    await _storageService.DeleteAsync(entity.BankQrCodeUrl);
                }

                // Upload new QR code
                using var stream = dto.QrCodeImage.OpenReadStream();
                var objectName = $"qr-codes/owner-{ownerId}/{Guid.NewGuid()}{Path.GetExtension(dto.QrCodeImage.FileName)}";
                var qrUrl = await _storageService.UploadAsync(
                    stream,
                    objectName,
                    dto.QrCodeImage.ContentType
                );
                entity.BankQrCodeUrl = qrUrl;
            }

            entity.UpdatedAt = TimeZoneHelper.VietnamNow;
            await _repo.UpdateAsync(entity);
        }

        public async Task<bool> ValidateBankInfoAsync(int ownerId)
        {
            var entity = await _repo.GetByOwnerIdAsync(ownerId);
            
            if (entity == null)
                return false;

            return !string.IsNullOrEmpty(entity.BankName) &&
                   !string.IsNullOrEmpty(entity.BankAccountNumber) &&
                   !string.IsNullOrEmpty(entity.BankAccountName) &&
                   !string.IsNullOrEmpty(entity.BankQrCodeUrl);
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) throw new Exception("OwnerSetting not found");

            await _repo.DeleteAsync(entity);
        }
    }
}