using Microsoft.Extensions.DependencyInjection;
using Client.Services;
using Client.Models;
using RichardSzalay.MockHttp;
using Xunit;
using System.Net.Http;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Collections.Generic;

namespace CampusConnect_Tests;

public class EventParticipantApiTests
{
    private EventParticipantApi CreateApiWithMockHttp(MockHttpMessageHandler mockHttp)
    {
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost:7071/");
        return new EventParticipantApi(httpClient);
    }

    #region GetActiveEventParticipantsAsync Tests

    [Fact]
    public async Task GetActiveEventParticipantsAsync_Success_ReturnsEventParticipants()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var expectedParticipants = new List<EventParticipant>
        {
            new EventParticipant { MemberId = 1, EventId = 100, ClubId = 10 },
            new EventParticipant { MemberId = 2, EventId = 101, ClubId = 11 }
        };

        mockHttp.When("*/api/eventparticipants")
            .Respond("application/json", JsonSerializer.Serialize(expectedParticipants));

        var api = CreateApiWithMockHttp(mockHttp);

        // Act
        var result = await api.GetActiveEventParticipantsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(1, result[0].MemberId);
        Assert.Equal(100, result[0].EventId);
        Assert.Equal(10, result[0].ClubId);
        Assert.Equal(2, result[1].MemberId);
        Assert.Equal(101, result[1].EventId);
        Assert.Equal(11, result[1].ClubId);
    }

    [Fact]
    public async Task GetActiveEventParticipantsAsync_EmptyResponse_ReturnsEmptyList()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("*/api/eventparticipants")
            .Respond("application/json", "[]");

        var api = CreateApiWithMockHttp(mockHttp);

        // Act
        var result = await api.GetActiveEventParticipantsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetActiveEventParticipantsAsync_NullResponse_ReturnsEmptyList()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("*/api/eventparticipants")
            .Respond("application/json", "null");

        var api = CreateApiWithMockHttp(mockHttp);

        // Act
        var result = await api.GetActiveEventParticipantsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetActiveEventParticipantsAsync_HttpError_ThrowsHttpRequestException()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("*/api/eventparticipants")
            .Respond(HttpStatusCode.InternalServerError, "text/plain", "Server error");

        var api = CreateApiWithMockHttp(mockHttp);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => api.GetActiveEventParticipantsAsync());

        Assert.Contains("InternalServerError", exception.Message);
        Assert.Contains("Server error", exception.Message);
    }

    [Fact]
    public async Task GetActiveEventParticipantsAsync_Unauthorized_ThrowsHttpRequestException()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("*/api/eventparticipants")
            .Respond(HttpStatusCode.Unauthorized, "text/plain", "Unauthorized access");

        var api = CreateApiWithMockHttp(mockHttp);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => api.GetActiveEventParticipantsAsync());

        Assert.Contains("Unauthorized", exception.Message);
        Assert.Contains("Unauthorized access", exception.Message);
    }

    [Fact]
    public async Task GetActiveEventParticipantsAsync_NetworkException_ThrowsOriginalException()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("*/api/eventparticipants")
            .Throw(new HttpRequestException("Connection refused"));

        var api = CreateApiWithMockHttp(mockHttp);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => api.GetActiveEventParticipantsAsync());

        Assert.Contains("Connection refused", exception.Message);
    }

    [Fact]
    public async Task GetActiveEventParticipantsAsync_TaskCancelled_ThrowsTaskCancelledException()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("*/api/eventparticipants")
            .Throw(new TaskCanceledException("Request timeout"));

        var api = CreateApiWithMockHttp(mockHttp);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TaskCanceledException>(
            () => api.GetActiveEventParticipantsAsync());

        Assert.Contains("Request timeout", exception.Message);
    }

    [Fact]
    public async Task GetActiveEventParticipantsAsync_WithCancellationToken_Success()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var expectedParticipants = new List<EventParticipant>
        {
            new EventParticipant { MemberId = 1, EventId = 100, ClubId = 10 }
        };

        mockHttp.When("*/api/eventparticipants")
            .Respond("application/json", JsonSerializer.Serialize(expectedParticipants));

        var api = CreateApiWithMockHttp(mockHttp);
        using var cts = new CancellationTokenSource();

        // Act
        var result = await api.GetActiveEventParticipantsAsync(cts.Token);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(1, result[0].MemberId);
    }

    [Fact]
    public async Task GetActiveEventParticipantsAsync_CancelledToken_ThrowsOperationCancelledException()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("*/api/eventparticipants")
            .Respond("application/json", "[]");

        var api = CreateApiWithMockHttp(mockHttp);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => api.GetActiveEventParticipantsAsync(cts.Token));
    }

    #endregion

    #region GetUserByMemberIdAsync Tests

    [Fact]
    public async Task GetUserByMemberIdAsync_Success_ReturnsUserId()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var expectedUserId = 123;
        var memberId = 456;

        mockHttp.When($"*/api/eventparticipants/member/{memberId}")
            .Respond("application/json", JsonSerializer.Serialize(expectedUserId));

        var api = CreateApiWithMockHttp(mockHttp);

        // Act
        var result = await api.GetUserByMemberIdAsync(memberId);

        // Assert
        Assert.Equal(expectedUserId, result);
    }

    [Fact]
    public async Task GetUserByMemberIdAsync_NullResponse_ReturnsNull()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var memberId = 456;

        mockHttp.When($"*/api/eventparticipants/member/{memberId}")
            .Respond("application/json", "null");

        var api = CreateApiWithMockHttp(mockHttp);

        // Act
        var result = await api.GetUserByMemberIdAsync(memberId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserByMemberIdAsync_NotFound_ThrowsHttpRequestException()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var memberId = 999;

        mockHttp.When($"*/api/eventparticipants/member/{memberId}")
            .Respond(HttpStatusCode.NotFound, "text/plain", "Member not found");

        var api = CreateApiWithMockHttp(mockHttp);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => api.GetUserByMemberIdAsync(memberId));

        Assert.Contains("NotFound", exception.Message);
        Assert.Contains("Member not found", exception.Message);
    }

    [Fact]
    public async Task GetUserByMemberIdAsync_BadRequest_ThrowsHttpRequestException()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var memberId = -1;

        mockHttp.When($"*/api/eventparticipants/member/{memberId}")
            .Respond(HttpStatusCode.BadRequest, "text/plain", "Invalid member ID");

        var api = CreateApiWithMockHttp(mockHttp);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => api.GetUserByMemberIdAsync(memberId));

        Assert.Contains("BadRequest", exception.Message);
        Assert.Contains("Invalid member ID", exception.Message);
    }

    [Fact]
    public async Task GetUserByMemberIdAsync_NetworkException_ThrowsOriginalException()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var memberId = 123;

        mockHttp.When($"*/api/eventparticipants/member/{memberId}")
            .Throw(new HttpRequestException("Network timeout"));

        var api = CreateApiWithMockHttp(mockHttp);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => api.GetUserByMemberIdAsync(memberId));

        Assert.Contains("Network timeout", exception.Message);
    }

    [Fact]
    public async Task GetUserByMemberIdAsync_WithInnerException_ThrowsWithInnerException()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var memberId = 123;
        var innerException = new InvalidOperationException("Inner error");

        mockHttp.When($"*/api/eventparticipants/member/{memberId}")
            .Throw(new HttpRequestException("Outer error", innerException));

        var api = CreateApiWithMockHttp(mockHttp);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => api.GetUserByMemberIdAsync(memberId));

        Assert.Contains("Outer error", exception.Message);
        Assert.NotNull(exception.InnerException);
        Assert.Contains("Inner error", exception.InnerException.Message);
    }

    [Fact]
    public async Task GetUserByMemberIdAsync_WithCancellationToken_Success()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var expectedUserId = 789;
        var memberId = 456;

        mockHttp.When($"*/api/eventparticipants/member/{memberId}")
            .Respond("application/json", JsonSerializer.Serialize(expectedUserId));

        var api = CreateApiWithMockHttp(mockHttp);
        using var cts = new CancellationTokenSource();

        // Act
        var result = await api.GetUserByMemberIdAsync(memberId, cts.Token);

        // Assert
        Assert.Equal(expectedUserId, result);
    }

    #endregion

    #region GetMemberIdsByUserIdAsync Tests

    [Fact]
    public async Task GetMemberIdsByUserIdAsync_Success_ReturnsMemberIds()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var expectedMemberIds = new List<int> { 1, 2, 3, 4 };
        var userId = 123;

        mockHttp.When($"*/api/eventparticipants/user/{userId}")
            .Respond("application/json", JsonSerializer.Serialize(expectedMemberIds));

        var api = CreateApiWithMockHttp(mockHttp);

        // Act
        var result = await api.GetMemberIdsByUserIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(4, result.Count);
        Assert.Equal(expectedMemberIds, result);
    }

    [Fact]
    public async Task GetMemberIdsByUserIdAsync_EmptyResponse_ReturnsEmptyList()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var userId = 123;

        mockHttp.When($"*/api/eventparticipants/user/{userId}")
            .Respond("application/json", "[]");

        var api = CreateApiWithMockHttp(mockHttp);

        // Act
        var result = await api.GetMemberIdsByUserIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetMemberIdsByUserIdAsync_NullResponse_ReturnsEmptyList()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var userId = 123;

        mockHttp.When($"*/api/eventparticipants/user/{userId}")
            .Respond("application/json", "null");

        var api = CreateApiWithMockHttp(mockHttp);

        // Act
        var result = await api.GetMemberIdsByUserIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetMemberIdsByUserIdAsync_SingleMemberId_ReturnsListWithOne()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var expectedMemberIds = new List<int> { 42 };
        var userId = 123;

        mockHttp.When($"*/api/eventparticipants/user/{userId}")
            .Respond("application/json", JsonSerializer.Serialize(expectedMemberIds));

        var api = CreateApiWithMockHttp(mockHttp);

        // Act
        var result = await api.GetMemberIdsByUserIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(42, result[0]);
    }

    [Fact]
    public async Task GetMemberIdsByUserIdAsync_Forbidden_ThrowsHttpRequestException()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var userId = 123;

        mockHttp.When($"*/api/eventparticipants/user/{userId}")
            .Respond(HttpStatusCode.Forbidden, "text/plain", "Access denied");

        var api = CreateApiWithMockHttp(mockHttp);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => api.GetMemberIdsByUserIdAsync(userId));

        Assert.Contains("Forbidden", exception.Message);
        Assert.Contains("Access denied", exception.Message);
    }

    [Fact]
    public async Task GetMemberIdsByUserIdAsync_GeneralException_ThrowsOriginalException()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var userId = 123;

        mockHttp.When($"*/api/eventparticipants/user/{userId}")
            .Throw(new InvalidOperationException("General error"));

        var api = CreateApiWithMockHttp(mockHttp);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => api.GetMemberIdsByUserIdAsync(userId));

        Assert.Contains("General error", exception.Message);
    }

    #endregion

    #region GetEventParticipantByIdAsync Tests

    [Fact]
    public async Task GetEventParticipantByIdAsync_Success_ReturnsEventParticipant()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var expectedParticipant = new EventParticipant { MemberId = 10, EventId = 100, ClubId = 5 };
        var eventId = 100;

        mockHttp.When($"*/api/eventparticipants/{eventId}")
            .Respond("application/json", JsonSerializer.Serialize(expectedParticipant));

        var api = CreateApiWithMockHttp(mockHttp);

        // Act
        var result = await api.GetEventParticipantByIdAsync(eventId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.MemberId);
        Assert.Equal(100, result.EventId);
        Assert.Equal(5, result.ClubId);
    }

    [Fact]
    public async Task GetEventParticipantByIdAsync_NotFound_ThrowsHttpRequestException()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var eventId = 999;

        mockHttp.When($"*/api/eventparticipants/{eventId}")
            .Respond(HttpStatusCode.NotFound, "text/plain", "Event participant not found");

        var api = CreateApiWithMockHttp(mockHttp);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => api.GetEventParticipantByIdAsync(eventId));

        Assert.Contains("NotFound", exception.Message);
        Assert.Contains("Event participant not found", exception.Message);
    }

    [Fact]
    public async Task GetEventParticipantByIdAsync_NullResponse_ReturnsNull()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var eventId = 100;

        mockHttp.When($"*/api/eventparticipants/{eventId}")
            .Respond("application/json", "null");

        var api = CreateApiWithMockHttp(mockHttp);

        // Act
        var result = await api.GetEventParticipantByIdAsync(eventId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetEventParticipantByIdAsync_InvalidJson_ThrowsJsonException()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var eventId = 100;

        mockHttp.When($"*/api/eventparticipants/{eventId}")
            .Respond("application/json", "{ invalid json");

        var api = CreateApiWithMockHttp(mockHttp);

        // Act & Assert
        await Assert.ThrowsAsync<JsonException>(
            () => api.GetEventParticipantByIdAsync(eventId));
    }

    [Fact]
    public async Task GetEventParticipantByIdAsync_TaskCancelled_ThrowsTaskCancelledException()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var eventId = 100;

        mockHttp.When($"*/api/eventparticipants/{eventId}")
            .Throw(new TaskCanceledException("Operation was cancelled"));

        var api = CreateApiWithMockHttp(mockHttp);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TaskCanceledException>(
            () => api.GetEventParticipantByIdAsync(eventId));

        Assert.Contains("Operation was cancelled", exception.Message);
    }

    #endregion

    #region GetMemberIdByUserIdAsync Tests

    [Fact]
    public async Task GetMemberIdByUserIdAsync_Success_ReturnsMemberId()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var expectedMemberId = 456;
        var userId = 123;

        mockHttp.When($"*/api/eventparticipants/member/user/{userId}")
            .Respond("application/json", JsonSerializer.Serialize(expectedMemberId));

        var api = CreateApiWithMockHttp(mockHttp);

        // Act
        var result = await api.GetMemberIdByUserIdAsync(userId);

        // Assert
        Assert.Equal(expectedMemberId, result);
    }

    [Fact]
    public async Task GetMemberIdByUserIdAsync_ZeroValue_ReturnsZero()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var expectedMemberId = 0;
        var userId = 123;

        mockHttp.When($"*/api/eventparticipants/member/user/{userId}")
            .Respond("application/json", JsonSerializer.Serialize(expectedMemberId));

        var api = CreateApiWithMockHttp(mockHttp);

        // Act
        var result = await api.GetMemberIdByUserIdAsync(userId);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public async Task GetMemberIdByUserIdAsync_NegativeValue_ReturnsNegativeValue()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var expectedMemberId = -1;
        var userId = 123;

        mockHttp.When($"*/api/eventparticipants/member/user/{userId}")
            .Respond("application/json", JsonSerializer.Serialize(expectedMemberId));

        var api = CreateApiWithMockHttp(mockHttp);

        // Act
        var result = await api.GetMemberIdByUserIdAsync(userId);

        // Assert
        Assert.Equal(-1, result);
    }

    [Fact]
    public async Task GetMemberIdByUserIdAsync_ServiceUnavailable_ThrowsHttpRequestException()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var userId = 123;

        mockHttp.When($"*/api/eventparticipants/member/user/{userId}")
            .Respond(HttpStatusCode.ServiceUnavailable, "text/plain", "Service temporarily unavailable");

        var api = CreateApiWithMockHttp(mockHttp);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => api.GetMemberIdByUserIdAsync(userId));

        Assert.Contains("ServiceUnavailable", exception.Message);
        Assert.Contains("Service temporarily unavailable", exception.Message);
    }

    [Fact]
    public async Task GetMemberIdByUserIdAsync_ArgumentException_ThrowsOriginalException()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var userId = 123;

        mockHttp.When($"*/api/eventparticipants/member/user/{userId}")
            .Throw(new ArgumentException("Invalid argument"));

        var api = CreateApiWithMockHttp(mockHttp);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => api.GetMemberIdByUserIdAsync(userId));

        Assert.Contains("Invalid argument", exception.Message);
    }

    #endregion

    #region RegisterForEventAsync Tests

    [Fact]
    public async Task RegisterForEventAsync_Success_ReturnsEventParticipant()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var registrationRequest = new { MemberId = 10, EventId = 100, ClubId = 5 };
        var expectedParticipant = new EventParticipant { MemberId = 10, EventId = 100, ClubId = 5 };

        mockHttp.When("*/api/eventparticipants/register")
            .Respond("application/json", JsonSerializer.Serialize(expectedParticipant));

        var api = CreateApiWithMockHttp(mockHttp);

        // Act
        var result = await api.RegisterForEventAsync(registrationRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.MemberId);
        Assert.Equal(100, result.EventId);
        Assert.Equal(5, result.ClubId);
    }

    [Fact]
    public async Task RegisterForEventAsync_NullResponse_ReturnsNull()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var registrationRequest = new { MemberId = 10, EventId = 100, ClubId = 5 };

        mockHttp.When("*/api/eventparticipants/register")
            .Respond("application/json", "null");

        var api = CreateApiWithMockHttp(mockHttp);

        // Act
        var result = await api.RegisterForEventAsync(registrationRequest);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task RegisterForEventAsync_EmptyRequest_Success()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var registrationRequest = new { };
        var expectedParticipant = new EventParticipant { MemberId = 0, EventId = 0, ClubId = 0 };

        mockHttp.When("*/api/eventparticipants/register")
            .Respond("application/json", JsonSerializer.Serialize(expectedParticipant));

        var api = CreateApiWithMockHttp(mockHttp);

        // Act
        var result = await api.RegisterForEventAsync(registrationRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.MemberId);
        Assert.Equal(0, result.EventId);
        Assert.Equal(0, result.ClubId);
    }

    [Fact]
    public async Task RegisterForEventAsync_StringRequest_Success()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var registrationRequest = "string request";
        var expectedParticipant = new EventParticipant { MemberId = 1, EventId = 2, ClubId = 3 };

        mockHttp.When("*/api/eventparticipants/register")
            .Respond("application/json", JsonSerializer.Serialize(expectedParticipant));

        var api = CreateApiWithMockHttp(mockHttp);

        // Act
        var result = await api.RegisterForEventAsync(registrationRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.MemberId);
        Assert.Equal(2, result.EventId);
        Assert.Equal(3, result.ClubId);
    }

    [Fact]
    public async Task RegisterForEventAsync_NullRequest_Success()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        object? registrationRequest = null;
        var expectedParticipant = new EventParticipant { MemberId = 1, EventId = 2, ClubId = 3 };

        mockHttp.When("*/api/eventparticipants/register")
            .Respond("application/json", JsonSerializer.Serialize(expectedParticipant));

        var api = CreateApiWithMockHttp(mockHttp);

        // Act
        var result = await api.RegisterForEventAsync(registrationRequest!);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.MemberId);
        Assert.Equal(2, result.EventId);
        Assert.Equal(3, result.ClubId);
    }

    [Fact]
    public async Task RegisterForEventAsync_Conflict_ThrowsHttpRequestException()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var registrationRequest = new { MemberId = 10, EventId = 100, ClubId = 5 };

        mockHttp.When("*/api/eventparticipants/register")
            .Respond(HttpStatusCode.Conflict, "text/plain", "Already registered for this event");

        var api = CreateApiWithMockHttp(mockHttp);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => api.RegisterForEventAsync(registrationRequest));

        Assert.Contains("Conflict", exception.Message);
        Assert.Contains("Already registered for this event", exception.Message);
    }

    [Fact]
    public async Task RegisterForEventAsync_BadRequest_ThrowsHttpRequestException()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var registrationRequest = new { InvalidProperty = "invalid" };

        mockHttp.When("*/api/eventparticipants/register")
            .Respond(HttpStatusCode.BadRequest, "text/plain", "Invalid registration data");

        var api = CreateApiWithMockHttp(mockHttp);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => api.RegisterForEventAsync(registrationRequest));

        Assert.Contains("BadRequest", exception.Message);
        Assert.Contains("Invalid registration data", exception.Message);
    }

    [Fact]
    public async Task RegisterForEventAsync_InternalServerError_ThrowsHttpRequestException()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var registrationRequest = new { MemberId = 10, EventId = 100, ClubId = 5 };

        mockHttp.When("*/api/eventparticipants/register")
            .Respond(HttpStatusCode.InternalServerError, "text/plain", "Database connection failed");

        var api = CreateApiWithMockHttp(mockHttp);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => api.RegisterForEventAsync(registrationRequest));

        Assert.Contains("InternalServerError", exception.Message);
        Assert.Contains("Database connection failed", exception.Message);
    }

    [Fact]
    public async Task RegisterForEventAsync_NetworkException_ThrowsOriginalException()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var registrationRequest = new { MemberId = 10, EventId = 100, ClubId = 5 };

        mockHttp.When("*/api/eventparticipants/register")
            .Throw(new HttpRequestException("Network error occurred"));

        var api = CreateApiWithMockHttp(mockHttp);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => api.RegisterForEventAsync(registrationRequest));

        Assert.Contains("Network error occurred", exception.Message);
    }

    [Fact]
    public async Task RegisterForEventAsync_TimeoutException_ThrowsOriginalException()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var registrationRequest = new { MemberId = 10, EventId = 100, ClubId = 5 };

        mockHttp.When("*/api/eventparticipants/register")
            .Throw(new TimeoutException("Request timed out"));

        var api = CreateApiWithMockHttp(mockHttp);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TimeoutException>(
            () => api.RegisterForEventAsync(registrationRequest));

        Assert.Contains("Request timed out", exception.Message);
    }

    [Fact]
    public async Task RegisterForEventAsync_WithCancellationToken_Success()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var registrationRequest = new { MemberId = 10, EventId = 100, ClubId = 5 };
        var expectedParticipant = new EventParticipant { MemberId = 10, EventId = 100, ClubId = 5 };

        mockHttp.When("*/api/eventparticipants/register")
            .Respond("application/json", JsonSerializer.Serialize(expectedParticipant));

        var api = CreateApiWithMockHttp(mockHttp);
        using var cts = new CancellationTokenSource();

        // Act
        var result = await api.RegisterForEventAsync(registrationRequest, cts.Token);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.MemberId);
        Assert.Equal(100, result.EventId);
        Assert.Equal(5, result.ClubId);
    }

    [Fact]
    public async Task RegisterForEventAsync_CancelledToken_ThrowsOperationCancelledException()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var registrationRequest = new { MemberId = 10, EventId = 100, ClubId = 5 };

        mockHttp.When("*/api/eventparticipants/register")
            .Respond("application/json", "{}");

        var api = CreateApiWithMockHttp(mockHttp);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => api.RegisterForEventAsync(registrationRequest, cts.Token));
    }

    #endregion

    #region Edge Cases and Additional Coverage Tests

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(999999)]
    [InlineData(-1)]
    public async Task GetUserByMemberIdAsync_VariousMemberIds_HandlesCorrectly(int memberId)
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var expectedUserId = memberId * 10;

        mockHttp.When($"*/api/eventparticipants/member/{memberId}")
            .Respond("application/json", JsonSerializer.Serialize(expectedUserId));

        var api = CreateApiWithMockHttp(mockHttp);

        // Act
        var result = await api.GetUserByMemberIdAsync(memberId);

        // Assert
        Assert.Equal(expectedUserId, result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(999999)]
    [InlineData(-1)]
    public async Task GetMemberIdsByUserIdAsync_VariousUserIds_HandlesCorrectly(int userId)
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var expectedMemberIds = new List<int> { userId, userId + 1, userId + 2 };

        mockHttp.When($"*/api/eventparticipants/user/{userId}")
            .Respond("application/json", JsonSerializer.Serialize(expectedMemberIds));

        var api = CreateApiWithMockHttp(mockHttp);

        // Act
        var result = await api.GetMemberIdsByUserIdAsync(userId);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(userId, result[0]);
        Assert.Equal(userId + 1, result[1]);
        Assert.Equal(userId + 2, result[2]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(999999)]
    [InlineData(-1)]
    public async Task GetEventParticipantByIdAsync_VariousEventIds_HandlesCorrectly(int eventId)
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var expectedParticipant = new EventParticipant 
        { 
            MemberId = eventId + 10, 
            EventId = eventId, 
            ClubId = eventId + 20 
        };

        mockHttp.When($"*/api/eventparticipants/{eventId}")
            .Respond("application/json", JsonSerializer.Serialize(expectedParticipant));

        var api = CreateApiWithMockHttp(mockHttp);

        // Act
        var result = await api.GetEventParticipantByIdAsync(eventId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(eventId + 10, result.MemberId);
        Assert.Equal(eventId, result.EventId);
        Assert.Equal(eventId + 20, result.ClubId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(999999)]
    [InlineData(-1)]
    public async Task GetMemberIdByUserIdAsync_VariousUserIds_HandlesCorrectly(int userId)
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var expectedMemberId = userId + 100;

        mockHttp.When($"*/api/eventparticipants/member/user/{userId}")
            .Respond("application/json", JsonSerializer.Serialize(expectedMemberId));

        var api = CreateApiWithMockHttp(mockHttp);

        // Act
        var result = await api.GetMemberIdByUserIdAsync(userId);

        // Assert
        Assert.Equal(expectedMemberId, result);
    }

    [Fact]
    public async Task RegisterForEventAsync_ComplexObjectRequest_Success()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var registrationRequest = new 
        { 
            MemberId = 10, 
            EventId = 100, 
            ClubId = 5,
            ExtraData = new { Field1 = "value1", Field2 = 42 },
            Tags = new[] { "tag1", "tag2" }
        };
        var expectedParticipant = new EventParticipant { MemberId = 10, EventId = 100, ClubId = 5 };

        mockHttp.When("*/api/eventparticipants/register")
            .Respond("application/json", JsonSerializer.Serialize(expectedParticipant));

        var api = CreateApiWithMockHttp(mockHttp);

        // Act
        var result = await api.RegisterForEventAsync(registrationRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.MemberId);
        Assert.Equal(100, result.EventId);
        Assert.Equal(5, result.ClubId);
    }

    [Fact]
    public async Task GetActiveEventParticipantsAsync_LargeDataset_HandlesCorrectly()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var expectedParticipants = new List<EventParticipant>();
        
        // Create 1000 event participants
        for (int i = 0; i < 1000; i++)
        {
            expectedParticipants.Add(new EventParticipant 
            { 
                MemberId = i + 1, 
                EventId = i + 100, 
                ClubId = i + 200 
            });
        }

        mockHttp.When("*/api/eventparticipants")
            .Respond("application/json", JsonSerializer.Serialize(expectedParticipants));

        var api = CreateApiWithMockHttp(mockHttp);

        // Act
        var result = await api.GetActiveEventParticipantsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1000, result.Count);
        Assert.Equal(1, result[0].MemberId);
        Assert.Equal(1000, result[999].MemberId);
    }

    #endregion
}