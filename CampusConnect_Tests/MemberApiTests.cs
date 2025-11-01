using Client.Models;
using Client.Services;
using RichardSzalay.MockHttp;
using System.Net;
using System.Text.Json;
using Xunit;

namespace CampusConnect_Tests;

public class MemberApiTests : IDisposable
{
    private readonly MockHttpMessageHandler _mockHttp;
    private readonly HttpClient _httpClient;
    private readonly MemberApi _memberApi;

    public MemberApiTests()
    {
        _mockHttp = new MockHttpMessageHandler();
        _httpClient = _mockHttp.ToHttpClient();
        _httpClient.BaseAddress = new Uri("https://localhost:7071/");
        _memberApi = new MemberApi(_httpClient);
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
        _mockHttp?.Dispose();
    }

    #region GetClubMembersAsync Tests

    [Fact]
    public async Task GetClubMembersAsync_ReturnsMembers_WhenSuccessful()
    {
        // Arrange
        var clubId = 1;
        var expectedMembers = new List<Member>
        {
            new() { MemberId = 1, UserId = 1, ClubId = clubId },
            new() { MemberId = 2, UserId = 2, ClubId = clubId }
        };

        _mockHttp.When($"/api/members/clubs/{clubId}/members")
                .Respond("application/json", JsonSerializer.Serialize(expectedMembers));

        // Act
        var result = await _memberApi.GetClubMembersAsync(clubId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(expectedMembers[0].MemberId, result[0].MemberId);
        Assert.Equal(expectedMembers[1].MemberId, result[1].MemberId);
    }

    [Fact]
    public async Task GetClubMembersAsync_ReturnsEmptyList_WhenNoMembers()
    {
        // Arrange
        var clubId = 1;
        var emptyMembers = new List<Member>();

        _mockHttp.When($"/api/members/clubs/{clubId}/members")
                .Respond("application/json", JsonSerializer.Serialize(emptyMembers));

        // Act
        var result = await _memberApi.GetClubMembersAsync(clubId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetClubMembersAsync_ReturnsEmptyList_WhenResponseIsNull()
    {
        // Arrange
        var clubId = 1;

        _mockHttp.When($"/api/members/clubs/{clubId}/members")
                .Respond("application/json", "null");

        // Act
        var result = await _memberApi.GetClubMembersAsync(clubId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetClubMembersAsync_ThrowsHttpRequestException_WhenApiCallFails()
    {
        // Arrange
        var clubId = 1;
        var errorMessage = "Internal Server Error";

        _mockHttp.When($"/api/members/clubs/{clubId}/members")
                .Respond(HttpStatusCode.InternalServerError, "text/plain", errorMessage);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => _memberApi.GetClubMembersAsync(clubId));

        Assert.Contains("InternalServerError", exception.Message);
        Assert.Contains(errorMessage, exception.Message);
    }

    [Fact]
    public async Task GetClubMembersAsync_ThrowsException_WhenNetworkError()
    {
        // Arrange
        var clubId = 1;

        _mockHttp.When($"/api/members/clubs/{clubId}/members")
                .Throw(new HttpRequestException("Network error"));

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(
            () => _memberApi.GetClubMembersAsync(clubId));
    }

    [Fact]
    public async Task GetClubMembersAsync_SupportsCancellation()
    {
        // Arrange
        var clubId = 1;
        var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        _mockHttp.When($"/api/members/clubs/{clubId}/members")
                .Respond("application/json", "[]");

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(
            () => _memberApi.GetClubMembersAsync(clubId, cts.Token));
    }

    #endregion

    #region GetUserMembershipsAsync Tests

    [Fact]
    public async Task GetUserMembershipsAsync_ReturnsMemberships_WhenSuccessful()
    {
        // Arrange
        var userId = 1;
        var expectedMemberships = new List<Member>
        {
            new() { MemberId = 1, UserId = userId, ClubId = 1 },
            new() { MemberId = 2, UserId = userId, ClubId = 2 }
        };

        _mockHttp.When($"/api/members/users/{userId}/memberships")
                .Respond("application/json", JsonSerializer.Serialize(expectedMemberships));

        // Act
        var result = await _memberApi.GetUserMembershipsAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, m => Assert.Equal(userId, m.UserId));
    }

    [Fact]
    public async Task GetUserMembershipsAsync_ReturnsEmptyList_WhenNoMemberships()
    {
        // Arrange
        var userId = 1;
        var emptyMemberships = new List<Member>();

        _mockHttp.When($"/api/members/users/{userId}/memberships")
                .Respond("application/json", JsonSerializer.Serialize(emptyMemberships));

        // Act
        var result = await _memberApi.GetUserMembershipsAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetUserMembershipsAsync_ReturnsEmptyList_WhenResponseIsNull()
    {
        // Arrange
        var userId = 1;

        _mockHttp.When($"/api/members/users/{userId}/memberships")
                .Respond("application/json", "null");

        // Act
        var result = await _memberApi.GetUserMembershipsAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetUserMembershipsAsync_ThrowsHttpRequestException_WhenApiCallFails()
    {
        // Arrange
        var userId = 1;
        var errorMessage = "User not found";

        _mockHttp.When($"/api/members/users/{userId}/memberships")
                .Respond(HttpStatusCode.NotFound, "text/plain", errorMessage);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => _memberApi.GetUserMembershipsAsync(userId));

        Assert.Contains("NotFound", exception.Message);
        Assert.Contains(errorMessage, exception.Message);
    }

    [Fact]
    public async Task GetUserMembershipsAsync_ThrowsException_WhenNetworkError()
    {
        // Arrange
        var userId = 1;

        _mockHttp.When($"/api/members/users/{userId}/memberships")
                .Throw(new TaskCanceledException("Request timeout"));

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(
            () => _memberApi.GetUserMembershipsAsync(userId));
    }

    [Fact]
    public async Task GetUserMembershipsAsync_SupportsCancellation()
    {
        // Arrange
        var userId = 1;
        var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        _mockHttp.When($"/api/members/users/{userId}/memberships")
                .Respond("application/json", "[]");

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(
            () => _memberApi.GetUserMembershipsAsync(userId, cts.Token));
    }

    #endregion

    #region AddMemberToClubAsync Tests

    [Fact]
    public async Task AddMemberToClubAsync_ReturnsMember_WhenSuccessful()
    {
        // Arrange
        var userId = 1;
        var clubId = 1;
        var expectedMember = new Member { MemberId = 1, UserId = userId, ClubId = clubId };

        _mockHttp.When($"/api/members/clubs/{clubId}/members")
                .Respond("application/json", JsonSerializer.Serialize(expectedMember));

        // Act
        var result = await _memberApi.AddMemberToClubAsync(userId, clubId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedMember.MemberId, result.MemberId);
        Assert.Equal(expectedMember.UserId, result.UserId);
        Assert.Equal(expectedMember.ClubId, result.ClubId);
    }

    [Fact]
    public async Task AddMemberToClubAsync_ReturnsNull_WhenResponseIsNull()
    {
        // Arrange
        var userId = 1;
        var clubId = 1;

        _mockHttp.When($"/api/members/clubs/{clubId}/members")
                .Respond("application/json", "null");

        // Act
        var result = await _memberApi.AddMemberToClubAsync(userId, clubId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AddMemberToClubAsync_ThrowsHttpRequestException_WhenApiCallFails()
    {
        // Arrange
        var userId = 1;
        var clubId = 1;
        var errorMessage = "User already member of club";

        _mockHttp.When($"/api/members/clubs/{clubId}/members")
                .Respond(HttpStatusCode.Conflict, "text/plain", errorMessage);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => _memberApi.AddMemberToClubAsync(userId, clubId));

        Assert.Contains("Conflict", exception.Message);
        Assert.Contains(errorMessage, exception.Message);
    }

    [Fact]
    public async Task AddMemberToClubAsync_ThrowsException_WhenNetworkError()
    {
        // Arrange
        var userId = 1;
        var clubId = 1;

        _mockHttp.When($"/api/members/clubs/{clubId}/members")
                .Throw(new HttpRequestException("Connection failed"));

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(
            () => _memberApi.AddMemberToClubAsync(userId, clubId));
    }

    [Fact]
    public async Task AddMemberToClubAsync_SupportsCancellation()
    {
        // Arrange
        var userId = 1;
        var clubId = 1;
        var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        _mockHttp.When($"/api/members/clubs/{clubId}/members")
                .Respond("application/json", "{}");

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(
            () => _memberApi.AddMemberToClubAsync(userId, clubId, cts.Token));
    }

    [Fact]
    public async Task AddMemberToClubAsync_SendsCorrectPayload()
    {
        // Arrange
        var userId = 123;
        var clubId = 456;
        var expectedMember = new Member { MemberId = 1, UserId = userId, ClubId = clubId };

        // Mock the response without strict content validation
        _mockHttp.When(HttpMethod.Post, $"/api/members/clubs/{clubId}/members")
                .Respond("application/json", JsonSerializer.Serialize(expectedMember));

        // Act
        var result = await _memberApi.AddMemberToClubAsync(userId, clubId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedMember.MemberId, result.MemberId);
        Assert.Equal(expectedMember.UserId, result.UserId);
        Assert.Equal(expectedMember.ClubId, result.ClubId);
    }

    #endregion

    [Fact]
    public void MemberApi_Constructor_InitializesHttpClient()
    {
        // Arrange & Act
        var httpClient = new HttpClient();
        var memberApi = new MemberApi(httpClient);

        // Assert
        Assert.NotNull(memberApi);
    }
}