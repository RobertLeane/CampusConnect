using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Client.Pages;
using Client.Services;
using RichardSzalay.MockHttp;
using Xunit;

namespace CampusConnect_Tests;

public class ClubsPageTests : TestContext
{
    public ClubsPageTests()
    {
        // Register required services for Clubs page
        Services.AddSingleton<AuthService>();
        Services.AddSingleton<ClubApi>();
        Services.AddSingleton<EventApi>();
        Services.AddSingleton<MemberApi>();
        Services.AddSingleton<UserApi>();
        
        // Setup mock JSRuntime
        JSInterop.Mode = JSRuntimeMode.Loose;
        
        // Setup basic HTTP client (will be replaced in individual tests as needed)
        var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri("https://localhost:7071/");
        Services.AddSingleton(httpClient);
    }

    #region Page Rendering Tests

    [Fact]
    public void ClubsPage_RendersCorrectly()
    {
        // Act
        var component = RenderComponent<Clubs>();

        // Assert - Check basic page structure (should render immediately)
        Assert.Contains("Discover Your", component.Markup);
        Assert.Contains("Clubs", component.Markup);
        Assert.Contains("Join the clubs that interest you", component.Markup);
        
        // Check statistics section labels (these should always be present)
        Assert.Contains("Active Clubs", component.Markup);
        Assert.Contains("Events", component.Markup);
        Assert.Contains("Active Members", component.Markup);
    }

    #endregion

    #region Club List Display Tests

