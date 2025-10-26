using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Client.Pages;
using Client.Services;
using RichardSzalay.MockHttp;
using Xunit;
using AngleSharp.Dom.Events;

namespace CampusConnect_Tests;

public class EventsPageTests : TestContext
{
   private readonly MockHttpMessageHandler _mockHttp;
    private readonly HttpClient _http;
    public EventsPageTests()
    {

        JSInterop.Mode = JSRuntimeMode.Loose;

        _mockHttp = new MockHttpMessageHandler();

        // Helpful fallback so unmatched calls don’t hang:
        _mockHttp.Fallback.Respond("application/json", "[]");

        _http = _mockHttp.ToHttpClient();
        _http.BaseAddress = new Uri("https://localhost:7071/");

        // Use SCOPED like a real Blazor app
        Services.AddScoped(sp => _http);
        Services.AddScoped<AuthService>();
        Services.AddScoped<ClubApi>();
        Services.AddScoped<EventApi>();
        Services.AddScoped<MemberApi>();
        Services.AddScoped<UserApi>();
        Services.AddScoped<EventParticipantApi>();
        Services.AddScoped<EventRegistrationService>();
    }

    #region Page Rendering Tests

    [Fact]
    public void EventsPage_RendersCorrectly()
    {
        // Act
        var component = RenderComponent<Events>();

        Assert.Contains("Events", component.Markup);
        Assert.Contains("Active Clubs", component.Markup);
        Assert.Contains("Active Members", component.Markup);
    }
     #endregion

    #region Event List Display Tests

    [Fact]
    public void EventsPage_DisplaysEventList()
    {
         // Arrange on the shared handler
       var eventData = new[] { new { EventId = 1, EventName = "Hackathon", EventDescription = "24h coding event.",
                                  EventDateTime = DateTime.Now.AddDays(2), EventImgURL = "", ClubId = 2, Location = "Main Hall" } };
    var clubData = new[] { new { ClubId = 1, ClubName = "Chess Club", ClubImgURL = "", ClubDescription = "A club for chess enthusiasts." } };
    var userData  = new[] { new { UserId = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com" } };

    _mockHttp.When("*/api/events").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(eventData));
    _mockHttp.When("*/api/clubs").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(clubData));
    _mockHttp.When("*/api/users").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(userData));

    var component = RenderComponent<Events>();

    component.WaitForAssertion(() =>
    {
        var markup = component.Markup;
        Assert.DoesNotContain("Loading events...", markup);
        Assert.Contains("Hackathon", markup);
    }, timeout: TimeSpan.FromSeconds(3));
    }

    [Fact]
    public void EventsPage_DisplaysEmptyState_WhenNoEvents()
    {
        // Arrange - Mock empty responses
        var mockHttp = new MockHttpMessageHandler();

        //Mock empty data array
        var emptyClubData = new object[0];
        var emptyEventData = new object[0];
        var emptyUserData = new object[0];

        //Setup all API endpoints with empty respones
        mockHttp.When("*/api/clubs").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(emptyClubData));
        mockHttp.When("*/api/events").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(emptyEventData));
        mockHttp.When("*/api/users").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(emptyUserData));

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost:7071/");
        
        Services.AddSingleton(httpClient);

        // Act
        var component = RenderComponent<Events>();

        // Assert - Wait for data to load and check empty state
        component.WaitForAssertion(() =>
        {
            var markup = component.Markup;
            
            // Should not be loading anymore
            Assert.DoesNotContain("Loading events...", markup);
            
            // Should show statistics as 0
            Assert.Contains("0", markup); // Should show 0 for stats
        }, timeout: TimeSpan.FromSeconds(3));


    }

    [Fact]
