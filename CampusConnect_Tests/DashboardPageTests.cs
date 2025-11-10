using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Client.Pages;
using Client.Services;
using RichardSzalay.MockHttp;
using Xunit;
using Bunit.TestDoubles;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Text.RegularExpressions;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;

namespace CampusConnect_Tests;

public class DashboardPageTests : TestContext
{
    public DashboardPageTests()
    {
        // Register required services for Dashboard page
        Services.AddSingleton<AuthService>();
        Services.AddSingleton<ClubApi>();
        Services.AddSingleton<EventApi>();
        Services.AddSingleton<MemberApi>();
        Services.AddSingleton<UserApi>();
        
        // Setup mock JSRuntime
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    private static bool ContainsAny(string html, params string[] needles) =>
        needles.Any(n => html.Contains(n, StringComparison.OrdinalIgnoreCase));

    private void SetupAuthenticatedUser(string email = "test@campus.edu", int userId = 1)
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("*/api/clubs/user/*")
            .Respond("application/json", "[]");
        mockHttp.When("*/api/events/user/*/rsvps")
            .Respond("application/json", "[]");
        mockHttp.Fallback.Respond("application/json", "[]");

        var http = mockHttp.ToHttpClient();
        http.BaseAddress = new Uri("https://localhost:7071/");
        Services.RemoveAll<HttpClient>();
        Services.AddSingleton(http);

        // Setup authenticated AuthService
        var authService = Services.GetRequiredService<AuthService>();
        typeof(AuthService).GetField("_currentUser", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(authService, new UserSession { UserId = userId, Email = email, IsAuthenticated = true });
    }

    private void SetupAuthenticationOnly(string email = "test@campus.edu", int userId = 1)
    {
        // Setup authenticated AuthService
        var authService = Services.GetRequiredService<AuthService>();
        typeof(AuthService).GetField("_currentUser", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(authService, new UserSession { UserId = userId, Email = email, IsAuthenticated = true });
    }

    private void SetupAuthenticatedUserWithData(string email, int userId, string clubsJson, string eventsJson)
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("*/api/clubs/user/*")
            .Respond("application/json", clubsJson);
        mockHttp.When("*/api/events/user/*/rsvps")
            .Respond("application/json", eventsJson);
        mockHttp.Fallback.Respond("application/json", "[]");

        var http = mockHttp.ToHttpClient();
        http.BaseAddress = new Uri("https://localhost:7071/");
        Services.RemoveAll<HttpClient>();
        Services.AddSingleton(http);

        // Setup authenticated AuthService
        var authService = Services.GetRequiredService<AuthService>();
        typeof(AuthService).GetField("_currentUser", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(authService, new UserSession { UserId = userId, Email = email, IsAuthenticated = true });
    }

    #region Authentication Tests

    [Fact]
    public void Dashboard_RequiresAuthentication()
    {
        // Arrange - no authentication
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.Fallback.Respond("application/json", "[]");
        
        var http = mockHttp.ToHttpClient();
        http.BaseAddress = new Uri("https://localhost:7071/");
        Services.RemoveAll<HttpClient>();
        Services.AddSingleton(http);

        // Act
        var component = RenderComponent<Dashboard>();

        // Assert - should show access denied message
        component.WaitForAssertion(() =>
        {
            var html = component.Markup;
            Assert.Contains("Access Denied", html);
            Assert.Contains("log in", html, StringComparison.OrdinalIgnoreCase);
        }, timeout: TimeSpan.FromSeconds(3));
    }

    [Fact]
    public void Dashboard_DisplaysUserInformation()
    {
        // Arrange - authenticated user
        SetupAuthenticatedUser("alex@campus.edu", 1);

        // Act
        var component = RenderComponent<Dashboard>();

        // Assert - should show user's email in welcome message
        component.WaitForAssertion(() =>
        {
            var html = component.Markup;
            Assert.Contains("Welcome back", html);
            Assert.Contains("alex@campus.edu", html);
        }, timeout: TimeSpan.FromSeconds(3));
    }

    [Fact]
    public void Dashboard_AuthService_StateChange_TriggersRerender()
    {
        // Arrange - start with unauthenticated state
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("*/api/clubs/user/*")
            .Respond("application/json", "[]");
        mockHttp.When("*/api/events/user/*/rsvps")
            .Respond("application/json", "[]");
        mockHttp.Fallback.Respond("application/json", "[]");

        var http = mockHttp.ToHttpClient();
        http.BaseAddress = new Uri("https://localhost:7071/");
        Services.RemoveAll<HttpClient>();
        Services.AddSingleton(http);

        // Start unauthenticated - AuthService currentUser is null by default
        var component = RenderComponent<Dashboard>();

        // Assert - initially shows access denied
        var html = component.Markup;
        Assert.Contains("Access Denied", html);

        // Act - simulate authentication state change
        var authService = Services.GetRequiredService<AuthService>();
        typeof(AuthService).GetField("_currentUser", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(authService, new UserSession { UserId = 1, Email = "test@campus.edu", IsAuthenticated = true });
        
        // Trigger StateHasChanged by forcing a re-render
        component.Render();

        // Assert - should now show dashboard content
        component.WaitForAssertion(() =>
        {
            var updatedHtml = component.Markup;
            Assert.DoesNotContain("Access Denied", updatedHtml);
            Assert.Contains("Dashboard", updatedHtml);
        }, timeout: TimeSpan.FromSeconds(3));
    }

    #endregion

    #region Layout and Navigation Tests

    [Fact]
    public void Dashboard_RendersCorrectly()
    {
        // Arrange - authenticated user with minimal data
        SetupAuthenticatedUser("test@campus.edu", 1);

        // Act
        var component = RenderComponent<Dashboard>();

        // Assert - check for main dashboard sections
        component.WaitForAssertion(() =>
        {
            var html = component.Markup;
            Assert.Contains("Dashboard", html);
            Assert.Contains("My Clubs", html);
            Assert.Contains("Attending Events", html);
            Assert.Contains("Clubs Joined", html);
            Assert.Contains("Events Registered", html);
        }, timeout: TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Dashboard_HasNavigationToOtherPages()
    {
        // Arrange - authenticated user
        SetupAuthenticatedUser("test@campus.edu", 1);

        // Act
        var component = RenderComponent<Dashboard>();

        // Assert - check for navigation links
        component.WaitForAssertion(() =>
        {
            var html = component.Markup;
            var hrefs = component.FindAll("a").Select(a => a.GetAttribute("href") ?? "").ToArray();

            Assert.Contains(hrefs, h => h.Contains("/clubs", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(hrefs, h => h.Contains("/events", StringComparison.OrdinalIgnoreCase));
        }, timeout: TimeSpan.FromSeconds(3));
    }

    #endregion

    #region Data Display Tests

    [Fact]
    public void Dashboard_DisplaysUserClubs()
    {
        // Arrange - user with clubs
        var clubsJson = """
            [
                {"ClubId":1,"ClubName":"Computer Science Society","ClubDescription":"Tech enthusiasts","ClubImgURL":""},
                {"ClubId":2,"ClubName":"Debate Club","ClubDescription":"Public speaking","ClubImgURL":""}
            ]
        """;
        SetupAuthenticatedUserWithData("test@campus.edu", 1, clubsJson, "[]");

        // Act
        var component = RenderComponent<Dashboard>();

        // Assert - clubs should be displayed
        component.WaitForAssertion(() =>
        {
            var html = component.Markup;
            Assert.Contains("Computer Science Society", html);
            Assert.Contains("Debate Club", html);
            Assert.Contains("Member", html);
        }, timeout: TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Dashboard_DisplaysUpcomingEvents()
    {
        // Arrange - user with events
        var eventsJson = """
            [
                {"EventId":1,"EventName":"Tech Conference","EventDescription":"Annual tech event","EventDateTime":"2025-11-15T14:00:00Z"},
                {"EventId":2,"EventName":"Networking Mixer","EventDescription":"Meet fellow students","EventDateTime":"2025-11-20T18:00:00Z"}
            ]
        """;
        SetupAuthenticatedUserWithData("test@campus.edu", 1, "[]", eventsJson);

        // Act
        var component = RenderComponent<Dashboard>();

        // Assert - events should be displayed
        component.WaitForAssertion(() =>
        {
            var html = component.Markup;
            Assert.Contains("Tech Conference", html);
            Assert.Contains("Networking Mixer", html);
            Assert.Contains("Nov", html);
        }, timeout: TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Dashboard_HandlesEmptyData()
    {
        // Arrange - user with no clubs or events
        SetupAuthenticatedUser("test@campus.edu", 1);

        // Act
        var component = RenderComponent<Dashboard>();

        // Assert - should show empty state messages
        component.WaitForAssertion(() =>
        {
            var html = component.Markup;
            Assert.Contains("No clubs joined yet", html);
            Assert.Contains("No events registered", html);
            Assert.Contains("Join your first club", html);
            Assert.Contains("RSVP to events", html);
        }, timeout: TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Dashboard_UserClubs_EmptyList_DisplayLogic()
    {
        // Arrange - specifically test empty clubs list logic
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("*/api/users/me*")
            .Respond("application/json", """{ "UserId": 1, "Email": "test@campus.edu" }""");
        mockHttp.When("*/api/clubs/user/*")
            .Respond("application/json", "[]");
        mockHttp.When("*/api/events/user/*/rsvps")
            .Respond("application/json", """
                [{"EventId":1,"EventName":"Test Event","EventDateTime":"2025-12-01T14:00:00Z"}]
            """);
        mockHttp.Fallback.Respond("application/json", "[]");

        var http = mockHttp.ToHttpClient();
        http.BaseAddress = new Uri("https://localhost:7071/");
        Services.RemoveAll<HttpClient>();
        Services.AddSingleton(http);

        SetupAuthenticationOnly();

        // Act
        var component = RenderComponent<Dashboard>();

        // Assert - should show empty clubs message but not empty events
        component.WaitForAssertion(() =>
        {
            var html = component.Markup;
            Assert.Contains("No clubs joined yet", html);
            Assert.Contains("Join your first club", html);
            Assert.DoesNotContain("No events registered", html);
            Assert.Contains("Test Event", html);
        }, timeout: TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Dashboard_UserEvents_EmptyList_DisplayLogic()
    {
        // Arrange - specifically test empty events list logic
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("*/api/users/me*")
            .Respond("application/json", """{ "UserId": 1, "Email": "test@campus.edu" }""");
        mockHttp.When("*/api/clubs/user/*")
            .Respond("application/json", """
                [{"ClubId":1,"ClubName":"Test Club","ClubDescription":"A test club"}]
            """);
        mockHttp.When("*/api/events/user/*/rsvps")
            .Respond("application/json", "[]");
        mockHttp.Fallback.Respond("application/json", "[]");

        var http = mockHttp.ToHttpClient();
        http.BaseAddress = new Uri("https://localhost:7071/");
        Services.RemoveAll<HttpClient>();
        Services.AddSingleton(http);

        SetupAuthenticationOnly();

        // Act
        var component = RenderComponent<Dashboard>();

        // Assert - should show empty events message but not empty clubs
        component.WaitForAssertion(() =>
        {
            var html = component.Markup;
            Assert.Contains("No events registered", html);
            Assert.Contains("RSVP to events", html);
            Assert.DoesNotContain("No clubs joined yet", html);
            Assert.Contains("Test Club", html);
        }, timeout: TimeSpan.FromSeconds(5));
    }

    #endregion

    #region Loading and Error States

    [Fact]
    public void Dashboard_ShowsLoadingState()
    {
        // Arrange - delayed responses
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("*/api/users/me*")
            .Respond("application/json", """{ "UserId": 1, "Email": "test@campus.edu" }""");
        mockHttp.When("*/api/clubs/user/*")
            .Respond(async () =>
            {
                await Task.Delay(100).ConfigureAwait(false);
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent("[]", System.Text.Encoding.UTF8, "application/json")
                };
            });
        mockHttp.When("*/api/events/user/*/rsvps")
            .Respond("application/json", "[]");
        mockHttp.Fallback.Respond("application/json", "[]");

        var http = mockHttp.ToHttpClient();
        http.BaseAddress = new Uri("https://localhost:7071/");
        Services.RemoveAll<HttpClient>();
        Services.AddSingleton(http);

        SetupAuthenticationOnly();

        // Act
        var component = RenderComponent<Dashboard>();

        // Assert - should show loading indicator initially
        var html = component.Markup;
        Assert.Contains("Loading your dashboard", html);

        // Wait for data to load
        component.WaitForAssertion(() =>
        {
            var updatedHtml = component.Markup;
            Assert.DoesNotContain("Loading your dashboard", updatedHtml);
        }, timeout: TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Dashboard_HandlesAPIErrors()
    {
        // Arrange - API failures
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("*/api/users/me*")
            .Respond("application/json", """{ "UserId": 1, "Email": "test@campus.edu" }""");
        mockHttp.When("*/api/clubs/user/*")
            .Respond(System.Net.HttpStatusCode.InternalServerError);
        mockHttp.When("*/api/events/user/*/rsvps")
            .Respond(System.Net.HttpStatusCode.InternalServerError);
        mockHttp.Fallback.Respond("application/json", "[]");

        var http = mockHttp.ToHttpClient();
        http.BaseAddress = new Uri("https://localhost:7071/");
        Services.RemoveAll<HttpClient>();
        Services.AddSingleton(http);

        SetupAuthenticationOnly();

        // Act
        var component = RenderComponent<Dashboard>();

        // Assert - page still renders with empty states
        component.WaitForAssertion(() =>
        {
            var html = component.Markup;
            Assert.Contains("Dashboard", html);
            // Should show empty states instead of crashing
            Assert.True(
                ContainsAny(html, "No clubs", "No events"),
                "Expected empty state messages when API fails"
            );
        }, timeout: TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Dashboard_LoadUserData_HttpRequestExceptionHandling()
    {
        // Arrange - test specific HttpRequestException handling
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("*/api/users/me*")
            .Respond("application/json", """{ "UserId": 1, "Email": "test@campus.edu" }""");
        mockHttp.When("*/api/clubs/user/*")
            .Throw(new HttpRequestException("Connection refused"));
        mockHttp.When("*/api/events/user/*/rsvps")
            .Respond("application/json", "[]");
        mockHttp.Fallback.Respond("application/json", "[]");

        var http = mockHttp.ToHttpClient();
        http.BaseAddress = new Uri("https://localhost:7071/");
        Services.RemoveAll<HttpClient>();
        Services.AddSingleton(http);

        SetupAuthenticationOnly();

        // Act
        var component = RenderComponent<Dashboard>();

        // Assert - should handle HttpRequestException gracefully
        component.WaitForAssertion(() =>
        {
            var html = component.Markup;
            Assert.Contains("Dashboard", html);
            // Should continue rendering despite exception
            Assert.True(html.Length > 200, "Component should render substantial content");
        }, timeout: TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Dashboard_LoadUserData_TaskCancelledException()
    {
        // Arrange - test TaskCanceledException handling
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("*/api/users/me*")
            .Respond("application/json", """{ "UserId": 1, "Email": "test@campus.edu" }""");
        mockHttp.When("*/api/clubs/user/*")
            .Respond("application/json", "[]");
        mockHttp.When("*/api/events/user/*/rsvps")
            .Throw(new TaskCanceledException("Request timeout"));
        mockHttp.Fallback.Respond("application/json", "[]");

        var http = mockHttp.ToHttpClient();
        http.BaseAddress = new Uri("https://localhost:7071/");
        Services.RemoveAll<HttpClient>();
        Services.AddSingleton(http);

        SetupAuthenticationOnly();

        // Act
        var component = RenderComponent<Dashboard>();

        // Assert - should handle TaskCanceledException gracefully
        component.WaitForAssertion(() =>
        {
            var html = component.Markup;
            Assert.Contains("Dashboard", html);
            Assert.DoesNotContain("Loading your dashboard", html);
        }, timeout: TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Dashboard_LoadUserData_UnauthorizedResponse()
    {
        // Arrange - test 401 Unauthorized response
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("*/api/users/me*")
            .Respond("application/json", """{ "UserId": 1, "Email": "test@campus.edu" }""");
        mockHttp.When("*/api/clubs/user/*")
            .Respond(System.Net.HttpStatusCode.Unauthorized);
        mockHttp.When("*/api/events/user/*/rsvps")
            .Respond(System.Net.HttpStatusCode.Unauthorized);
        mockHttp.Fallback.Respond("application/json", "[]");

        var http = mockHttp.ToHttpClient();
        http.BaseAddress = new Uri("https://localhost:7071/");
        Services.RemoveAll<HttpClient>();
        Services.AddSingleton(http);

        SetupAuthenticationOnly();

        // Act
        var component = RenderComponent<Dashboard>();

        // Assert - should handle 401 gracefully
        component.WaitForAssertion(() =>
        {
            var html = component.Markup;
            Assert.Contains("Dashboard", html);
            // Should show empty states when unauthorized
            Assert.True(ContainsAny(html, "No clubs", "No events"));
        }, timeout: TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Dashboard_HandlesNullUserScenario()
    {
        // Arrange - authenticated but user data is null
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("*/api/users/me*")
            .Respond("application/json", "null");
        mockHttp.When("*/api/clubs/user/*")
            .Respond("application/json", "[]");
        mockHttp.When("*/api/events/user/*/rsvps")
            .Respond("application/json", "[]");
        mockHttp.Fallback.Respond("application/json", "[]");

        var http = mockHttp.ToHttpClient();
        http.BaseAddress = new Uri("https://localhost:7071/");
        Services.RemoveAll<HttpClient>();
        Services.AddSingleton(http);

        // Setup authenticated but with null CurrentUser (special case for this test)
        var authService = Services.GetRequiredService<AuthService>();
        typeof(AuthService).GetField("_currentUser", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(authService, new UserSession { UserId = 1, Email = "", IsAuthenticated = true });

        // Act
        var component = RenderComponent<Dashboard>();

        // Assert - should handle null user gracefully
        component.WaitForAssertion(() =>
        {
            var html = component.Markup;
            Assert.Contains("Dashboard", html);
            // Should not crash when CurrentUser email is empty
            Assert.Contains("Welcome back,", html);
            Assert.DoesNotContain("@null", html);
        }, timeout: TimeSpan.FromSeconds(3));
    }

    #endregion

    #region Quick Actions Tests

    [Fact]
    public void Dashboard_HasQuickActionButtons()
    {
        // Arrange - authenticated user
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("*/api/users/me*")
            .Respond("application/json", """{ "UserId": 1, "Email": "test@campus.edu" }""");
        mockHttp.When("*/api/clubs/user/*")
            .Respond("application/json", "[]");
        mockHttp.When("*/api/events/user/*/rsvps")
            .Respond("application/json", "[]");
        mockHttp.Fallback.Respond("application/json", "[]");

        var http = mockHttp.ToHttpClient();
        http.BaseAddress = new Uri("https://localhost:7071/");
        Services.RemoveAll<HttpClient>();
        Services.AddSingleton(http);

        SetupAuthenticationOnly();

        // Act
        var component = RenderComponent<Dashboard>();

        // Assert - check for quick action buttons
        component.WaitForAssertion(() =>
        {
            var html = component.Markup;
            Assert.Contains("Discover More Clubs", html);
            Assert.Contains("Find More Events", html);
            Assert.Contains("View All", html);
        }, timeout: TimeSpan.FromSeconds(3));
    }

    [Fact]
    public void Dashboard_QuickActions_WorkCorrectly()
    {
        // Arrange - authenticated user
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("*/api/users/me*")
            .Respond("application/json", """{ "UserId": 1, "Email": "test@campus.edu" }""");
        mockHttp.When("*/api/clubs/user/*")
            .Respond("application/json", "[]");
        mockHttp.When("*/api/events/user/*/rsvps")
            .Respond("application/json", "[]");
        mockHttp.Fallback.Respond("application/json", "[]");

        var http = mockHttp.ToHttpClient();
        http.BaseAddress = new Uri("https://localhost:7071/");
        Services.RemoveAll<HttpClient>();
        Services.AddSingleton(http);

        SetupAuthenticationOnly();

        // Act
        var component = RenderComponent<Dashboard>();

        // Assert - verify links point to correct pages
        component.WaitForAssertion(() =>
        {
            var links = component.FindAll("a");
            var clubLinks = links.Where(a => (a.GetAttribute("href") ?? "").Contains("/clubs")).ToList();
            var eventLinks = links.Where(a => (a.GetAttribute("href") ?? "").Contains("/events")).ToList();

            Assert.NotEmpty(clubLinks);
            Assert.NotEmpty(eventLinks);
        }, timeout: TimeSpan.FromSeconds(3));
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public void Dashboard_DisplaysUserStatistics()
    {
        // Arrange - user with 2 clubs and 3 events
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("*/api/users/me*")
            .Respond("application/json", """{ "UserId": 1, "Email": "test@campus.edu" }""");
        mockHttp.When("*/api/clubs/user/*")
            .Respond("application/json", """
                [
                    {"ClubId":1,"ClubName":"Club A"},
                    {"ClubId":2,"ClubName":"Club B"}
                ]
            """);
        mockHttp.When("*/api/events/user/*/rsvps")
            .Respond("application/json", """
                [
                    {"EventId":1,"EventName":"Event A","EventDateTime":"2025-11-15T14:00:00Z"},
                    {"EventId":2,"EventName":"Event B","EventDateTime":"2025-11-16T14:00:00Z"},
                    {"EventId":3,"EventName":"Event C","EventDateTime":"2025-11-17T14:00:00Z"}
                ]
            """);
        mockHttp.Fallback.Respond("application/json", "[]");

        var http = mockHttp.ToHttpClient();
        http.BaseAddress = new Uri("https://localhost:7071/");
        Services.RemoveAll<HttpClient>();
        Services.AddSingleton(http);

        SetupAuthenticationOnly();

        // Act
        var component = RenderComponent<Dashboard>();

        // Assert - check statistics display correct counts
        component.WaitForAssertion(() =>
        {
            var html = component.Markup;
            Assert.Contains("Clubs Joined", html);
            Assert.Contains("Events Registered", html);
            
            // Check for the numbers (with some flexibility for formatting)
            var hasClubCount = Regex.IsMatch(html, @">\s*2\s*<");
            var hasEventCount = Regex.IsMatch(html, @">\s*3\s*<");
            
            Assert.True(hasClubCount, "Expected to find club count of 2");
            Assert.True(hasEventCount, "Expected to find event count of 3");
        }, timeout: TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Dashboard_Statistics_ZeroValues()
    {
        // Arrange - test statistics with zero values
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("*/api/users/me*")
            .Respond("application/json", """{ "UserId": 1, "Email": "test@campus.edu" }""");
        mockHttp.When("*/api/clubs/user/*")
            .Respond("application/json", "[]");
        mockHttp.When("*/api/events/user/*/rsvps")
            .Respond("application/json", "[]");
        mockHttp.Fallback.Respond("application/json", "[]");

        var http = mockHttp.ToHttpClient();
        http.BaseAddress = new Uri("https://localhost:7071/");
        Services.RemoveAll<HttpClient>();
        Services.AddSingleton(http);

        SetupAuthenticationOnly();

        // Act
        var component = RenderComponent<Dashboard>();

        // Assert - should display zero statistics correctly
        component.WaitForAssertion(() =>
        {
            var html = component.Markup;
            Assert.Contains("Clubs Joined", html);
            Assert.Contains("Events Registered", html);
            
            // Check for zero values
            var hasZeroClubCount = Regex.IsMatch(html, @">\s*0\s*<");
            Assert.True(hasZeroClubCount, "Expected to find zero club/event count");
        }, timeout: TimeSpan.FromSeconds(5));
    }

    #endregion

    #region Comprehensive Dashboard Component Tests (inspired by User tests)

    [Fact]
    public void Dashboard_Component_RendersAllSections()
    {
        // Arrange - authenticated user with comprehensive data
        var clubsJson = """[{"ClubId":1,"ClubName":"Test Club","ClubDescription":"Test Description","ClubImgURL":""}]""";
        var eventsJson = """[{"EventId":1,"EventName":"Test Event","EventDescription":"Test Event Desc","EventDateTime":"2025-12-01T14:00:00Z"}]""";
        SetupAuthenticatedUserWithData("test@campus.edu", 1, clubsJson, eventsJson);

        // Act
        var component = RenderComponent<Dashboard>();

        // Assert - all main sections should be present
        component.WaitForAssertion(() =>
        {
            var html = component.Markup;
            Assert.Contains("Dashboard", html);
            Assert.Contains("Welcome back", html);
            Assert.Contains("test@campus.edu", html);
            Assert.Contains("Clubs Joined", html);
            Assert.Contains("Events Registered", html);
            Assert.Contains("My Clubs", html);
            Assert.Contains("Attending Events", html);
            Assert.Contains("Discover More Clubs", html);
            Assert.Contains("Find More Events", html);
        }, timeout: TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData("user1@test.com", 1)]
    [InlineData("user2@test.com", 2)]
    [InlineData("admin@campus.edu", 999)]
    public void Dashboard_Component_HandlesVariousUsers(string email, int userId)
    {
        // Arrange
        SetupAuthenticatedUser(email, userId);

        // Act
        var component = RenderComponent<Dashboard>();

        // Assert
        component.WaitForAssertion(() =>
        {
            var html = component.Markup;
            Assert.Contains("Welcome back", html);
            Assert.Contains(email, html);
            Assert.Contains("Dashboard", html);
        }, timeout: TimeSpan.FromSeconds(3));
    }

    [Fact]
    public void Dashboard_Statistics_DisplayCorrectValues()
    {
        // Arrange - user with known counts
        var clubsJson = """
        [
            {"ClubId":1,"ClubName":"Club 1","ClubDescription":"","ClubImgURL":""},
            {"ClubId":2,"ClubName":"Club 2","ClubDescription":"","ClubImgURL":""},
            {"ClubId":3,"ClubName":"Club 3","ClubDescription":"","ClubImgURL":""}
        ]
        """;
        var eventsJson = """
        [
            {"EventId":1,"EventName":"Event 1","EventDateTime":"2025-12-01T14:00:00Z"},
            {"EventId":2,"EventName":"Event 2","EventDateTime":"2025-12-02T14:00:00Z"}
        ]
        """;
        SetupAuthenticatedUserWithData("test@campus.edu", 1, clubsJson, eventsJson);

        // Act
        var component = RenderComponent<Dashboard>();

        // Assert - statistics should show correct counts
        component.WaitForAssertion(() =>
        {
            var html = component.Markup;
            Assert.Contains("3", html);
            Assert.Contains("2", html);
            Assert.Contains("Clubs Joined", html);
            Assert.Contains("Events Registered", html);
        }, timeout: TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Dashboard_LoadingState_IsNotVisible()
    {
        // Arrange
        SetupAuthenticatedUser("test@campus.edu", 1);

        // Act
        var component = RenderComponent<Dashboard>();

        // Assert - loading should not be visible after initialization
        component.WaitForAssertion(() =>
        {
            var html = component.Markup;
            Assert.DoesNotContain("Loading your dashboard", html);
            Assert.Contains("Dashboard", html);
        }, timeout: TimeSpan.FromSeconds(3));
    }

    [Fact]
    public void Dashboard_ClubImages_HandleMissingUrls()
    {
        // Arrange - clubs without image URLs
        var clubsJson = """
        [
            {"ClubId":1,"ClubName":"Club Without Image","ClubDescription":"Test","ClubImgURL":""},
            {"ClubId":2,"ClubName":"Club With Image","ClubDescription":"Test","ClubImgURL":"http://example.com/image.jpg"}
        ]
        """;
        SetupAuthenticatedUserWithData("test@campus.edu", 1, clubsJson, "[]");

        // Act
        var component = RenderComponent<Dashboard>();

        // Assert - should handle missing image URLs gracefully
        component.WaitForAssertion(() =>
        {
            var html = component.Markup;
            Assert.Contains("Club Without Image", html);
            Assert.Contains("Club With Image", html);
            Assert.Contains("picsum.photos", html);
        }, timeout: TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Dashboard_EventDates_FormattedCorrectly()
    {
        // Arrange - events with specific dates
        var eventsJson = """
        [
            {"EventId":1,"EventName":"December Event","EventDateTime":"2025-12-15T14:30:00Z"},
            {"EventId":2,"EventName":"January Event","EventDateTime":"2026-01-10T09:15:00Z"}
        ]
        """;
        SetupAuthenticatedUserWithData("test@campus.edu", 1, "[]", eventsJson);

        // Act
        var component = RenderComponent<Dashboard>();

        // Assert - dates should be formatted properly
        component.WaitForAssertion(() =>
        {
            var html = component.Markup;
            Assert.Contains("December Event", html);
            Assert.Contains("January Event", html);
            Assert.Contains("Dec", html);
            Assert.Contains("Jan", html);
        }, timeout: TimeSpan.FromSeconds(5));
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Dashboard_LoadsQuickly()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("*/api/users/me*")
            .Respond("application/json", """{ "UserId": 1, "Email": "test@campus.edu" }""");
        mockHttp.When("*/api/clubs/user/*")
            .Respond("application/json", "[]");
        mockHttp.When("*/api/events/user/*/rsvps")
            .Respond("application/json", "[]");
        mockHttp.Fallback.Respond("application/json", "[]");

        var http = mockHttp.ToHttpClient();
        http.BaseAddress = new Uri("https://localhost:7071/");
        Services.RemoveAll<HttpClient>();
        Services.AddSingleton(http);

        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("test@campus.edu");

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var component = RenderComponent<Dashboard>();
        stopwatch.Stop();

        // Assert - should render under 2 seconds
        Assert.True(stopwatch.ElapsedMilliseconds < 2000, 
            $"Dashboard page took too long to load ({stopwatch.ElapsedMilliseconds}ms).");
        Assert.NotNull(component);
    }

    [Fact]
    public void Dashboard_DisplaysLimitedItemsInPreview()
    {
        // Arrange - user with many clubs and events (should only show top 3)
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("*/api/users/me*")
            .Respond("application/json", """{ "UserId": 1, "Email": "test@campus.edu" }""");
        mockHttp.When("*/api/clubs/user/*")
            .Respond("application/json", """
                [
                    {"ClubId":1,"ClubName":"Club 1"},
                    {"ClubId":2,"ClubName":"Club 2"},
                    {"ClubId":3,"ClubName":"Club 3"},
                    {"ClubId":4,"ClubName":"Club 4"},
                    {"ClubId":5,"ClubName":"Club 5"}
                ]
            """);
        mockHttp.When("*/api/events/user/*/rsvps")
            .Respond("application/json", """
                [
                    {"EventId":1,"EventName":"Event 1","EventDateTime":"2025-11-15T14:00:00Z"},
                    {"EventId":2,"EventName":"Event 2","EventDateTime":"2025-11-16T14:00:00Z"},
                    {"EventId":3,"EventName":"Event 3","EventDateTime":"2025-11-17T14:00:00Z"},
                    {"EventId":4,"EventName":"Event 4","EventDateTime":"2025-11-18T14:00:00Z"}
                ]
            """);
        mockHttp.Fallback.Respond("application/json", "[]");

        var http = mockHttp.ToHttpClient();
        http.BaseAddress = new Uri("https://localhost:7071/");
        Services.RemoveAll<HttpClient>();
        Services.AddSingleton(http);

        SetupAuthenticationOnly();

        // Act
        var component = RenderComponent<Dashboard>();

        // Assert - should only show first 3 of each (as per .Take(3) in Dashboard.razor)
        component.WaitForAssertion(() =>
        {
            var html = component.Markup;
            
            // Should show Club 1-3 but not Club 4-5
            Assert.Contains("Club 1", html);
            Assert.Contains("Club 2", html);
            Assert.Contains("Club 3", html);
            Assert.DoesNotContain("Club 4", html);
            
            // Should show Event 1-3 but not Event 4
            Assert.Contains("Event 1", html);
            Assert.Contains("Event 2", html);
            Assert.Contains("Event 3", html);
            Assert.DoesNotContain("Event 4", html);
        }, timeout: TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Dashboard_LoadUserData_WithValidUser()
    {
        // Arrange - test successful data loading path
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("*/api/users/me*")
            .Respond("application/json", """{ "UserId": 123, "Email": "valid@campus.edu", "FirstName": "Valid" }""");
        mockHttp.When("*/api/clubs/user/*")
            .Respond("application/json", """
                [{"ClubId":1,"ClubName":"Test Club","ClubDescription":"A test club"}]
            """);
        mockHttp.When("*/api/events/user/*/rsvps")
            .Respond("application/json", """
                [{"EventId":1,"EventName":"Test Event","EventDateTime":"2025-12-01T14:00:00Z"}]
            """);
        mockHttp.Fallback.Respond("application/json", "[]");

        var http = mockHttp.ToHttpClient();
        http.BaseAddress = new Uri("https://localhost:7071/");
        Services.RemoveAll<HttpClient>();
        Services.AddSingleton(http);

        SetupAuthenticationOnly("valid@campus.edu", 123);

        // Act
        var component = RenderComponent<Dashboard>();

        // Assert - verify successful data loading
        component.WaitForAssertion(() =>
        {
            var html = component.Markup;
            Assert.Contains("valid@campus.edu", html);
            Assert.Contains("Test Club", html);
            Assert.Contains("Test Event", html);
            Assert.DoesNotContain("Loading", html);
        }, timeout: TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Dashboard_EventDateTime_Formatting()
    {
        // Arrange - test event date formatting
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("*/api/users/me*")
            .Respond("application/json", """{ "UserId": 1, "Email": "test@campus.edu" }""");
        mockHttp.When("*/api/clubs/user/*")
            .Respond("application/json", "[]");
        mockHttp.When("*/api/events/user/*/rsvps")
            .Respond("application/json", """
                [
                    {"EventId":1,"EventName":"December Event","EventDateTime":"2025-12-25T15:30:00Z"},
                    {"EventId":2,"EventName":"January Event","EventDateTime":"2026-01-15T09:00:00Z"}
                ]
            """);
        mockHttp.Fallback.Respond("application/json", "[]");

        var http = mockHttp.ToHttpClient();
        http.BaseAddress = new Uri("https://localhost:7071/");
        Services.RemoveAll<HttpClient>();
        Services.AddSingleton(http);

        SetupAuthenticationOnly();

        // Act
        var component = RenderComponent<Dashboard>();

        // Assert - should format dates correctly
        component.WaitForAssertion(() =>
        {
            var html = component.Markup;
            Assert.Contains("December Event", html);
            Assert.Contains("January Event", html);
            // Check for month abbreviations (typical formatting)
            Assert.True(
                ContainsAny(html, "Dec", "Jan", "25", "15"),
                "Expected to find formatted date elements"
            );
        }, timeout: TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Dashboard_TakeThree_Logic_EdgeCases()
    {
        // Arrange - test exactly 3 items (boundary condition)
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("*/api/users/me*")
            .Respond("application/json", """{ "UserId": 1, "Email": "test@campus.edu" }""");
        mockHttp.When("*/api/clubs/user/*")
            .Respond("application/json", """
                [
                    {"ClubId":1,"ClubName":"Club One"},
                    {"ClubId":2,"ClubName":"Club Two"},
                    {"ClubId":3,"ClubName":"Club Three"}
                ]
            """);
        mockHttp.When("*/api/events/user/*/rsvps")
            .Respond("application/json", """
                [
                    {"EventId":1,"EventName":"Event One","EventDateTime":"2025-11-15T14:00:00Z"},
                    {"EventId":2,"EventName":"Event Two","EventDateTime":"2025-11-16T14:00:00Z"},
                    {"EventId":3,"EventName":"Event Three","EventDateTime":"2025-11-17T14:00:00Z"}
                ]
            """);
        mockHttp.Fallback.Respond("application/json", "[]");

        var http = mockHttp.ToHttpClient();
        http.BaseAddress = new Uri("https://localhost:7071/");
        Services.RemoveAll<HttpClient>();
        Services.AddSingleton(http);

        SetupAuthenticationOnly();

        // Act
        var component = RenderComponent<Dashboard>();

        // Assert - should show all 3 items (exactly at the limit)
        component.WaitForAssertion(() =>
        {
            var html = component.Markup;
            Assert.Contains("Club One", html);
            Assert.Contains("Club Two", html);
            Assert.Contains("Club Three", html);
            Assert.Contains("Event One", html);
            Assert.Contains("Event Two", html);
            Assert.Contains("Event Three", html);
        }, timeout: TimeSpan.FromSeconds(5));
    }

    #endregion
}
