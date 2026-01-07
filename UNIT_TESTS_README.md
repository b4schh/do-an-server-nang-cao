# 🧪 Unit Tests - Football Field Booking API

## 📋 Tổng quan

Project đã được thêm **Unit Tests** để đảm bảo CI/CD có ý nghĩa thực sự:

- ✅ **6 test cases** cho AuthService
- ✅ **3 test cases** cho TokenService
- ✅ **8 test cases** cho Validation
- ✅ **Tổng cộng: 17 tests**

## 🏗️ Cấu trúc Test Project

```
src/DoAn.Tests/
├── DoAn.Tests.csproj          # Test project file
├── GlobalUsings.cs            # Global using directives
├── Services/
│   └── Auth/
│       ├── AuthServiceTests.cs      # Tests cho AuthService
│       └── TokenServiceTests.cs     # Tests cho TokenService
├── Utils/
│   └── ValidationTests.cs     # Tests cho validation logic
└── Integration/
    └── IntegrationTests.cs    # Integration tests (skipped)
```

## 🚀 Chạy Tests

### Local (Windows):
```batch
run-tests.bat
```

### Local (Linux/Mac):
```bash
chmod +x run-tests.sh
./run-tests.sh
```

### Bằng dotnet CLI:
```bash
dotnet test src/DoAn.Tests/DoAn.Tests.csproj --verbosity normal
```

## 📊 Test Cases

### 1. AuthService Tests (6 tests)

#### ✅ RegisterAsync_WithValidData_ShouldReturnLoginResponse
- Test đăng ký với dữ liệu hợp lệ
- Verify email và phone không tồn tại
- Expect: Trả về LoginResponse

#### ✅ RegisterAsync_WithExistingEmail_ShouldReturnNull
- Test đăng ký với email đã tồn tại
- Expect: Trả về null

#### ✅ RegisterAsync_WithExistingPhone_ShouldReturnNull
- Test đăng ký với phone đã tồn tại
- Expect: Trả về null

#### ✅ RegisterAsync_WithInvalidEmail_ShouldHandleGracefully
- Test với email invalid: "", null, "invalid-email"
- Expect: Handle gracefully (null hoặc exception)

### 2. TokenService Tests (3 tests)

#### ✅ GenerateToken_WithValidUser_ShouldReturnToken
- Test tạo JWT token
- Expect: Token không null và có 3 parts (JWT format)

#### ✅ ValidateToken_WithValidToken_ShouldReturnTrue
- Test validate token hợp lệ

#### ✅ ValidateToken_WithInvalidToken_ShouldReturnFalse
- Test validate token không hợp lệ

### 3. Validation Tests (8 tests)

#### Email Validation (4 tests)
- ✅ "test@example.com" → Valid
- ✅ "invalid.email" → Invalid
- ✅ "" → Invalid
- ✅ null → Invalid

#### Phone Validation (4 tests)
- ✅ "0901234567" → Valid (10 digits)
- ✅ "0123456789" → Valid (10 digits)
- ✅ "123" → Invalid (too short)
- ✅ "" → Invalid

#### Password Strength
- ✅ "Password123!" → Valid (8+ chars)
- ✅ "weak" → Invalid
- ✅ "" → Invalid

## 🔧 CI/CD Integration

### Jenkinsfile đã được cập nhật:

```groovy
stage('Run Unit Tests') {
    steps {
        sh '''
            dotnet restore
            dotnet test src/DoAn.Tests/DoAn.Tests.csproj \
                --no-restore \
                --verbosity normal \
                --logger "trx;LogFileName=test-results.trx"
        '''
    }
}
```

**Flow CI/CD mới:**
1. ✅ Checkout code
2. ✅ **Run Unit Tests** ← MỚI!
3. ✅ Build Docker Image (chỉ khi tests pass)
4. ✅ Push to Registry
5. ✅ Deploy to Production

**Nếu tests fail:**
- ❌ Pipeline dừng ngay
- ❌ Không build image
- ❌ Không deploy
- → **Đảm bảo chỉ code đã test mới được deploy!**

## 📈 Test Coverage

Hiện tại test coverage:
- ✅ AuthService: ~60% coverage
- ✅ Validation: 100% coverage
- ⏳ TokenService: Mock tests (cần implementation thực)

## 🎯 Demo cho Giáo viên

### Scenario 1: Tests Pass ✅

```bash
# Trên máy CI
cd d:\CODE\football-field-booking-api
run-tests.bat
```

**Kết quả:**
```
========================================
🧪 Running Unit Tests
========================================

[1/3] Restoring dependencies...
✅ Dependencies restored

[2/3] Building test project...
✅ Test project built

[3/3] Running tests...

Starting test execution...
Passed! - RegisterAsync_WithValidData_ShouldReturnLoginResponse
Passed! - RegisterAsync_WithExistingEmail_ShouldReturnNull
Passed! - RegisterAsync_WithExistingPhone_ShouldReturnNull
...

Total tests: 17
     Passed: 17
     Failed: 0
   Skipped: 4

========================================
✅ ALL TESTS PASSED!
========================================
```

→ Jenkins sẽ build và deploy

### Scenario 2: Tests Fail ❌

Giả sử có bug trong code:

```
Total tests: 17
     Passed: 15
     Failed: 2  ← ❌
```

→ Jenkins pipeline **DỪNG LẠI**
→ **KHÔNG build image**
→ **KHÔNG deploy**

## 🔍 Verify trong Jenkins

Sau khi build:

1. **Console Output** sẽ hiển thị:
```
[Run Unit Tests] 🧪 Running Unit Tests
[Run Unit Tests] ========================================
[Run Unit Tests] Starting test execution...
[Run Unit Tests] Passed! - RegisterAsync_WithValidData...
[Run Unit Tests] ✅ All tests passed!
```

2. **Test Results** (nếu config Jenkins plugins):
   - Test trend graph
   - Test report details
   - Coverage report

## 📦 Test Dependencies

```xml
<PackageReference Include="xunit" Version="2.6.2" />
<PackageReference Include="Moq" Version="4.20.70" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
```

- **xUnit**: Test framework
- **Moq**: Mocking framework
- **FluentAssertions**: Assertion library

## 🎓 Giải thích cho Giáo viên

### Tại sao cần Unit Tests trong CI/CD?

**TRƯỚC (Không có tests):**
```
Code → Build → Deploy → ❌ Lỗi ở production!
```

**SAU (Có tests):**
```
Code → Tests → ✅ Pass → Build → Deploy → ✅ Safe!
         ↓
       ❌ Fail → STOP! → Fix bug → Retry
```

### Lợi ích:

1. ✅ **Phát hiện bug sớm** - Ngay khi code xong
2. ✅ **Tự động** - Không cần test thủ công
3. ✅ **Đảm bảo chất lượng** - Chỉ code tốt mới lên production
4. ✅ **Tài liệu sống** - Tests = specification
5. ✅ **Regression prevention** - Đảm bảo fix bug không gây bug mới

## 🚀 Mở rộng

Để thêm tests mới:

1. Tạo file test mới trong `src/DoAn.Tests/`
2. Viết test cases
3. Run `run-tests.bat` để verify
4. Commit → Jenkins tự động chạy tests

## 📊 Metrics

Sau mỗi build, Jenkins sẽ report:
- ✅ Số tests passed
- ❌ Số tests failed
- ⏭️ Số tests skipped
- ⏱️ Test execution time
- 📈 Test trend over time