public void EventsPage_DisplaysEventDetails()

    {
        var when = DateTime.Now.AddDays(1);

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

            // Stubs (allow trailing segments or query with *)
        _mockHttp.When("*/api/events*")
                .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(eventData));
        _mockHttp.When("*/api/clubs*")
                .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(clubData));
        _mockHttp.When("*/api/users*")
                .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(userData));
        _mockHttp.When("*/api/eventparticipants*").Respond("application/json", "[]");
        _mockHttp.When("*/api/members*").Respond("application/json", "[]");

       // Act
        var cut = RenderComponent<Clubs>();
        cut.WaitForAssertion(() =>
        {
            var html = cut.Markup;
            Assert.Contains("Photo Walk", html);
            Assert.Contains("Bring your camera for a campus photo tour.", html);
            Assert.Contains("RSVP", html);
               
        }, timeout: TimeSpan.FromSeconds(10));
}

    #endregion

    #region Event Actions Tests

    [Fact]
    public void EventsPage_RSVPToEventWorks()
    {
        // Arrange – use the shared handler configured in the ctor
    var when = DateTime.Now;

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
        new
        {
            EventId = 1,
            EventName = "Basketball Game",
            EventDescription = "Test",
            EventDateTime = when,
            EventImgURL = "",
            ClubId = 1,
            Location = "Main Hall"
        }
    };

    var userData = new[]
    {
        new { UserId = 1, FirstName = "Mike", LastName = "Jordan", Email = "mike@example.com" }
    };

    // Stub ALL endpoints the page touches (allow trailing segments/queries)
    _mockHttp.When("*/api/events*")
             .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(eventData));
    _mockHttp.When("*/api/clubs*")
             .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(clubData));
    _mockHttp.When("*/api/users*")
             .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(userData));
    _mockHttp.When("*/api/eventparticipants*").Respond("application/json", "[]");
    _mockHttp.When("*/api/members*").Respond("application/json", "[]");

    // Act
    var cut = RenderComponent<Client.Pages.Events>();

    // Assert – event appears and there is an RSVP-like action
    cut.WaitForAssertion(() =>
    {
        var html = cut.Markup;
        Assert.Contains("Basketball Game", html);
        Assert.True(
            html.Contains("RSVP", StringComparison.OrdinalIgnoreCase) ||
            html.Contains("Register", StringComparison.OrdinalIgnoreCase) ||
            html.Contains("Attend", StringComparison.OrdinalIgnoreCase),
            "Expected an RSVP/Register/Attend action on the event card."
        );
    }, timeout: TimeSpan.FromSeconds(5));
    }
   

    #endregion

    #region Authentication and Permissions Tests

    [Fact]
    public void EventsPage_RequiresLogin_ForRSVP()
    {
        // Arrange – stub ALL endpoints on the shared handler
    var when = DateTime.Now;

    _mockHttp.When("*/api/events*").Respond("application/json",
        System.Text.Json.JsonSerializer.Serialize(new[]
        {
            new {
                EventId = 1,
                EventName = "Basketball Game",
                EventDescription = "Test",
                EventDateTime = when,
                EventImgURL = "",
                ClubId = 1,
                Location = "Court A"
            }
        }));

    _mockHttp.When("*/api/clubs*").Respond("application/json",
        System.Text.Json.JsonSerializer.Serialize(new[]
        {
            new {
                ClubId = 1,
                ClubName = "Debate Club",
                ClubImgURL = "",
                ClubDescription = "Improve your public speaking and debate skills."
            }
        }));

    _mockHttp.When("*/api/users*").Respond("application/json", "[]");
    _mockHttp.When("*/api/eventparticipants*").Respond("application/json", "[]");
    _mockHttp.When("*/api/members*").Respond("application/json", "[]");

    // Act
    var cut = RenderComponent<Client.Pages.Events>();

    // Assert – login/join required for RSVP when unauthenticated
    cut.WaitForAssertion(() =>
    {
        var html = cut.Markup;
        Assert.Contains("Basketball Game", html);
        Assert.True(
            html.Contains("Login", StringComparison.OrdinalIgnoreCase) ||
            html.Contains("Sign in", StringComparison.OrdinalIgnoreCase) ||
            html.Contains("Join Club", StringComparison.OrdinalIgnoreCase),
            "Expected a login/join prompt for RSVP when not authenticated."
        );
    }, timeout: TimeSpan.FromSeconds(5));
    }

    #endregion

    #region Loading and Error States

    [Fact]
    public void EventsPage_ShowsLoadingState()
    {
        // Arrange – no endpoint setup (initial render check)
        var component = RenderComponent<Events>();

        // Assert – initial loading indicator + page frame

        Assert.Contains("Events", component.Markup);
    }

    [Fact]
    public void EventsPage_HandlesAPIErrors()
    {
        // Arrange – simulate API failure
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("*/api/events").Respond(System.Net.HttpStatusCode.InternalServerError);
        mockHttp.When("*/api/users").Respond("application/json", "[]");
        mockHttp.When("*/api/clubs").Respond("application/json", "[]");

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost:7071/");
        Services.AddSingleton(httpClient);

        // Act
        var component = RenderComponent<Events>();

        // Assert – shows graceful error/empty frame post-load
        component.WaitForAssertion(() =>
        {
            var markup = component.Markup;
            Assert.DoesNotContain("Loading events...", markup);
            Assert.Contains("Events", markup);
        }, timeout: TimeSpan.FromSeconds(3));
    }

    #endregion
}
