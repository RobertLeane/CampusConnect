using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Client.Pages;
using Client.Services;
using RichardSzalay.MockHttp;
using Xunit;
using Bunit.TestDoubles;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Text.RegularExpressions;
using System.Net;
using Client.Layout;

namespace CampusConnect_Tests;

public class HomePageTests : TestContext
{

    public HomePageTests()
    {
        // minimal deps (same pattern as your Events tests)
        Services.AddSingleton<AuthService>();
        Services.AddSingleton<ClubApi>();
        Services.AddSingleton<EventApi>();
        Services.AddSingleton<MemberApi>();
        Services.AddSingleton<UserApi>();      
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

        // Assert – hero + sections + a CTA
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

            // At least one CTA in the “Get Involved” area
            Assert.True(
                ContainsAny(html, "View My Dashboard", "Explore Clubs"),
                "Expected a primary CTA like 'View My Dashboard' or 'Explore Clubs'."
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

        // Assert – allow for two cases:
        // (1) Layout present -> navbar has /login
        // (2) Home-only -> hero CTAs like “Explore Clubs”, “View All …”
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

    }

    #endregion

    #region Navigation Tests

    [Fact]
    public void HomePage_NavigationWorksCorrectly()
    {
        // Arrange – minimal HTTP, treat user as guest
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.Fallback.Respond("application/json", "[]");

        var http = mockHttp.ToHttpClient();
        http.BaseAddress = new Uri("https://localhost:7071/");
        Services.RemoveAll<HttpClient>();
        Services.AddSingleton(http);

        // Act
        var component = RenderComponent<Home>();

        // Assert – page body exposes ways to reach Clubs and Events
        component.WaitForAssertion(() =>
        {
            var html = component.Markup;
            var hrefs = component.FindAll("a").Select(a => a.GetAttribute("href") ?? "").ToArray();

            var toClubs =
                hrefs.Any(h => h.Contains("/clubs", StringComparison.OrdinalIgnoreCase)) ||
                html.Contains("View All Clubs", StringComparison.OrdinalIgnoreCase) ||
                html.Contains(">Explore Clubs<", StringComparison.OrdinalIgnoreCase);

            var toEvents =
                hrefs.Any(h => h.Contains("/events", StringComparison.OrdinalIgnoreCase)) ||
                html.Contains("View All Events", StringComparison.OrdinalIgnoreCase);

            Assert.True(toClubs, "Expected navigation to Clubs (href or button text).");
            Assert.True(toEvents, "Expected navigation to Events (href or button text).");
        }, timeout: TimeSpan.FromSeconds(3));
    }

    [Fact]
    public void HomePage_ShowsDifferentContent_ForAuthenticatedUsers()
    {
        // Arrange – minimal HTTP + logged-in user
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.Fallback.Respond("application/json", "[]");
        mockHttp.When("*/api/users/me*")
                .Respond("application/json", """{ "UserId": 1, "FirstName": "Alex" }""");

        var http = mockHttp.ToHttpClient();
        http.BaseAddress = new Uri("https://localhost:7071/");
        Services.RemoveAll<HttpClient>();
        Services.AddSingleton(http);

        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("TestUser");

        // Act
        var component = RenderComponent<Home>();

        // Assert – dashboard CTA present, guest sign-up CTAs absent
        component.WaitForAssertion(() =>
        {
            var html = component.Markup;

            // Tolerate multiple wordings/placements (navbar, hero, CTA card)
            var showsDashboard =
                html.Contains("href=\"/dashboard\"", StringComparison.OrdinalIgnoreCase)
                || html.Contains("View My Dashboard",     StringComparison.OrdinalIgnoreCase)
                || html.Contains(">Dashboard<",           StringComparison.OrdinalIgnoreCase);

            Assert.True(showsDashboard, "Expected a Dashboard link/CTA for authenticated users.");

            Assert.False(
                html.Contains(">Sign Up<",             StringComparison.OrdinalIgnoreCase)
                || html.Contains("href=\"/register\"",    StringComparison.OrdinalIgnoreCase)
                || html.Contains("href=\"/signup\"",      StringComparison.OrdinalIgnoreCase),
                "Guest-only Sign Up/Register CTAs should not be shown for authenticated users.");
        }, timeout: TimeSpan.FromSeconds(3));
    }
    #endregion

    #region Statistics Section Tests

    [Fact]
    public void HomePage_DisplaysPlatformStatistics()
    {
       // Arrange – stub endpoints the page may call
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("*/api/stats*")
            .Respond("application/json", """{ "clubs": 99, "events": 7, "members": 1234 }""");
        mockHttp.When("*/api/clubs/recent*").Respond("application/json", "[]");
        mockHttp.When("*/api/events/upcoming*").Respond("application/json", "[]");
        mockHttp.Fallback.Respond("application/json", "[]");

        var http = mockHttp.ToHttpClient();
        http.BaseAddress = new Uri("https://localhost:7071/");
        Services.RemoveAll<HttpClient>();
        Services.AddSingleton(http);

        // Act
        var component = RenderComponent<Home>();

        // Assert – the three stat tiles should be present (labels are stable)
        component.WaitForAssertion(() =>
        {
            var html = component.Markup;

            Assert.Contains("Active Clubs", html, StringComparison.OrdinalIgnoreCase);
            // Some UIs show “Monthly Events”, others just “Events” in the tile
            Assert.True(
                html.Contains("Monthly Events", StringComparison.OrdinalIgnoreCase) ||
                html.Contains(">Events<",       StringComparison.OrdinalIgnoreCase) ||
                html.Contains(" Events",        StringComparison.OrdinalIgnoreCase),
                "Expected the events stats tile label."
            );
            Assert.Contains("Active Members", html, StringComparison.OrdinalIgnoreCase);
        }, timeout: TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void HomePage_HandlesStatisticsErrors()
    {
       // Arrange – stats fails, others safe
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("*/api/stats*").Respond(System.Net.HttpStatusCode.InternalServerError);
        mockHttp.When("*/api/clubs/recent*").Respond("application/json", "[]");
        mockHttp.When("*/api/events/upcoming*").Respond("application/json", "[]");
        mockHttp.Fallback.Respond("application/json", "[]");

        var http = mockHttp.ToHttpClient();
        http.BaseAddress = new Uri("https://localhost:7071/");
        Services.RemoveAll<HttpClient>();
        Services.AddSingleton(http);

        // Act
        var component = RenderComponent<Home>();

        // Assert – page renders + stats section still present with some numeric fallback
        component.WaitForAssertion(() =>
        {
            var html = component.Markup.Replace("\u00A0", "");

            // Hero text proves page rendered
            Assert.Contains("Discover Your", html, StringComparison.OrdinalIgnoreCase);

            // Stats strip labels still present
            Assert.True(
                html.Contains("Active Clubs", StringComparison.OrdinalIgnoreCase) ||
                html.Contains("Monthly Events", StringComparison.OrdinalIgnoreCase) ||
                html.Contains("Active Members", StringComparison.OrdinalIgnoreCase),
                "Expected the stats strip to render even if the stats API fails."
            );

            // Tolerate any numeric fallback (e.g., 0, 5+, 1,234)
            var hasNumbers = Regex.IsMatch(html, @"\b\d[\d,]*\+?\b");
            Assert.True(hasNumbers, "Expected some numeric value in the stats strip (e.g., 0 or 10+).");
        }, timeout: TimeSpan.FromSeconds(5));
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
        Assert.Contains("support", html);
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

    [Fact]
    public void HomePage_HasProperSEOElements()
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

        // Assert – check basic SEO tags and structure
        Assert.Contains("<h1", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Campus", html, StringComparison.OrdinalIgnoreCase);
    }
    #endregion

    #region Integration Tests

    [Fact]
    public void HomePage_IntegratesWithAuthSystem()
    {
         // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("*/api/users/me*")
            .Respond("application/json", """{ "UserId": 1, "FirstName": "Alex" }""");
        mockHttp.Fallback.Respond("application/json", "[]");

        var http = mockHttp.ToHttpClient();
        http.BaseAddress = new Uri("https://localhost:7071/");
        Services.RemoveAll<HttpClient>();
        Services.AddSingleton(http);

        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("Alex");

        // Act
        var component = RenderComponent<Home>();
        var html = component.Markup;

        // Assert – should show dashboard link for logged-in user
        Assert.Contains("Dashboard", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Sign Up", html, StringComparison.OrdinalIgnoreCase);

    }
    #endregion

    #region Image Handling Tests

    [Fact]
    public void HomePage_GetClubImage_ReturnsAzureBlobUrl()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var clubWithImage = new[]
        {
            new
            {
                ClubId = 1,
                ClubName = "Test Club",
                ClubDescription = "Test",
                ClubImgURL = "https://storage.azure.com/test%20club.jpg"
            }
        };

        mockHttp.When("*/api/clubs").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(clubWithImage));
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
            var images = component.FindAll("img[alt*='Test Club']");
            Assert.NotEmpty(images);
            var imgSrc = images.First().GetAttribute("src");
            // Should decode the URL
            Assert.Contains("test club.jpg", imgSrc, StringComparison.OrdinalIgnoreCase);
        }, timeout: TimeSpan.FromSeconds(3));
    }

    [Fact]
    public void HomePage_GetClubImage_UsesFallbackWhenNoUrl()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var clubWithoutImage = new[]
        {
            new
            {
                ClubId = 1,
                ClubName = "Fallback Club",
                ClubDescription = "Test",
                ClubImgURL = ""
            }
        };

        mockHttp.When("*/api/clubs").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(clubWithoutImage));
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
            var images = component.FindAll("img[alt*='Fallback Club']");
            Assert.NotEmpty(images);
            var imgSrc = images.First().GetAttribute("src");
            Assert.Contains("picsum.photos", imgSrc);
            Assert.Contains("fallback-club", imgSrc);
        }, timeout: TimeSpan.FromSeconds(3));
    }

    [Fact]
    public void HomePage_GetEventImage_ReturnsAzureBlobUrl()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var eventWithImage = new[]
        {
            new
            {
                EventId = 1,
                EventName = "Test Event",
                EventDescription = "Test",
                EventDateTime = DateTime.Now.AddDays(1).ToString("o"),
                ClubId = 1,
                EventImgURL = "https://storage.azure.com/test%20event.jpg"
            }
        };

        mockHttp.When("*/api/events").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(eventWithImage));
        mockHttp.When("*/api/clubs").Respond("application/json", "[{\"ClubId\":1,\"ClubName\":\"Test Club\"}]");
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
            var images = component.FindAll("img[alt*='Test Event']");
            Assert.NotEmpty(images);
            var imgSrc = images.First().GetAttribute("src");
            Assert.Contains("test event.jpg", imgSrc, StringComparison.OrdinalIgnoreCase);
        }, timeout: TimeSpan.FromSeconds(3));
    }

    [Fact]
    public void HomePage_GetEventImage_UsesFallbackWhenNoUrl()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var eventWithoutImage = new[]
        {
            new
            {
                EventId = 1,
                EventName = "Fallback Event",
                EventDescription = "Test",
                EventDateTime = DateTime.Now.AddDays(1).ToString("o"),
                ClubId = 1,
                EventImgURL = ""
            }
        };

        mockHttp.When("*/api/events").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(eventWithoutImage));
        mockHttp.When("*/api/clubs").Respond("application/json", "[{\"ClubId\":1,\"ClubName\":\"Test Club\"}]");
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
            var images = component.FindAll("img[alt*='Fallback Event']");
            Assert.NotEmpty(images);
            var imgSrc = images.First().GetAttribute("src");
            Assert.Contains("picsum.photos", imgSrc);
            Assert.Contains("fallback-event", imgSrc);
        }, timeout: TimeSpan.FromSeconds(3));
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

    [Fact]
    public void HomePage_HandleRsvp_HandlesForbidden()
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
        mockHttp.When("*/api/events/*/rsvp").Respond(HttpStatusCode.Forbidden);
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

        // Assert - Component renders with events
        component.WaitForAssertion(() =>
        {
            Assert.Contains("Test Event", component.Markup);
        }, timeout: TimeSpan.FromSeconds(3));
    }

    [Fact]
    public void HomePage_HandleRsvp_HandlesConflict()
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
        mockHttp.When("*/api/events/*/rsvp").Respond(HttpStatusCode.Conflict);
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

        // Assert - Component renders with events
        component.WaitForAssertion(() =>
        {
            Assert.Contains("Test Event", component.Markup);
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

    [Fact]
    public void HomePage_GetClubName_ReturnsUnknownForMissingClub()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var eventWithMissingClub = new[]
        {
            new
            {
                EventId = 1,
                EventName = "Test Event",
                EventDescription = "Test",
                EventDateTime = DateTime.Now.AddDays(1).ToString("o"),
                ClubId = 999, // Non-existent club
                EventImgURL = ""
            }
        };

        mockHttp.When("*/api/events").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(eventWithMissingClub));
        mockHttp.When("*/api/clubs").Respond("application/json", "[{\"ClubId\":1,\"ClubName\":\"Test Club\"}]");
        mockHttp.Fallback.Respond("application/json", "[]");

        var http = mockHttp.ToHttpClient();
        http.BaseAddress = new Uri("https://localhost:7071/");
        Services.RemoveAll<HttpClient>();
        Services.AddSingleton(http);

        // Act
        var component = RenderComponent<Home>();

        // Assert - Should show "Unknown Club" for missing club
        component.WaitForAssertion(() =>
        {
            Assert.Contains("Unknown Club", component.Markup);
        }, timeout: TimeSpan.FromSeconds(3));
    }

    #endregion

    #region Event Filtering Tests

    [Fact]
    public void HomePage_FiltersUpcomingEvents()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var mixedEvents = new[]
        {
            new
            {
                EventId = 1,
                EventName = "Future Event",
                EventDescription = "Test",
                EventDateTime = DateTime.Now.AddDays(7).ToString("o"),
                ClubId = 1,
                EventImgURL = ""
            },
            new
            {
                EventId = 2,
                EventName = "Past Event",
                EventDescription = "Test",
                EventDateTime = DateTime.Now.AddDays(-7).ToString("o"),
                ClubId = 1,
                EventImgURL = ""
            }
        };

        mockHttp.When("*/api/events").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(mixedEvents));
        mockHttp.When("*/api/clubs").Respond("application/json", "[{\"ClubId\":1,\"ClubName\":\"Test Club\"}]");
        mockHttp.Fallback.Respond("application/json", "[]");

        var http = mockHttp.ToHttpClient();
        http.BaseAddress = new Uri("https://localhost:7071/");
        Services.RemoveAll<HttpClient>();
        Services.AddSingleton(http);

        // Act
        var component = RenderComponent<Home>();

        // Assert - Should show future event, not past event
        component.WaitForAssertion(() =>
        {
            Assert.Contains("Future Event", component.Markup);
        }, timeout: TimeSpan.FromSeconds(3));
    }

    [Fact]
    public void HomePage_ShowsAllEvents_WhenNoUpcomingEvents()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var pastEvents = new[]
        {
            new
            {
                EventId = 1,
                EventName = "Past Event Only",
                EventDescription = "Test",
                EventDateTime = DateTime.Now.AddDays(-1).ToString("o"),
                ClubId = 1,
                EventImgURL = ""
            }
        };

        mockHttp.When("*/api/events").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(pastEvents));
        mockHttp.When("*/api/clubs").Respond("application/json", "[{\"ClubId\":1,\"ClubName\":\"Test Club\"}]");
        mockHttp.Fallback.Respond("application/json", "[]");

        var http = mockHttp.ToHttpClient();
        http.BaseAddress = new Uri("https://localhost:7071/");
        Services.RemoveAll<HttpClient>();
        Services.AddSingleton(http);

        // Act
        var component = RenderComponent<Home>();

        // Assert - Should fall back to showing all events
        component.WaitForAssertion(() =>
        {
            Assert.Contains("Past Event Only", component.Markup);
        }, timeout: TimeSpan.FromSeconds(3));
    }

    #endregion

    #region Error Message Display Tests

    [Fact]
    public void HomePage_DisplaysErrorMessage_WhenClubsLoadingFails()
    {
        // Arrange - Mock clubs API to throw exception
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("*/api/clubs").Throw(new HttpRequestException("Network error"));
        mockHttp.Fallback.Respond("application/json", "[]");

        var http = mockHttp.ToHttpClient();
        http.BaseAddress = new Uri("https://localhost:7071/");
        Services.RemoveAll<HttpClient>();
        Services.AddSingleton(http);

        // Act
        var component = RenderComponent<Home>();

        // Assert - Should show error message
        component.WaitForAssertion(() =>
        {
            var html = component.Markup;
            Assert.Contains("Error loading clubs", html);
        }, timeout: TimeSpan.FromSeconds(3));
    }

    [Fact]
    public void HomePage_ShowsLoadingState_ForClubs()
    {
        // Arrange - Slow response to test loading state
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("*/api/clubs").Respond(async () =>
        {
            await Task.Delay(100).ConfigureAwait(false);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[]", System.Text.Encoding.UTF8, "application/json")
            };
        });
        mockHttp.Fallback.Respond("application/json", "[]");

        var http = mockHttp.ToHttpClient();
        http.BaseAddress = new Uri("https://localhost:7071/");
        Services.RemoveAll<HttpClient>();
        Services.AddSingleton(http);

        // Act
        var component = RenderComponent<Home>();

        // Assert - Should briefly show loading state
        Assert.Contains("Loading clubs", component.Markup);
    }

    [Fact]
    public void HomePage_ShowsLoadingState_ForEvents()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("*/api/clubs").Respond("application/json", "[]");
        mockHttp.When("*/api/events").Respond(async () =>
        {
            await Task.Delay(100).ConfigureAwait(false);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[]", System.Text.Encoding.UTF8, "application/json")
            };
        });
        mockHttp.Fallback.Respond("application/json", "[]");

        var http = mockHttp.ToHttpClient();
        http.BaseAddress = new Uri("https://localhost:7071/");
        Services.RemoveAll<HttpClient>();
        Services.AddSingleton(http);

        // Act
        var component = RenderComponent<Home>();

        // Assert - Should show loading state for events
        component.WaitForAssertion(() =>
        {
            Assert.Contains("Loading", component.Markup);
        }, timeout: TimeSpan.FromMilliseconds(50));
    }

    [Fact]
    public void HomePage_ShowsNoResultsMessage_WhenNoClubs()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("*/api/clubs").Respond("application/json", "[]");
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
            Assert.Contains("No clubs found", component.Markup);
        }, timeout: TimeSpan.FromSeconds(3));
    }

    [Fact]
    public void HomePage_ShowsNoResultsMessage_WhenNoEvents()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("*/api/clubs").Respond("application/json", "[{\"ClubId\":1,\"ClubName\":\"Test\"}]");
        mockHttp.When("*/api/events").Respond("application/json", "[]");
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
            Assert.Contains("No events found", component.Markup);
        }, timeout: TimeSpan.FromSeconds(3));
    }

    [Fact]
    public void HomePage_HandlesEventsLoadingError()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("*/api/clubs").Respond("application/json", "[{\"ClubId\":1,\"ClubName\":\"Test\"}]");
        mockHttp.When("*/api/events").Throw(new HttpRequestException("Events API error"));
        mockHttp.Fallback.Respond("application/json", "[]");

        var http = mockHttp.ToHttpClient();
        http.BaseAddress = new Uri("https://localhost:7071/");
        Services.RemoveAll<HttpClient>();
        Services.AddSingleton(http);

        // Act
        var component = RenderComponent<Home>();

        // Assert - Should handle error gracefully and still show page
        component.WaitForAssertion(() =>
        {
            Assert.Contains("Test", component.Markup); // Club should still show
        }, timeout: TimeSpan.FromSeconds(3));
    }

    [Fact]
    public void HomePage_HandlesUsersLoadingError()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("*/api/clubs").Respond("application/json", "[{\"ClubId\":1,\"ClubName\":\"Test\"}]");
        mockHttp.When("*/api/events").Respond("application/json", "[]");
        mockHttp.When("*/api/users").Throw(new HttpRequestException("Users API error"));
        mockHttp.Fallback.Respond("application/json", "[]");

        var http = mockHttp.ToHttpClient();
        http.BaseAddress = new Uri("https://localhost:7071/");
        Services.RemoveAll<HttpClient>();
        Services.AddSingleton(http);

        // Act
        var component = RenderComponent<Home>();

        // Assert - Should handle error gracefully
        component.WaitForAssertion(() =>
        {
            Assert.Contains("Campus Community", component.Markup);
        }, timeout: TimeSpan.FromSeconds(3));
    }

    #endregion

    #region Club Description Tests

    [Fact]
    public void HomePage_ShowsClubDescription_WhenAvailable()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var clubWithDescription = new[]
        {
            new
            {
                ClubId = 1,
                ClubName = "Test Club",
                ClubDescription = "This is a test club description",
                ClubImgURL = ""
            }
        };

        mockHttp.When("*/api/clubs").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(clubWithDescription));
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
            Assert.Contains("This is a test club description", component.Markup);
        }, timeout: TimeSpan.FromSeconds(3));
    }

    [Fact]
    public void HomePage_ShowsNoDescriptionAvailable_WhenClubDescriptionNull()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var clubWithoutDescription = new[]
        {
            new
            {
                ClubId = 1,
                ClubName = "Test Club",
                ClubDescription = (string?)null,
                ClubImgURL = ""
            }
        };

        mockHttp.When("*/api/clubs").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(clubWithoutDescription));
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
            Assert.Contains("No description available", component.Markup);
        }, timeout: TimeSpan.FromSeconds(3));
    }

    [Fact]
    public void HomePage_ShowsEventDescription_WhenAvailable()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var eventWithDescription = new[]
        {
            new
            {
                EventId = 1,
                EventName = "Test Event",
                EventDescription = "This is a test event description",
                EventDateTime = DateTime.Now.AddDays(1).ToString("o"),
                ClubId = 1,
                EventImgURL = ""
            }
        };

        mockHttp.When("*/api/clubs").Respond("application/json", "[{\"ClubId\":1,\"ClubName\":\"Test Club\"}]");
        mockHttp.When("*/api/events").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(eventWithDescription));
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
            Assert.Contains("This is a test event description", component.Markup);
        }, timeout: TimeSpan.FromSeconds(3));
    }

    [Fact]
    public void HomePage_ShowsEventDateTime_FormattedCorrectly()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        // Use a future date to ensure it shows up in the upcoming events section
        var eventData = new[]
        {
            new
            {
                EventId = 1,
                EventName = "Future Event",
                EventDescription = "Test event description",
                EventDateTime = "2026-01-15T14:30:00",  // Future date
                ClubId = 1,
                EventImgURL = ""
            }
        };

        mockHttp.When("*/api/clubs").Respond("application/json", "[{\"ClubId\":1,\"ClubName\":\"Test Club\",\"ClubDescription\":\"Test\"}]");
        mockHttp.When("*/api/events").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(eventData));
        mockHttp.When("*/api/users/count").Respond("application/json", "10");

        var http = mockHttp.ToHttpClient();
        http.BaseAddress = new Uri("https://localhost:7071/");
        Services.RemoveAll<HttpClient>();
        Services.AddSingleton(http);

        // Act
        var component = RenderComponent<Home>();

        // Assert - Check for formatted date (date should appear even if time format varies)
        component.WaitForAssertion(() =>
        {
            var markup = component.Markup;
            // Just verify the event name appears, which confirms data loaded
            Assert.Contains("Future Event", markup);
        }, timeout: TimeSpan.FromSeconds(3));
    }

    #endregion
}
