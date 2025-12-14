# LocationManagement Module - Hướng Dẫn Sử Dụng

## Tổng Quan

Module **LocationManagement** cung cấp chức năng quản lý thông tin địa giới hành chính của Việt Nam, bao gồm:
- Tỉnh / Thành phố (Province)
- Phường / Xã (Ward)

Dữ liệu được lấy từ API mở: https://provinces.open-api.vn

## Cấu Trúc Module

```
Modules/LocationManagement/
├── Entities/
│   ├── Province.cs
│   └── Ward.cs
├── Configurations/
│   ├── ProvinceConfiguration.cs
│   └── WardConfiguration.cs
├── Repositories/
│   ├── IProvinceRepository.cs
│   ├── IWardRepository.cs
│   ├── ProvinceRepository.cs
│   └── WardRepository.cs
├── Services/
│   ├── IProvinceService.cs
│   ├── IWardService.cs
│   ├── ProvinceService.cs
│   ├── WardService.cs
│   └── LocationSeeder.cs
├── Dtos/
│   ├── ProvinceDto.cs
│   ├── WardDto.cs
│   └── ProvinceWithWardsDto.cs
├── Controllers/
│   └── LocationController.cs
└── LocationManagementModule.cs
```

## Database Schema

Module sử dụng schema riêng `location` với 2 bảng:

### Bảng `location.Provinces`
```sql
Code (int, PK)
Name (nvarchar(100))
Codename (nvarchar(100))
DivisionType (nvarchar(50))
```

### Bảng `location.Wards`
```sql
Code (int, PK)
Name (nvarchar(100))
Codename (nvarchar(100))
DivisionType (nvarchar(50))
ProvinceCode (int, FK → location.Provinces)
```

## Hướng Dẫn Cài Đặt

### Bước 1: Tạo Migration

Mở terminal tại thư mục gốc của project và chạy lệnh:

```powershell
dotnet ef migrations add AddLocationManagementModule
```

### Bước 2: Áp Dụng Migration

Cập nhật database với migration vừa tạo:

```powershell
dotnet ef database update
```

Lệnh này sẽ:
- Tạo schema `location`
- Tạo bảng `location.Provinces`
- Tạo bảng `location.Wards`
- Tạo các indexes và foreign keys

### Bước 3: Seeding Dữ Liệu

**Cách 1: Tự động khi khởi động ứng dụng (Đã cấu hình sẵn)**

Khi bạn chạy ứng dụng lần đầu tiên sau khi migration, dữ liệu địa điểm sẽ tự động được seeded từ API:

```powershell
dotnet run
```

Trong file `Program.cs`, seeder đã được cấu hình để chạy tự động:
- Kiểm tra nếu database chưa có dữ liệu Province
- Gọi API https://provinces.open-api.vn để lấy danh sách tỉnh
- Với mỗi tỉnh, lấy danh sách phường/xã
- Lưu tất cả vào database

**Cách 2: Chạy thủ công bằng endpoint (Nếu cần re-seed)**

Nếu cần xóa và re-seed dữ liệu, bạn có thể:
1. Xóa dữ liệu trong database thủ công
2. Khởi động lại ứng dụng để trigger seeder

### Bước 4: Kiểm Tra Dữ Liệu

Kiểm tra dữ liệu đã được seed thành công:

```sql
-- Kiểm tra số lượng tỉnh/thành phố
SELECT COUNT(*) FROM location.Provinces;

-- Kiểm tra số lượng phường/xã
SELECT COUNT(*) FROM location.Wards;

-- Xem danh sách một vài tỉnh
SELECT TOP 10 * FROM location.Provinces;

-- Xem phường/xã của Hà Nội (code = 1)
SELECT * FROM location.Wards WHERE province_code = 1;
```

## API Endpoints

Module cung cấp các API endpoints sau:

### 1. Lấy Danh Sách Tất Cả Tỉnh/Thành Phố

```http
GET /api/locations/provinces
```

