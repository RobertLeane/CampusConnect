using Client.Models;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace CampusConnect_Tests;

public class LoginRequestTests
{
    [Fact]
    public void LoginRequest_Properties_CanBeSetAndGet()
    {
        // Arrange
        var loginRequest = new LoginRequest();

        // Act
        loginRequest.Email = "test@example.com";
        loginRequest.Password = "password123";

        // Assert
        Assert.Equal("test@example.com", loginRequest.Email);
        Assert.Equal("password123", loginRequest.Password);
    }

    [Fact]
    public void LoginRequest_DefaultValues_AreEmptyStrings()
    {
        // Arrange & Act
        var loginRequest = new LoginRequest();

        // Assert
        Assert.Equal("", loginRequest.Email);
        Assert.Equal("", loginRequest.Password);
    }

    [Fact]
    public void LoginRequest_CanCreateWithInitializer()
    {
        // Arrange & Act
        var loginRequest = new LoginRequest
        {
            Email = "user@example.com",
            Password = "mypassword"
        };

        // Assert
        Assert.Equal("user@example.com", loginRequest.Email);
        Assert.Equal("mypassword", loginRequest.Password);
    }

    [Fact]
    public void LoginRequest_Email_HasRequiredAttribute()
    {
        // Arrange
        var property = typeof(LoginRequest).GetProperty(nameof(LoginRequest.Email));

        // Act
        var requiredAttribute = property?.GetCustomAttributes(typeof(RequiredAttribute), false);

        // Assert
        Assert.NotNull(requiredAttribute);
        Assert.NotEmpty(requiredAttribute);
    }

    [Fact]
    public void LoginRequest_Email_HasEmailAddressAttribute()
    {
        // Arrange
        var property = typeof(LoginRequest).GetProperty(nameof(LoginRequest.Email));

        // Act
        var emailAttribute = property?.GetCustomAttributes(typeof(EmailAddressAttribute), false);

        // Assert
        Assert.NotNull(emailAttribute);
        Assert.NotEmpty(emailAttribute);
    }

    [Fact]
    public void LoginRequest_Password_HasRequiredAttribute()
    {
        // Arrange
        var property = typeof(LoginRequest).GetProperty(nameof(LoginRequest.Password));

        // Act
        var requiredAttribute = property?.GetCustomAttributes(typeof(RequiredAttribute), false);

        // Assert
        Assert.NotNull(requiredAttribute);
        Assert.NotEmpty(requiredAttribute);
    }

    [Theory]
    [InlineData("valid@email.com", "password123", true)]
    [InlineData("", "password123", false)] // Empty email
    [InlineData("plaintext", "password123", false)] // Invalid email format
    [InlineData("valid@email.com", "", false)] // Empty password
    [InlineData("", "", false)] // Both empty
    public void LoginRequest_Validation_WorksCorrectly(string email, string password, bool expectedValid)
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = email,
            Password = password
        };

        var context = new ValidationContext(loginRequest);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(loginRequest, context, results, true);

        // Assert
        Assert.Equal(expectedValid, isValid);
        
        if (!expectedValid)
        {
            Assert.NotEmpty(results);
        }
        else
        {
            Assert.Empty(results);
        }
    }

    [Fact]
    public void LoginRequest_RequiredValidation_FailsForEmptyEmail()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "",
            Password = "validpassword"
        };

        var context = new ValidationContext(loginRequest);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(loginRequest, context, results, true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains("Email"));
    }

    [Fact]
    public void LoginRequest_RequiredValidation_FailsForEmptyPassword()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "valid@email.com",
            Password = ""
        };

        var context = new ValidationContext(loginRequest);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(loginRequest, context, results, true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains("Password"));
    }

    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.co.uk")]
    [InlineData("admin@subdomain.example.org")]
    [InlineData("firstname.lastname@company.com")]
    public void LoginRequest_EmailValidation_PassesForValidFormats(string validEmail)
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = validEmail,
            Password = "validpassword"
        };

        var context = new ValidationContext(loginRequest);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(loginRequest, context, results, true);

        // Assert
        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Fact]
    public void LoginRequest_EmailValidation_FailsForInvalidFormats()
    {
        // Test a single clearly invalid email format
        var loginRequest = new LoginRequest
        {
            Email = "not-an-email",
            Password = "validpassword"
        };

        var context = new ValidationContext(loginRequest);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(loginRequest, context, results, true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains("Email"));
    }

    [Fact]
    public void LoginRequest_Properties_AreIndependent()
    {
        // Arrange
        var request1 = new LoginRequest { Email = "user1@test.com", Password = "pass1" };
        var request2 = new LoginRequest { Email = "user2@test.com", Password = "pass2" };

        // Act & Assert
        Assert.NotEqual(request1.Email, request2.Email);
        Assert.NotEqual(request1.Password, request2.Password);
    }

    [Fact]
    public void LoginRequest_ValidationContext_Property()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "test@example.com",
            Password = "password"
        };

        // Act
        var context = new ValidationContext(loginRequest);

        // Assert
        Assert.Equal(loginRequest, context.ObjectInstance);
        Assert.Equal(typeof(LoginRequest), context.ObjectType);
    }
}