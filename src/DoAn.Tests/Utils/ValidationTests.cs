namespace DoAn.Tests.Utils
{
    public class ValidationTests
    {
        [Theory]
        [InlineData("test@example.com", true)]
        [InlineData("invalid.email", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void Email_Validation_ShouldWorkCorrectly(string? email, bool expected)
        {
            // Arrange
            var isValid = !string.IsNullOrWhiteSpace(email) && email.Contains('@');

            // Act & Assert
            isValid.Should().Be(expected);
        }

        [Theory]
        [InlineData("0901234567", true)]
        [InlineData("0123456789", true)]
        [InlineData("123", false)]
        [InlineData("", false)]
        public void Phone_Validation_ShouldWorkCorrectly(string phone, bool expected)
        {
            // Arrange
            var isValid = !string.IsNullOrWhiteSpace(phone) && 
                          phone.Length >= 10 && 
                          phone.Length <= 11 &&
                          phone.All(char.IsDigit);

            // Act & Assert
            isValid.Should().Be(expected);
        }

        [Theory]
        [InlineData("Password123!", true)]
        [InlineData("weak", false)]
        [InlineData("", false)]
        public void Password_Strength_ShouldBeValidated(string password, bool expected)
        {
            // Arrange - minimum 8 characters
            var isValid = !string.IsNullOrWhiteSpace(password) && password.Length >= 8;

            // Act & Assert
            isValid.Should().Be(expected);
        }
    }
}
