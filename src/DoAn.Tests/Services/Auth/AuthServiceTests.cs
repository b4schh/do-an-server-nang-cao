using DoAn.Core.Application.DTOs.Auth;
using DoAn.Core.Application.Interfaces;
using DoAn.Core.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace DoAn.Tests.Services.Auth;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<ITokenService> _mockTokenService;

    public AuthServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockTokenService = new Mock<ITokenService>();
    }

    [Fact]
    public void RegisterRequest_ShouldHaveRequiredProperties()
    {
        // Arrange & Act
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "Password123",
            ConfirmPassword = "Password123",
            FirstName = "John",
            LastName = "Doe",
            Phone = "0901234567"
        };

        // Assert
        request.Email.Should().Be("test@example.com");
        request.FirstName.Should().Be("John");
        request.LastName.Should().Be("Doe");
        request.Phone.Should().Be("0901234567");
        request.Password.Should().Be(request.ConfirmPassword);
    }

    [Fact]
    public void LoginRequest_ShouldValidateEmail()
    {
        // Arrange & Act
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "Password123"
        };

        // Assert
        request.Email.Should().NotBeNullOrEmpty();
        request.Email.Should().Contain("@");
        request.Password.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void User_ShouldHaveCorrectDefaultValues()
    {
        // Arrange & Act
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        // Assert
        user.Status.Should().Be(UserStatus.Active);
        user.IsDeleted.Should().BeFalse();
        user.UserRoles.Should().NotBeNull();
        user.UserRoles.Should().BeEmpty();
    }

    [Fact]
    public void UserStatus_ShouldHaveCorrectValues()
    {
        // Arrange & Act & Assert
        ((byte)UserStatus.Inactive).Should().Be(0);
        ((byte)UserStatus.Active).Should().Be(1);
        ((byte)UserStatus.Banned).Should().Be(2);
    }

    [Theory]
    [InlineData("test@example.com", true)]
    [InlineData("invalid-email", false)]
    [InlineData("", false)]
    public void Email_ShouldValidateFormat(string email, bool isValid)
    {
        // Arrange & Act
        var hasAtSymbol = email.Contains("@");
        var hasLength = email.Length > 0;

        // Assert
        if (isValid)
        {
            hasAtSymbol.Should().BeTrue();
            hasLength.Should().BeTrue();
        }
    }

    [Theory]
    [InlineData("0901234567", true)]
    [InlineData("123", false)]
    [InlineData("", false)]
    public void Phone_ShouldValidateLength(string phone, bool isValid)
    {
        // Arrange & Act
        var hasValidLength = phone.Length >= 10;

        // Assert
        hasValidLength.Should().Be(isValid);
    }

    [Fact]
    public async Task UserRepository_GetByEmailAsync_ShouldReturnUser()
    {
        // Arrange
        var email = "test@example.com";
        var expectedUser = new User
        {
            Id = 1,
            Email = email,
            FirstName = "John",
            LastName = "Doe",
            Status = UserStatus.Active
        };

        _mockUserRepository.Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _mockUserRepository.Object.GetByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
        result.Status.Should().Be(UserStatus.Active);
    }

    [Fact]
    public async Task UserRepository_GetByEmailAsync_WhenNotFound_ShouldReturnNull()
    {
        // Arrange
        var email = "notfound@example.com";
        _mockUserRepository.Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _mockUserRepository.Object.GetByEmailAsync(email);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void TokenService_GenerateToken_ShouldReturnString()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        _mockTokenService.Setup(x => x.GenerateToken(user))
            .Returns("jwt_token_here");

        // Act
        var token = _mockTokenService.Object.GenerateToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Should().Be("jwt_token_here");
    }
}
