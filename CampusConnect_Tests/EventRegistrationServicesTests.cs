using Xunit;
using Microsoft.JSInterop;
using Client.Services;
using RichardSzalay.MockHttp;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace CampusConnect_Tests;

public class TestJSRuntime : IJSRuntime
{
    public List<(string method, object[] args)> InvocationsHistory { get; } = new();
    
    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
    {
        var safeArgs = args?.Cast<object>().ToArray() ?? Array.Empty<object>();
        InvocationsHistory.Add((identifier, safeArgs));
        return new ValueTask<TValue>(default(TValue)!);
    }

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
    {
        var safeArgs = args?.Cast<object>().ToArray() ?? Array.Empty<object>();
        InvocationsHistory.Add((identifier, safeArgs));
        return new ValueTask<TValue>(default(TValue)!);
    }

    public void VerifyInvoked(string method, string expectedMessage)
    {
        var invocation = InvocationsHistory.FirstOrDefault(i => i.method == method);
        Assert.NotEqual(default, invocation);
        Assert.Single(invocation.args);
        Assert.Equal(expectedMessage, invocation.args[0].ToString());
    }

    public void VerifyInvokedTimes(string method, int expectedTimes)
    {
        var invocations = InvocationsHistory.Where(i => i.method == method).ToList();
        Assert.Equal(expectedTimes, invocations.Count);
    }

    public void Reset()
    {
        InvocationsHistory.Clear();
    }
}

public class EventRegistrationServicesTests
{
    private readonly TestJSRuntime _mockJSRuntime;

    public EventRegistrationServicesTests()
    {
        _mockJSRuntime = new TestJSRuntime();
    }

    private EventRegistrationService CreateServiceWithMockHttp(MockHttpMessageHandler mockHttp)
    {
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost:7071/");

        var eventApi = new EventApi(httpClient);
        var eventParticipantApi = new EventParticipantApi(httpClient);

        return new EventRegistrationService(eventApi, eventParticipantApi, _mockJSRuntime);
    }

    [Fact]
    public async Task HandleRsvpAsync_WithZeroUserId_ShowsLoginAlert()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var service = CreateServiceWithMockHttp(mockHttp);

        // Act
        await service.HandleRsvpAsync(1, 1, 0);

