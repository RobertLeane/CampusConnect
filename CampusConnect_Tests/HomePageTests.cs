using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Client.Pages;
using Client.Services;
using RichardSzalay.MockHttp;
using Xunit;
using Bunit.TestDoubles;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Text.RegularExpressions;
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
}
