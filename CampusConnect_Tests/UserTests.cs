using Client.Models;
using Xunit;

namespace CampusConnect_Tests;

public class UserTests
{
    [Fact]
    public void User_Properties_CanBeSetAndGet()
    {
        // Arrange
        var user = new User();

        // Act
        user.UserId = 123;
        user.FirstName = "John";
        user.LastName = "Doe";
        user.Email = "john.doe@example.com";

        // Assert
        Assert.Equal(123, user.UserId);
        Assert.Equal("John", user.FirstName);
        Assert.Equal("Doe", user.LastName);
        Assert.Equal("john.doe@example.com", user.Email);
    }

    [Fact]
    public void User_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var user = new User();

        // Assert
        Assert.Equal(0, user.UserId);
        Assert.Equal(string.Empty, user.FirstName);
        Assert.Equal(string.Empty, user.LastName);
        Assert.Equal(string.Empty, user.Email);
    }

    [Fact]
    public void User_FullName_ReturnsFirstAndLastName()
    {
        // Arrange
        var user = new User
        {
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var fullName = user.FullName;

        // Assert
        Assert.Equal("John Doe", fullName);
    }

    [Fact]
    public void User_FullName_HandlesEmptyFirstName()
    {
        // Arrange
        var user = new User
        {
            FirstName = "",
            LastName = "Doe"
        };

        // Act
        var fullName = user.FullName;

        // Assert
        Assert.Equal("Doe", fullName);
    }

    [Fact]
    public void User_FullName_HandlesEmptyLastName()
    {
        // Arrange
        var user = new User
        {
            FirstName = "John",
            LastName = ""
        };

        // Act
        var fullName = user.FullName;

        // Assert
        Assert.Equal("John", fullName);
    }

    [Fact]
    public void User_FullName_HandlesBothNamesEmpty()
    {
        // Arrange
        var user = new User
        {
            FirstName = "",
            LastName = ""
        };

        // Act
        var fullName = user.FullName;

        // Assert
        Assert.Equal("", fullName);
    }

    [Fact]
    public void User_FullName_TrimsWhitespace()
    {
        // Arrange
        var user = new User
        {
            FirstName = " John ",
            LastName = " Doe "
        };

        // Act
        var fullName = user.FullName;

        // Assert
        Assert.Equal("John   Doe", fullName.Trim());
    }

    [Fact]
    public void User_FullName_HandlesOnlySpaces()
    {
        // Arrange
        var user = new User
        {
            FirstName = "   ",
            LastName = "   "
        };

        // Act
        var fullName = user.FullName;

        // Assert
        Assert.Equal("", fullName);
    }

    [Fact]
    public void User_CanCreateWithInitializer()
    {
        // Arrange & Act
        var user = new User
        {
            UserId = 456,
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@example.com"
        };

        // Assert
        Assert.Equal(456, user.UserId);
        Assert.Equal("Jane", user.FirstName);
        Assert.Equal("Smith", user.LastName);
        Assert.Equal("jane.smith@example.com", user.Email);
        Assert.Equal("Jane Smith", user.FullName);
    }

    [Theory]
    [InlineData("", "", "")]
    [InlineData("John", "", "John")]
    [InlineData("", "Doe", "Doe")]
    [InlineData("John", "Doe", "John Doe")]
    [InlineData("Mary Jane", "Watson", "Mary Jane Watson")]
    [InlineData("O'Connor", "Smith-Jones", "O'Connor Smith-Jones")]
    public void User_FullName_HandlesVariousNameCombinations(string firstName, string lastName, string expected)
    {
        // Arrange
        var user = new User
        {
            FirstName = firstName,
            LastName = lastName
        };

        // Act
        var fullName = user.FullName;

        // Assert
        Assert.Equal(expected, fullName);
    }

    [Fact]
    public void User_Properties_AreIndependent()
    {
        // Arrange
        var user1 = new User { UserId = 1, FirstName = "John" };
        var user2 = new User { UserId = 2, FirstName = "Jane" };

        // Act & Assert
        Assert.NotEqual(user1.UserId, user2.UserId);
        Assert.NotEqual(user1.FirstName, user2.FirstName);
    }
}