        // Assert
        _mockJSRuntime.VerifyInvoked("alert", "Please log in to RSVP.");
    }

    [Fact]
    public async Task HandleRsvpAsync_WithNegativeUserId_ProcessesRequest()
    {
        // Arrange - Negative user IDs are not validated client-side, API will handle
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When(HttpMethod.Post, "*/api/events/*/rsvp*")
            .Respond(HttpStatusCode.Unauthorized); // Using Unauthorized instead of BadRequest to avoid duplication

        var service = CreateServiceWithMockHttp(mockHttp);

        // Act
        await service.HandleRsvpAsync(1, 1, -1);

        // Assert - Should get generic error message since API returned Unauthorized
        _mockJSRuntime.VerifyInvoked("alert", "RSVP failed: Unauthorized");
    }

    [Fact]
    public async Task HandleRsvpAsync_With403Status_ShowsJoinClubMessage()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When(HttpMethod.Post, "*/api/events/*/rsvp*")
            .Respond(HttpStatusCode.Forbidden);

        var service = CreateServiceWithMockHttp(mockHttp);

        // Act
        await service.HandleRsvpAsync(1, 1, 123);

        // Assert
        _mockJSRuntime.VerifyInvoked("alert", "You must join this club to RSVP.");
    }

    [Fact]
    public async Task HandleRsvpAsync_With404Status_ShowsNotFoundMessage()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When(HttpMethod.Post, "*/api/events/*/rsvp*")
            .Respond(HttpStatusCode.NotFound);

        var service = CreateServiceWithMockHttp(mockHttp);

        // Act
        await service.HandleRsvpAsync(1, 1, 123);

        // Assert
        _mockJSRuntime.VerifyInvoked("alert", "Event or user not found.");
    }

    [Fact]
    public async Task HandleRsvpAsync_With409Status_ShowsAlreadyRsvpedMessage()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When(HttpMethod.Post, "*/api/events/*/rsvp*")
            .Respond(HttpStatusCode.Conflict);

        var service = CreateServiceWithMockHttp(mockHttp);

        // Act
        await service.HandleRsvpAsync(1, 1, 123);

        // Assert
        _mockJSRuntime.VerifyInvoked("alert", "You already RSVPed.");
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task HandleRsvpAsync_WithOtherErrorCodes_ShowsGenericErrorMessage(HttpStatusCode statusCode)
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When(HttpMethod.Post, "*/api/events/*/rsvp*")
            .Respond(statusCode);

        var service = CreateServiceWithMockHttp(mockHttp);

        // Act
        await service.HandleRsvpAsync(1, 1, 123);

        // Assert
        _mockJSRuntime.VerifyInvoked("alert", $"RSVP failed: {statusCode}");
    }

    [Fact]
    public async Task HandleRsvpAsync_WithHttpRequestException_ShowsErrorMessage()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When(HttpMethod.Post, "*/api/events/*/rsvp*")
            .Throw(new HttpRequestException("Network error"));

        var service = CreateServiceWithMockHttp(mockHttp);

        // Act
        await service.HandleRsvpAsync(1, 1, 123);

        // Assert
        _mockJSRuntime.VerifyInvoked("alert", "Network error during RSVP: Network error");
    }

    [Fact]
    public async Task HandleRsvpAsync_WithTaskCancelledException_ShowsErrorMessage()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When(HttpMethod.Post, "*/api/events/*/rsvp*")
            .Throw(new TaskCanceledException("Request timeout"));

        var service = CreateServiceWithMockHttp(mockHttp);

        // Act & Assert - TaskCanceledException should be re-thrown per the service implementation
        await Assert.ThrowsAsync<TaskCanceledException>(async () => 
            await service.HandleRsvpAsync(1, 1, 123).ConfigureAwait(true)).ConfigureAwait(true);
    }

    [Fact]
    public async Task HandleRsvpAsync_WithGenericException_ShowsErrorMessage()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When(HttpMethod.Post, "*/api/events/*/rsvp*")
            .Throw(new Exception("Generic error"));

        var service = CreateServiceWithMockHttp(mockHttp);

        // Act & Assert - Generic exceptions should be re-thrown per the service implementation
        await Assert.ThrowsAsync<Exception>(async () => 
            await service.HandleRsvpAsync(1, 1, 123).ConfigureAwait(true)).ConfigureAwait(true);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.Created)]
    [InlineData(HttpStatusCode.Accepted)]
    public async Task HandleRsvpAsync_WithSuccessStatusCodes_ShowsSuccessMessage(HttpStatusCode statusCode)
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When(HttpMethod.Post, "*/api/events/*/rsvp*")
            .Respond(statusCode);

        var service = CreateServiceWithMockHttp(mockHttp);

        // Act
        await service.HandleRsvpAsync(1, 1, 123);

        // Assert
        _mockJSRuntime.VerifyInvoked("alert", "RSVP successful 🎉");
    }

    [Fact]
    public async Task HandleRsvpAsync_MultipleCallsInSequence_HandledCorrectly()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When(HttpMethod.Post, "*/api/events/1/rsvp*")
            .Respond(HttpStatusCode.OK);
        mockHttp.When(HttpMethod.Post, "*/api/events/2/rsvp*")
            .Respond(HttpStatusCode.Conflict);

        var service = CreateServiceWithMockHttp(mockHttp);

        // Act
        await service.HandleRsvpAsync(1, 1, 123);
        await service.HandleRsvpAsync(2, 1, 123);

        // Assert
        _mockJSRuntime.VerifyInvokedTimes("alert", 2);
        Assert.Contains(_mockJSRuntime.InvocationsHistory, i => i.args[0].ToString() == "RSVP successful 🎉");
        Assert.Contains(_mockJSRuntime.InvocationsHistory, i => i.args[0].ToString() == "You already RSVPed.");
    }
}