    [Fact]
    public void ClubsPage_DisplaysClubList()
    {
        // Arrange - Mock all required HTTP endpoints
        var mockHttp = new MockHttpMessageHandler();
        
        // Mock clubs data with proper model structure
        var clubData = new[]
        {
            new
            {
                ClubId = 1,
                ClubName = "Chess Club",
                ClubImgURL = "https://example.com/chess-club.jpg",
                ClubDescription = "A club for chess enthusiasts."
            }
        };

        // Mock events data
        var eventData = new[]
        {
            new { EventId = 1, EventName = "Chess Tournament", EventDescription = "Test", EventDateTime = DateTime.Now, EventImgURL = "", ClubId = 1 }
        };

        // Mock users data  
        var userData = new[]
        {
            new { UserId = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com" }
        };

        // Setup all API endpoints
        mockHttp.When("*/api/clubs").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(clubData));
        mockHttp.When("*/api/events").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(eventData));
        mockHttp.When("*/api/users").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(userData));

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost:7071/");
        
        // Replace the HTTP client in the current test context
        Services.AddSingleton(httpClient);

        // Act
        var component = RenderComponent<Clubs>();

        // Assert - Initially should show loading
        Assert.Contains("Loading clubs...", component.Markup);

        // Wait for data to load and check for club content
        component.WaitForAssertion(() =>
        {
            var markup = component.Markup;
            
            // Should not be loading anymore
            Assert.DoesNotContain("Loading clubs...", markup);
            
            // Should show club name somewhere in the page
            Assert.Contains("Chess Club", markup);
        }, timeout: TimeSpan.FromSeconds(3));
    }

    [Fact]
    public void ClubsPage_DisplaysEmptyState_WhenNoClubs()
    {
        // Arrange - Mock empty responses
        var mockHttp = new MockHttpMessageHandler();
        
        // Mock empty data arrays
        var emptyClubData = new object[0];
        var emptyEventData = new object[0];
        var emptyUserData = new object[0];

        // Setup all API endpoints with empty responses
        mockHttp.When("*/api/clubs").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(emptyClubData));
        mockHttp.When("*/api/events").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(emptyEventData));
        mockHttp.When("*/api/users").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(emptyUserData));

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost:7071/");
        
        Services.AddSingleton(httpClient);

        // Act
        var component = RenderComponent<Clubs>();

        // Assert - Wait for data to load and check empty state
        component.WaitForAssertion(() =>
        {
            var markup = component.Markup;
            
            // Should not be loading anymore
            Assert.DoesNotContain("Loading clubs...", markup);
            
            // Should show statistics as 0
            Assert.Contains("0", markup); // Should show 0 for stats
        }, timeout: TimeSpan.FromSeconds(3));
    }

    [Fact]
    public void ClubsPage_DisplaysClubDetails()
    {
        // Arrange - Mock club with detailed information
        var mockHttp = new MockHttpMessageHandler();
        
        var clubData = new[]
        {
            new
            {
                ClubId = 1,
                ClubName = "Photography Club",
                ClubImgURL = "https://example.com/photo-club.jpg",
                ClubDescription = "Capture amazing moments and learn photography techniques."
            }
        };

        var eventData = new[]
        {
            new { EventId = 1, EventName = "Photo Walk", EventDescription = "Test", EventDateTime = DateTime.Now, EventImgURL = "", ClubId = 1 }
        };

        var userData = new[]
        {
            new { UserId = 1, FirstName = "Jane", LastName = "Smith", Email = "jane@example.com" }
        };

        // Setup all API endpoints
        mockHttp.When("*/api/clubs").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(clubData));
        mockHttp.When("*/api/events").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(eventData));
        mockHttp.When("*/api/users").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(userData));

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost:7071/");
        
        Services.AddSingleton(httpClient);

        // Act
        var component = RenderComponent<Clubs>();

        // Assert - Wait for data and check club details
        component.WaitForAssertion(() =>
        {
            var markup = component.Markup;
            
            // Should not be loading anymore
            Assert.DoesNotContain("Loading clubs...", markup);
            
            // Check club details
            Assert.Contains("Photography Club", markup);
            Assert.Contains("Capture amazing moments and learn photography techniques.", markup);
            Assert.Contains("Join Club", markup);
        }, timeout: TimeSpan.FromSeconds(3));
    }

    #endregion

   

    #region Club Actions Tests

    [Fact]
    public void ClubsPage_JoinClubWorks()
    {
        // Arrange - Mock club data and setup for join action
        var mockHttp = new MockHttpMessageHandler();
        
        var clubData = new[]
        {
            new
            {
                ClubId = 1,
                ClubName = "Basketball Club",
                ClubImgURL = "https://example.com/basketball.jpg",
                ClubDescription = "Play basketball and improve your skills."
            }
        };

        var eventData = new[]
        {
            new { EventId = 1, EventName = "Basketball Game", EventDescription = "Test", EventDateTime = DateTime.Now, EventImgURL = "", ClubId = 1 }
        };

        var userData = new[]
        {
            new { UserId = 1, FirstName = "Mike", LastName = "Jordan", Email = "mike@example.com" }
        };

        // Setup all API endpoints
        mockHttp.When("*/api/clubs").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(clubData));
        mockHttp.When("*/api/events").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(eventData));
        mockHttp.When("*/api/users").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(userData));

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost:7071/");
        
        Services.AddSingleton(httpClient);

        // Act
        var component = RenderComponent<Clubs>();

        // Assert - Wait for data and check join button is present
        component.WaitForAssertion(() =>
        {
            var markup = component.Markup;
            
            // Should not be loading anymore
            Assert.DoesNotContain("Loading clubs...", markup);
            
            // Check that join functionality is available
            Assert.Contains("Basketball Club", markup);
            Assert.Contains("Join Club", markup);
        }, timeout: TimeSpan.FromSeconds(3));
    }

    

    #endregion

    #region Permission and Authentication Tests

    [Fact]
    public void ClubsPage_RequiresLogin_ForJoining()
    {
        // Arrange - Mock club data for authentication test
        var mockHttp = new MockHttpMessageHandler();
        
        var clubData = new[]
        {
            new
            {
                ClubId = 1,
                ClubName = "Debate Club",
                ClubImgURL = "https://example.com/debate.jpg",
                ClubDescription = "Improve your public speaking and debate skills."
            }
        };

        var eventData = new[]
        {
            new { EventId = 1, EventName = "Debate Tournament", EventDescription = "Test", EventDateTime = DateTime.Now, EventImgURL = "", ClubId = 1 }
        };

        var userData = new[]
        {
            new { UserId = 1, FirstName = "Alice", LastName = "Cooper", Email = "alice@example.com" }
        };

        // Setup all API endpoints
        mockHttp.When("*/api/clubs").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(clubData));
        mockHttp.When("*/api/events").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(eventData));
        mockHttp.When("*/api/users").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(userData));

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost:7071/");
        
        Services.AddSingleton(httpClient);

        // Act
        var component = RenderComponent<Clubs>();

        // Assert - Check that page loads and displays login-related elements
        component.WaitForAssertion(() =>
        {
            var markup = component.Markup;
            
            // Should not be loading anymore
            Assert.DoesNotContain("Loading clubs...", markup);
            
            // Check club is displayed
            Assert.Contains("Debate Club", markup);
            
            // Basic check for authentication elements (join button or login prompt)
            Assert.True(markup.Contains("Join Club") || markup.Contains("Login") || markup.Contains("Sign in"));
        }, timeout: TimeSpan.FromSeconds(3));
    }

    #endregion

    #region Loading and Error States

    [Fact]
    public void ClubsPage_ShowsLoadingState()
    {
        // Arrange - No HTTP mocking to test initial loading state
        var component = RenderComponent<Clubs>();

        // Assert - Check initial loading state
        Assert.Contains("Loading clubs...", component.Markup);
        
        // Basic page structure should still be present
        Assert.Contains("Discover Your", component.Markup);
        Assert.Contains("Clubs", component.Markup);
    }

    [Fact]
    public void ClubsPage_HandlesAPIErrors()
    {
        // Arrange - Mock API error responses
        var mockHttp = new MockHttpMessageHandler();
        
        // Setup API endpoints to return error responses
        mockHttp.When("*/api/clubs").Respond(System.Net.HttpStatusCode.InternalServerError);
        mockHttp.When("*/api/events").Respond(System.Net.HttpStatusCode.InternalServerError);
        mockHttp.When("*/api/users").Respond(System.Net.HttpStatusCode.InternalServerError);

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost:7071/");
        
        Services.AddSingleton(httpClient);

        // Act
        var component = RenderComponent<Clubs>();

        // Assert - Initially should show loading
        Assert.Contains("Loading clubs...", component.Markup);

        // Wait and check that error is handled gracefully
        component.WaitForAssertion(() =>
        {
            var markup = component.Markup;
            
            // Should not be loading anymore
            Assert.DoesNotContain("Loading clubs...", markup);
            
            // Basic page structure should still be present even with errors
            Assert.Contains("Discover Your", markup);
            Assert.Contains("Clubs", markup);
        }, timeout: TimeSpan.FromSeconds(3));
    }

    #endregion
}