using Client.Models;
using Xunit;

namespace CampusConnect_Tests;

public class MemberTests
{
    [Fact]
    public void Member_Properties_CanBeSetAndGet()
    {
        // Arrange
        var member = new Member();

        // Act
        member.MemberId = 123;
        member.UserId = 456;
        member.ClubId = 789;

        // Assert
        Assert.Equal(123, member.MemberId);
        Assert.Equal(456, member.UserId);
        Assert.Equal(789, member.ClubId);
    }

    [Fact]
    public void Member_DefaultValues_AreZero()
    {
        // Arrange & Act
        var member = new Member();

        // Assert
        Assert.Equal(0, member.MemberId);
        Assert.Equal(0, member.UserId);
        Assert.Equal(0, member.ClubId);
    }

    [Fact]
    public void Member_CanCreateWithInitializer()
    {
        // Arrange & Act
        var member = new Member
        {
            MemberId = 100,
            UserId = 200,
            ClubId = 300
        };

        // Assert
        Assert.Equal(100, member.MemberId);
        Assert.Equal(200, member.UserId);
        Assert.Equal(300, member.ClubId);
    }

    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(999, 888, 777)]
    [InlineData(0, 0, 0)]
    [InlineData(-1, -2, -3)] // Edge case: negative IDs
    public void Member_Properties_AcceptVariousValues(int memberId, int userId, int clubId)
    {
        // Arrange & Act
        var member = new Member
        {
            MemberId = memberId,
            UserId = userId,
            ClubId = clubId
        };

        // Assert
        Assert.Equal(memberId, member.MemberId);
        Assert.Equal(userId, member.UserId);
        Assert.Equal(clubId, member.ClubId);
    }

    [Fact]
    public void Member_Properties_AreIndependent()
    {
        // Arrange
        var member1 = new Member { MemberId = 1, UserId = 10, ClubId = 100 };
        var member2 = new Member { MemberId = 2, UserId = 20, ClubId = 200 };

        // Act & Assert
        Assert.NotEqual(member1.MemberId, member2.MemberId);
        Assert.NotEqual(member1.UserId, member2.UserId);
        Assert.NotEqual(member1.ClubId, member2.ClubId);
    }

    [Fact]
    public void Member_PropertiesAreIntegerType()
    {
        // Arrange
        var member = new Member();

        // Act & Assert
        Assert.IsType<int>(member.MemberId);
        Assert.IsType<int>(member.UserId);
        Assert.IsType<int>(member.ClubId);
    }

    [Fact]
    public void Member_CanBeUsedInCollections()
    {
        // Arrange
        var members = new List<Member>
        {
            new() { MemberId = 1, UserId = 10, ClubId = 100 },
            new() { MemberId = 2, UserId = 20, ClubId = 100 },
            new() { MemberId = 3, UserId = 30, ClubId = 200 }
        };

        // Act & Assert
        Assert.Equal(3, members.Count);
        Assert.All(members, m => Assert.True(m.MemberId > 0));
        
        // Test LINQ operations work
        var club100Members = members.Where(m => m.ClubId == 100).ToList();
        Assert.Equal(2, club100Members.Count);
    }

    [Fact]
    public void Member_SupportsPropertyModification()
    {
        // Arrange
        var member = new Member { MemberId = 1, UserId = 10, ClubId = 100 };

        // Act
        member.UserId = 999;
        member.ClubId = 888;

        // Assert
        Assert.Equal(1, member.MemberId); // Unchanged
        Assert.Equal(999, member.UserId); // Changed
        Assert.Equal(888, member.ClubId); // Changed
    }
}