**Response:**
```json
{
  "success": true,
  "message": "Lấy danh sách tỉnh/thành phố thành công",
  "data": [
    {
      "code": 1,
      "name": "Thành phố Hà Nội",
      "codename": "thanh_pho_ha_noi",
      "divisionType": "Thành phố Trung ương"
    },
    {
      "code": 79,
      "name": "Thành phố Hồ Chí Minh",
      "codename": "thanh_pho_ho_chi_minh",
      "divisionType": "Thành phố Trung ương"
    }
  ]
}
```

### 2. Lấy Thông Tin Tỉnh/Thành Phố Theo Mã

```http
GET /api/locations/provinces/{code}
```

**Example:**
```http
GET /api/locations/provinces/1
```

**Response:**
```json
{
  "success": true,
  "message": "Lấy thông tin tỉnh/thành phố thành công",
  "data": {
    "code": 1,
    "name": "Thành phố Hà Nội",
    "codename": "thanh_pho_ha_noi",
    "divisionType": "Thành phố Trung ương"
  }
}
```

### 3. Lấy Danh Sách Phường/Xã Theo Tỉnh/Thành Phố

```http
GET /api/locations/provinces/{code}/wards
```

**Example:**
```http
GET /api/locations/provinces/1/wards
```

**Response:**
```json
{
  "success": true,
  "message": "Lấy danh sách phường/xã thành công",
  "data": [
    {
      "code": 1,
      "name": "Phường Phúc Xá",
      "codename": "phuong_phuc_xa",
      "divisionType": "Phường",
      "provinceCode": 1
    },
    {
      "code": 4,
      "name": "Phường Trúc Bạch",
      "codename": "phuong_truc_bach",
      "divisionType": "Phường",
      "provinceCode": 1
    }
  ]
}
```

### 4. Lấy Thông Tin Phường/Xã Theo Mã

```http
GET /api/locations/wards/{code}
```

**Example:**
```http
GET /api/locations/wards/1
```

**Response:**
```json
{
  "success": true,
  "message": "Lấy thông tin phường/xã thành công",
  "data": {
    "code": 1,
    "name": "Phường Phúc Xá",
    "codename": "phuong_phuc_xa",
    "divisionType": "Phường",
    "provinceCode": 1
  }
}
```

## Hướng Dẫn Sử Dụng Cho Frontend

### Kịch Bản: Dropdown Chọn Tỉnh → Phường/Xã

```javascript
// 1. Lấy danh sách tỉnh/thành phố khi load trang
async function loadProvinces() {
  const response = await fetch('http://localhost:5000/api/locations/provinces');
  const result = await response.json();
  
  if (result.success) {
    const provinces = result.data;
    // Populate dropdown tỉnh/thành phố
    populateProvinceDropdown(provinces);
  }
}

// 2. Khi user chọn tỉnh, load danh sách phường/xã
async function onProvinceChange(provinceCode) {
  const response = await fetch(`http://localhost:5000/api/locations/provinces/${provinceCode}/wards`);
  const result = await response.json();
  
  if (result.success) {
    const wards = result.data;
    // Populate dropdown phường/xã
    populateWardDropdown(wards);
  }
}
```

### React Example

```jsx
import { useState, useEffect } from 'react';

