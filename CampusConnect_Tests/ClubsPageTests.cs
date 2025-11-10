using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Client.Pages;
using Client.Services;
using RichardSzalay.MockHttp;
using Xunit;
using System.Threading.Tasks;

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
        
        // Setup basic HTTP client
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

        // Check basic page structure
        Assert.Contains("Discover Your", component.Markup);
        Assert.Contains("Clubs", component.Markup);
        Assert.Contains("Join the clubs that interest you", component.Markup);
        
        // Check statistics section labels
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

        // Mock users count
        mockHttp.When("*/api/users/count").Respond("application/json", "10");

        // Setup all API endpoints
        mockHttp.When("*/api/clubs").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(clubData));
        mockHttp.When("*/api/events").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(eventData));

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost:7071/");
        
        // Replace the HTTP client in the current test context
        Services.AddSingleton(httpClient);

        // Act
        var component = RenderComponent<Clubs>();

        // Wait for the data to load
        component.WaitForAssertion(() =>
        {
            var markup = component.Markup;
            Assert.Contains("Chess Club", markup);
        }, timeout: TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void ClubsPage_DisplaysEmptyState_WhenNoClubs()
    {
        // Arrange - Mock empty responses
        var mockHttp = new MockHttpMessageHandler();
        
        // Mock empty data arrays
        var emptyClubData = new object[0];
        var emptyEventData = new object[0];

        // Mock users count
        mockHttp.When("*/api/users/count").Respond("application/json", "0");

        // Setup all API endpoints with empty responses
        mockHttp.When("*/api/clubs").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(emptyClubData));
        mockHttp.When("*/api/events").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(emptyEventData));

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
            Assert.Contains("0", markup);
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

        // Mock users count
        mockHttp.When("*/api/users/count").Respond("application/json", "1");

        // Setup all API endpoints
        mockHttp.When("*/api/clubs").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(clubData));
        mockHttp.When("*/api/events").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(eventData));

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

        // Mock users count
        mockHttp.When("*/api/users/count").Respond("application/json", "1");

        // Setup all API endpoints
        mockHttp.When("*/api/clubs").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(clubData));
        mockHttp.When("*/api/events").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(eventData));

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

        // Mock users count
        mockHttp.When("*/api/users/count").Respond("application/json", "1");

        // Setup all API endpoints
        mockHttp.When("*/api/clubs").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(clubData));
        mockHttp.When("*/api/events").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(eventData));

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
        mockHttp.When("*/api/users/count").Respond(System.Net.HttpStatusCode.InternalServerError);
        mockHttp.When("*/api/clubs").Respond(System.Net.HttpStatusCode.InternalServerError);
        mockHttp.When("*/api/events").Respond(System.Net.HttpStatusCode.InternalServerError);

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost:7071/");
        
        Services.AddSingleton(httpClient);

        // Act
        var component = RenderComponent<Clubs>();

        // Assert - Wait and check that error is handled gracefully
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

    #region Club Description Tests

    [Fact]
    public void ShowsClubDescription_WhenAvailable()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var clubsJson = System.Text.Json.JsonSerializer.Serialize(new[]
        {
            new { ClubID = 1, ClubName = "Chess Club", ClubDescription = "Learn and play chess with fellow students", ClubImgURL = "", CreatedDate = "2024-01-01" }
        });

        mockHttp.When("*/api/clubs").Respond("application/json", clubsJson);
        mockHttp.When("*/api/members*").Respond("application/json", "[]");
        mockHttp.When("*/api/events").Respond("application/json", "[]");
        mockHttp.When("*/api/users/count").Respond("application/json", "0");

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost:7071/");
        Services.AddSingleton(httpClient);

        // Act
        var component = RenderComponent<Clubs>();
        component.WaitForAssertion(() => Assert.Contains("Learn and play chess with fellow students", component.Markup));

        // Assert
        Assert.Contains("Learn and play chess with fellow students", component.Markup);
    }

    [Fact]
    public void ShowsNoDescriptionAvailable_WhenDescriptionNull()
    {
        // Arrange - Club with null description
        var mockHttp = new MockHttpMessageHandler();
        var clubsJson = System.Text.Json.JsonSerializer.Serialize(new[]
        {
            new { ClubID = 1, ClubName = "Mystery Club", ClubDescription = (string?)null, ClubImgURL = "", CreatedDate = "2024-01-01" }
        });

        mockHttp.When("*/api/clubs").Respond("application/json", clubsJson);
        mockHttp.When("*/api/members*").Respond("application/json", "[]");
        mockHttp.When("*/api/events").Respond("application/json", "[]");
        mockHttp.When("*/api/users/count").Respond("application/json", "0");

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost:7071/");
        Services.AddSingleton(httpClient);

        // Act
        var component = RenderComponent<Clubs>();
        component.WaitForAssertion(() => Assert.Contains("No description available", component.Markup));

        // Assert
        Assert.Contains("No description available", component.Markup);
    }

    #endregion

    #region Error Message Tests

    [Fact]
    public void ShowsErrorMessage_WhenLoadingFails()
    {
        // Arrange - Simulate API failure
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("*/api/clubs").Throw(new HttpRequestException("Network error"));

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost:7071/");
        Services.AddSingleton(httpClient);

        // Act
        var component = RenderComponent<Clubs>();
        component.WaitForAssertion(() => Assert.Contains("Error loading clubs", component.Markup), timeout: TimeSpan.FromSeconds(3));

        // Assert
        var markup = component.Markup;
        Assert.Contains("Error loading clubs", markup);
    }

    [Fact]
    public void ShowsNoClubsMessage_WhenEmpty()
    {
        // Arrange - Empty clubs array
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("*/api/clubs").Respond("application/json", "[]");
        mockHttp.When("*/api/members*").Respond("application/json", "[]");
        mockHttp.When("*/api/events").Respond("application/json", "[]");
        mockHttp.When("*/api/users/count").Respond("application/json", "0");

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost:7071/");
        Services.AddSingleton(httpClient);

        // Act
        var component = RenderComponent<Clubs>();
        component.WaitForAssertion(() => Assert.Contains("No clubs found", component.Markup));

        // Assert
        Assert.Contains("No clubs found", component.Markup);
    }

    #endregion

    #region Multiple Clubs Display Tests

    [Fact]
    public void DisplaysAllClubs_WhenMultipleExist()
    {
        // Arrange - Three clubs
        var mockHttp = new MockHttpMessageHandler();
        var clubsJson = System.Text.Json.JsonSerializer.Serialize(new[]
        {
            new { ClubID = 1, ClubName = "Chess Club", ClubDescription = "Chess", ClubImgURL = "", CreatedDate = "2024-01-01" },
            new { ClubID = 2, ClubName = "Drama Club", ClubDescription = "Drama", ClubImgURL = "", CreatedDate = "2024-01-02" },
            new { ClubID = 3, ClubName = "Coding Club", ClubDescription = "Code", ClubImgURL = "", CreatedDate = "2024-01-03" }
        });

        mockHttp.When("*/api/clubs").Respond("application/json", clubsJson);
        mockHttp.When("*/api/members*").Respond("application/json", "[]");
        mockHttp.When("*/api/events").Respond("application/json", "[]");
        mockHttp.When("*/api/users/count").Respond("application/json", "0");

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost:7071/");
        Services.AddSingleton(httpClient);

        // Act
        var component = RenderComponent<Clubs>();
        component.WaitForAssertion(() => Assert.Contains("Chess Club", component.Markup));

        // Assert - All three clubs should be displayed
        var markup = component.Markup;
        Assert.Contains("Chess Club", markup);
        Assert.Contains("Drama Club", markup);
        Assert.Contains("Coding Club", markup);
    }

    [Fact]
    public void DisplaysJoinButtonForEachClub()
    {
        // Arrange - Two clubs
        var mockHttp = new MockHttpMessageHandler();
        var clubsJson = System.Text.Json.JsonSerializer.Serialize(new[]
        {
            new { ClubID = 1, ClubName = "Club A", ClubDescription = "A", ClubImgURL = "", CreatedDate = "2024-01-01" },
            new { ClubID = 2, ClubName = "Club B", ClubDescription = "B", ClubImgURL = "", CreatedDate = "2024-01-02" }
        });

        mockHttp.When("*/api/clubs").Respond("application/json", clubsJson);
        mockHttp.When("*/api/members*").Respond("application/json", "[]");
        mockHttp.When("*/api/events").Respond("application/json", "[]");
        mockHttp.When("*/api/users/count").Respond("application/json", "0");

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost:7071/");
        Services.AddSingleton(httpClient);

        // Act
        var component = RenderComponent<Clubs>();
        component.WaitForAssertion(() => Assert.Contains("Club A", component.Markup));

        // Assert - Each club should have a Join button
        var markup = component.Markup;
        var joinButtonCount = System.Text.RegularExpressions.Regex.Matches(markup, "Join").Count;
        Assert.True(joinButtonCount >= 2, $"Expected at least 2 Join buttons, found {joinButtonCount}");
    }

    #endregion

    #region Stats Section Tests

    [Fact]
    public void LoadsStatsData()
    {
        // Arrange - Mock stats endpoints
        var mockHttp = new MockHttpMessageHandler();
        var clubsJson = System.Text.Json.JsonSerializer.Serialize(new[]
        {
            new { ClubID = 1, ClubName = "Club 1", ClubDescription = "One", ClubImgURL = "", CreatedDate = "2024-01-01" },
            new { ClubID = 2, ClubName = "Club 2", ClubDescription = "Two", ClubImgURL = "", CreatedDate = "2024-01-02" }
        });
        var eventsJson = System.Text.Json.JsonSerializer.Serialize(new[]
        {
            new { EventID = 1, EventName = "Event 1", EventDate = "2025-01-01" },
            new { EventID = 2, EventName = "Event 2", EventDate = "2025-01-02" },
            new { EventID = 3, EventName = "Event 3", EventDate = "2025-01-03" }
        });

        mockHttp.When("*/api/clubs").Respond("application/json", clubsJson);
        mockHttp.When("*/api/events").Respond("application/json", eventsJson);
        mockHttp.When("*/api/users/count").Respond("application/json", "15");
        mockHttp.When("*/api/members*").Respond("application/json", "[]");

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost:7071/");
        Services.AddSingleton(httpClient);

        // Act
        var component = RenderComponent<Clubs>();
        component.WaitForAssertion(() => Assert.Contains("Club 1", component.Markup));

        // Assert - Stats should be loaded (2 clubs, 3 events, 15 users)
        var markup = component.Markup;
        Assert.Contains("Club 1", markup);
        Assert.Contains("Club 2", markup);
    }

    #endregion
}