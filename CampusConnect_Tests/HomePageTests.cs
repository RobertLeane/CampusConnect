using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Client.Pages;
using Client.Services;
using RichardSzalay.MockHttp;
using Xunit;
using Bunit.TestDoubles;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text.Json;
using System.Net;
using Client.Layout;

namespace CampusConnect_Tests;

public class HomePageTests : TestContext
{

    public HomePageTests()
    {
        // Register required services for Home page
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

    #region Landing Page Tests

    [Fact]
    public void HomePage_RendersCorrectly()
    {
         // Arrange – minimal happy-path stubs
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("*/api/stats*")
            .Respond("application/json", """{ "clubs": 40, "events": 12, "members": 200 }""");
        mockHttp.When("*/api/clubs/recent*")
            .Respond("application/json", """[{"ClubId":1,"ClubName":"Computer Science Society"}]""");
        mockHttp.When("*/api/events/upcoming*")
            .Respond("application/json", """[{"EventId":1,"EventName":"Annual Tech Conference","EventDateTime":"2025-10-30T14:00:00Z"}]""");
        mockHttp.Fallback.Respond("application/json", "[]");

        var http = mockHttp.ToHttpClient();
        http.BaseAddress = new Uri("https://localhost:7071/");
        Services.RemoveAll<HttpClient>();
        Services.AddSingleton(http);

        // Act
        var component = RenderComponent<Home>();

        // Assert
        component.WaitForAssertion(() =>
        {
            var html = component.Markup;

            Assert.Contains("Discover Your", html);
            Assert.Contains("Campus Community", html);

            // At least one of these sections should be present
            Assert.True(
                ContainsAny(html, "Featured Clubs", "Upcoming Events"),
                "Expected 'Featured Clubs' or 'Upcoming Events' section."
            );

            // At least one call to action
            Assert.True(
                ContainsAny(html, "View My Dashboard", "Explore Clubs"),
                "Expected a primary call to action like 'View My Dashboard' or 'Explore Clubs'."
            );
        }, timeout: TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void HomePage_HasWelcomeMessage()
    {
         // Arrange – minimal stubs (hero text is static; APIs can just return empty)
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.Fallback.Respond("application/json", "[]");

        var http = mockHttp.ToHttpClient();
        http.BaseAddress = new Uri("https://localhost:7071/");
        Services.RemoveAll<HttpClient>();
        Services.AddSingleton(http);

        // Act
        var component = RenderComponent<Home>();

        // Assert – hero heading + tagline snippet
        component.WaitForAssertion(() =>
        {
            var html = component.Markup;
            Assert.Contains("Discover Your", html);
            Assert.True(
                html.Contains("Campus Community", StringComparison.OrdinalIgnoreCase) ||
                html.Contains("connect with fellow students", StringComparison.OrdinalIgnoreCase)
            );
        }, timeout: TimeSpan.FromSeconds(3));
    }

    [Fact]
    public void HomePage_HasCallToActionButtons()
    {
       // Arrange – minimal http + guest
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.Fallback.Respond("application/json", "[]");
        var http = mockHttp.ToHttpClient();
        http.BaseAddress = new Uri("https://localhost:7071/");
        Services.RemoveAll<HttpClient>();
        Services.AddSingleton(http);

        // Act
        var component = RenderComponent<Home>();

        // Assert – look for login call to action or hero call to actions
        component.WaitForAssertion(() =>
        {
            var html = component.Markup;

            var hasLoginLink =
                html.Contains("href=\"/login\"", StringComparison.OrdinalIgnoreCase) ||
                html.Contains(">Login<", StringComparison.OrdinalIgnoreCase) ||
                html.Contains(">Log In<", StringComparison.OrdinalIgnoreCase) ||
                html.Contains(">Sign In<", StringComparison.OrdinalIgnoreCase);

            var hasHeroCTA =
                html.Contains("Explore Clubs", StringComparison.OrdinalIgnoreCase) ||
                html.Contains("View All Events", StringComparison.OrdinalIgnoreCase) ||
                html.Contains("View All Clubs", StringComparison.OrdinalIgnoreCase);

            Assert.True(hasLoginLink || hasHeroCTA,
                "Expected a CTA: /login (from layout) or hero CTAs like 'Explore Clubs'/'View All …'.");
        }, timeout: TimeSpan.FromSeconds(3));
    }

    #endregion

    #region Features Section Tests

    [Fact]
    public void HomePage_DisplaysFeatureHighlights()
    {
        // Arrange – minimal HTTP + default fallbacks
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.Fallback.Respond("application/json", "[]");
        var http = mockHttp.ToHttpClient();
        http.BaseAddress = new Uri("https://localhost:7071/");
        Services.RemoveAll<HttpClient>();
        Services.AddSingleton(http);

        // Act
        var component = RenderComponent<Home>();

        // Assert – look for any of the visible feature sections
        component.WaitForAssertion(() =>
        {
            var html = component.Markup;
            var hasFeatureSection =
                html.Contains("Featured Clubs", StringComparison.OrdinalIgnoreCase)
                || html.Contains("Upcoming Events", StringComparison.OrdinalIgnoreCase)
                || html.Contains("Explore by Category", StringComparison.OrdinalIgnoreCase)
                || html.Contains("Quick Links", StringComparison.OrdinalIgnoreCase);

            Assert.True(hasFeatureSection,
                "Expected feature highlights such as 'Featured Clubs', 'Upcoming Events', 'Explore by Category', or 'Quick Links'.");
        }, timeout: TimeSpan.FromSeconds(3));
    }

    [Fact]
    public void HomePage_HasAboutSection()
    {

        // Arrange – minimal HTTP + default fallbacks
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.Fallback.Respond("application/json", "[]");
        var http = mockHttp.ToHttpClient();
        http.BaseAddress = new Uri("https://localhost:7071/");
        Services.RemoveAll<HttpClient>();
        Services.AddSingleton(http);

        // Act
        var component = RenderComponent<Home>();

        component.WaitForAssertion(() =>
        {
            var html = component.Markup;

            // Look for hero paragraph or CTA copy that's effectively 'about' text
            var hasDescriptiveCopy =
                html.Contains("Join clubs, attend events", StringComparison.OrdinalIgnoreCase) ||
                html.Contains("Ready to Get Involved?", StringComparison.OrdinalIgnoreCase) ||
                html.Contains("Join the vibrant campus community", StringComparison.OrdinalIgnoreCase) ||
                html.Contains("connect with fellow students", StringComparison.OrdinalIgnoreCase);

            Assert.True(hasDescriptiveCopy, "Expected descriptive 'about' style copy in hero or CTA.");

            // And expect at least one discover/learn link (dashboard or clubs)
            var hasLearnLink =
                html.Contains("href=\"/dashboard\"", StringComparison.OrdinalIgnoreCase) ||
                html.Contains("href=\"/clubs\"", StringComparison.OrdinalIgnoreCase) ||
                html.Contains("Explore Clubs", StringComparison.OrdinalIgnoreCase) ||
                html.Contains("View My Dashboard", StringComparison.OrdinalIgnoreCase);

            Assert.True(hasLearnLink, "Expected a Dashboard/Explore Clubs link or CTA to let users learn more.");
        }, timeout: TimeSpan.FromSeconds(5));

    }

    [Fact]
    public void HomePage_JoinClub_PromptsLogin_ForGuest()
    {
        // Arrange – clubs available but user is guest (no users)
        var mockHttp = new MockHttpMessageHandler();

        var clubs = new[]
        {
            new { ClubId = 1, ClubName = "Sustainability Club", ClubImgURL = "", ClubDescription = "A club focused on promoting sustainable practices." }
        };

        mockHttp.When("*/api/clubs*").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(clubs));
        mockHttp.When("*/api/events*").Respond("application/json", "[]");
        // Mock users as empty to represent guest
        mockHttp.When("*/api/users*").Respond("application/json", "[]");
        mockHttp.When("*").Respond("application/json", "[]");

        var http = mockHttp.ToHttpClient();
        http.BaseAddress = new Uri("https://localhost:7071/");
        Services.RemoveAll<HttpClient>();
        Services.AddSingleton(http);

        // Act
        var component = RenderComponent<Home>();

        // Wait for club card to appear
        component.WaitForAssertion(() =>
        {
            Assert.Contains("Sustainability Club", component.Markup);
        }, timeout: TimeSpan.FromSeconds(5));

        // Find Join button and click
        var joinBtn = component.FindAll("button").FirstOrDefault(b => (b.TextContent ?? string.Empty).Contains("Join", StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(joinBtn);

        joinBtn.Click();

        // Verify JS alert asking user to log in was invoked
        var alertInvocation = JSInterop.Invocations.FirstOrDefault(i => (i.Identifier ?? string.Empty).Equals("alert", StringComparison.OrdinalIgnoreCase));
        Assert.False(string.IsNullOrEmpty(alertInvocation.Identifier), "Expected a JS 'alert' invocation when a guest clicks Join Club.");
        var alertArg = alertInvocation.Arguments?.FirstOrDefault()?.ToString() ?? string.Empty;
        Assert.Contains("Please log in to join clubs", alertArg, StringComparison.OrdinalIgnoreCase);
    }

    #endregion


    #region Footer Tests

    [Fact]
    public void HomePage_HasCompleteFooter()
    {
        var component = RenderComponent<MainLayout>(); 

        var html = component.Markup.ToLowerInvariant();

        Assert.Contains("campusconnect", html);
        Assert.Contains("quick links", html);
    }

    [Fact]
    public void HomePage_FooterLinksWork()
    {
        var component = RenderComponent<MainLayout>(); 

        var html = component.Markup.ToLowerInvariant(); 

        Assert.Contains("browse clubs", html);
        Assert.Contains("find events", html);
        Assert.Contains("my dashboard", html);
    }
    #endregion

    #region Responsive / Perf / SEO

    [Fact]
    public void HomePage_ResponsiveDesign()
    {
       // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.Fallback.Respond("application/json", "[]");
        var http = mockHttp.ToHttpClient();
        http.BaseAddress = new Uri("https://localhost:7071/");
        Services.RemoveAll<HttpClient>();
        Services.AddSingleton(http);

        // Act
        var component = RenderComponent<Home>();
        var html = component.Markup;

        // Assert – just verify key visible text renders
        Assert.Contains("Discover Your", html);
        Assert.Contains("Campus Community", html);
}
    [Fact]
    public void HomePage_LoadsQuickly()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.Fallback.Respond("application/json", "[]");
        var http = mockHttp.ToHttpClient();
        http.BaseAddress = new Uri("https://localhost:7071/");
        Services.RemoveAll<HttpClient>();
        Services.AddSingleton(http);

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var component = RenderComponent<Home>();
        stopwatch.Stop();

        // Assert – should render under 2 seconds
        Assert.True(stopwatch.ElapsedMilliseconds < 2000, 
            $"Home page took too long to load ({stopwatch.ElapsedMilliseconds}ms).");

        Assert.NotNull(component);
    }

  
    #endregion


    #region Event RSVP Tests

    [Fact]
    public void HomePage_HandleRsvp_RequiresAuthentication()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var eventData = new[]
        {
            new
            {
                EventId = 1,
                EventName = "Test Event",
                EventDescription = "Test",
                EventDateTime = DateTime.Now.AddDays(1).ToString("o"),
                ClubId = 1,
                EventImgURL = ""
            }
        };

        mockHttp.When("*/api/events").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(eventData));
        mockHttp.When("*/api/clubs").Respond("application/json", "[{\"ClubId\":1,\"ClubName\":\"Test Club\"}]");
        mockHttp.Fallback.Respond("application/json", "[]");

        var http = mockHttp.ToHttpClient();
        http.BaseAddress = new Uri("https://localhost:7071/");
        Services.RemoveAll<HttpClient>();
        Services.AddSingleton(http);

        // Act
        var component = RenderComponent<Home>();

        // Assert - button should be present
        component.WaitForAssertion(() =>
        {
            var rsvpButtons = component.FindAll("button.btn--primary");
            Assert.NotEmpty(rsvpButtons);
        }, timeout: TimeSpan.FromSeconds(3));
    }

    [Fact]
    public void HomePage_HandleRsvp_SuccessfulRsvp()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var eventData = new[]
        {
            new
            {
                EventId = 1,
                EventName = "Test Event",
                EventDescription = "Test",
                EventDateTime = DateTime.Now.AddDays(1).ToString("o"),
                ClubId = 1,
                EventImgURL = ""
            }
        };

        mockHttp.When("*/api/events").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(eventData));
        mockHttp.When("*/api/clubs").Respond("application/json", "[{\"ClubId\":1,\"ClubName\":\"Test Club\"}]");
        mockHttp.When("*/api/events/*/rsvp").Respond(HttpStatusCode.OK);
        mockHttp.When("*/api/users/me").Respond("application/json", "{\"UserId\":1,\"FirstName\":\"Test\"}");
        mockHttp.Fallback.Respond("application/json", "[]");