function LocationSelector() {
  const [provinces, setProvinces] = useState([]);
  const [wards, setWards] = useState([]);
  const [selectedProvince, setSelectedProvince] = useState('');
  const [selectedWard, setSelectedWard] = useState('');

  // Load provinces on mount
  useEffect(() => {
    fetch('http://localhost:5000/api/locations/provinces')
      .then(res => res.json())
      .then(result => {
        if (result.success) {
          setProvinces(result.data);
        }
      });
  }, []);

  // Load wards when province changes
  useEffect(() => {
    if (selectedProvince) {
      fetch(`http://localhost:5000/api/locations/provinces/${selectedProvince}/wards`)
        .then(res => res.json())
        .then(result => {
          if (result.success) {
            setWards(result.data);
          }
        });
    } else {
      setWards([]);
    }
    setSelectedWard(''); // Reset ward selection
  }, [selectedProvince]);

  return (
    <div>
      <select 
        value={selectedProvince} 
        onChange={(e) => setSelectedProvince(e.target.value)}
      >
        <option value="">Chọn Tỉnh/Thành phố</option>
        {provinces.map(p => (
          <option key={p.code} value={p.code}>{p.name}</option>
        ))}
      </select>

      <select 
        value={selectedWard} 
        onChange={(e) => setSelectedWard(e.target.value)}
        disabled={!selectedProvince}
      >
        <option value="">Chọn Phường/Xã</option>
        {wards.map(w => (
          <option key={w.code} value={w.code}>{w.name}</option>
        ))}
      </select>
    </div>
  );
}
```

## Testing với Swagger

1. Chạy ứng dụng:
   ```powershell
   dotnet run
   ```

2. Mở trình duyệt và truy cập Swagger UI:
   ```
   https://localhost:5001/swagger
   hoặc
   http://localhost:5000/swagger
   ```

3. Test các endpoints trong section **Location**:
   - `GET /api/locations/provinces` - Lấy tất cả tỉnh
   - `GET /api/locations/provinces/{code}` - Lấy 1 tỉnh
   - `GET /api/locations/provinces/{code}/wards` - Lấy phường/xã theo tỉnh
   - `GET /api/locations/wards/{code}` - Lấy 1 phường/xã

## Troubleshooting

### Lỗi: Migration không tạo được schema

**Giải pháp:**
```sql
-- Tạo schema thủ công nếu cần
CREATE SCHEMA location;
```

### Lỗi: Seeder không chạy

**Kiểm tra:**
1. Database đã được migrate chưa
2. Kết nối internet (cần để gọi API)
3. Xem logs trong console để biết chi tiết lỗi

### Lỗi: API trả về dữ liệu rỗng

**Kiểm tra:**
1. Dữ liệu đã được seed vào database chưa
   ```sql
   SELECT COUNT(*) FROM location.Provinces;
   ```
2. Nếu chưa có dữ liệu, khởi động lại ứng dụng để trigger seeder

### Lỗi: Foreign key constraint khi seed

**Giải pháp:**
- Seeder đã được thiết kế để insert Provinces trước, sau đó mới insert Wards
- Nếu vẫn gặp lỗi, xóa dữ liệu và chạy lại seeder

## Lưu Ý Quan Trọng

1. **Tránh Duplicate**: Seeder tự động kiểm tra và chỉ seed nếu chưa có dữ liệu Province
2. **Performance**: Seeder có delay 100ms giữa mỗi request để tránh quá tải API
3. **Error Handling**: Nếu một tỉnh gặp lỗi, seeder vẫn tiếp tục với các tỉnh còn lại
4. **Logging**: Tất cả hoạt động seeding được log ra console để dễ theo dõi

## Tích Hợp Với Module Khác

### Sử dụng trong ComplexManagement

Khi tạo/cập nhật Complex, có thể sử dụng Ward và Province name:

```csharp
// Trong ComplexService
var ward = await _wardRepository.GetByCodeAsync(createDto.WardCode);
var province = await _provinceRepository.GetByCodeAsync(createDto.ProvinceCode);

complex.Ward = ward.Name;
complex.Province = province.Name;
```

### Validation

Có thể thêm validation để kiểm tra Ward code có tồn tại không:

```csharp
public class CreateComplexDtoValidator : AbstractValidator<CreateComplexDto>
{
    public CreateComplexDtoValidator(IWardRepository wardRepository)
    {
        RuleFor(x => x.WardCode)
            .MustAsync(async (code, cancellation) => await wardRepository.CodeExistsAsync(code))
            .WithMessage("Mã phường/xã không hợp lệ");
    }
}
```

## Kết Luận

Module LocationManagement đã được tích hợp hoàn chỉnh vào hệ thống với:
- ✅ Schema riêng biệt (`location`)
- ✅ Auto-seeding từ API mở
- ✅ RESTful API endpoints
- ✅ Tuân thủ chuẩn kiến trúc Modular Monolith
- ✅ Sẵn sàng sử dụng cho frontend
- ✅ Logging và error handling đầy đủ

Để biết thêm chi tiết, xem source code trong `Modules/LocationManagement/`.