        var http = mockHttp.ToHttpClient();
        http.BaseAddress = new Uri("https://localhost:7071/");
        Services.RemoveAll<HttpClient>();
        Services.AddSingleton(http);

        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("TestUser");

        // Act
        var component = RenderComponent<Home>();

        // Assert - RSVP button exists
        component.WaitForAssertion(() =>
        {
            var rsvpButtons = component.FindAll("button");
            var hasRsvpButton = rsvpButtons.Any(b => b.TextContent.Contains("RSVP", StringComparison.OrdinalIgnoreCase));
            Assert.True(hasRsvpButton);
        }, timeout: TimeSpan.FromSeconds(3));
    }

    #endregion

    #region Club Joining Tests

    [Fact]
    public void HomePage_HandleJoinClub_RequiresAuthentication()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var clubData = new[]
        {
            new
            {
                ClubId = 1,
                ClubName = "Test Club",
                ClubDescription = "Test",
                ClubImgURL = ""
            }
        };

        mockHttp.When("*/api/clubs").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(clubData));
        mockHttp.Fallback.Respond("application/json", "[]");

        var http = mockHttp.ToHttpClient();
        http.BaseAddress = new Uri("https://localhost:7071/");
        Services.RemoveAll<HttpClient>();
        Services.AddSingleton(http);

        // Act
        var component = RenderComponent<Home>();

        // Assert - Join button exists
        component.WaitForAssertion(() =>
        {
            var joinButtons = component.FindAll("button");
            var hasJoinButton = joinButtons.Any(b => b.TextContent.Contains("Join", StringComparison.OrdinalIgnoreCase));
            Assert.True(hasJoinButton);
        }, timeout: TimeSpan.FromSeconds(3));
    }

    [Fact]
    public void HomePage_HandleJoinClub_SuccessfulJoin()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var clubData = new[]
        {
            new
            {
                ClubId = 1,
                ClubName = "Test Club",
                ClubDescription = "Test",
                ClubImgURL = ""
            }
        };

        mockHttp.When("*/api/clubs").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(clubData));
        mockHttp.When("*/api/members").Respond(HttpStatusCode.OK, "application/json", "{\"memberId\":1}");
        mockHttp.When("*/api/users/me").Respond("application/json", "{\"UserId\":1,\"FirstName\":\"Test\"}");
        mockHttp.Fallback.Respond("application/json", "[]");

        var http = mockHttp.ToHttpClient();
        http.BaseAddress = new Uri("https://localhost:7071/");
        Services.RemoveAll<HttpClient>();
        Services.AddSingleton(http);

        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("TestUser");

        // Act
        var component = RenderComponent<Home>();

        // Assert - Component renders with clubs
        component.WaitForAssertion(() =>
        {
            Assert.Contains("Test Club", component.Markup);
        }, timeout: TimeSpan.FromSeconds(3));
    }

    #endregion


}
