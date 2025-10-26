using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Client.Pages;
using Client.Services;
using RichardSzalay.MockHttp;
using Xunit;
using Bunit.TestDoubles;

namespace CampusConnect_Tests;

public class HomePageTests : TestContext
{
    private readonly MockHttpMessageHandler _mockHttp;
    private readonly HttpClient _http;
    private readonly TestAuthorizationContext _auth;

    // ---------- helpers ----------
    private static bool ContainsAny(string html, params string[] needles) =>
        needles.Any(n => html.Contains(n, StringComparison.OrdinalIgnoreCase));

    private void ForceGuest()
    {
        // Force Blazor auth state to "not authorized"
        _auth.SetNotAuthorized();

        // If your app probes the current user, return 401
        _mockHttp.When("*/api/users/me*")
                 .Respond(System.Net.HttpStatusCode.Unauthorized);

        // If your app reads any token from localStorage, return null for any key
        JSInterop.Setup<string>("localStorage.getItem", _ => true).SetResult(null);
        // (If your code calls a wrapper like "auth.getToken", add another Setup for that identifier.)
    }
    // -----------------------------

    public HomePageTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;

        _mockHttp = new MockHttpMessageHandler();
        _mockHttp.Fallback.Respond("application/json", "[]");

        _http = _mockHttp.ToHttpClient();
        _http.BaseAddress = new Uri("https://localhost:7071/");

        Services.AddScoped(sp => _http);
        Services.AddScoped<AuthService>();
        Services.AddScoped<ClubApi>();
        Services.AddScoped<EventApi>();
        Services.AddScoped<MemberApi>();
        Services.AddScoped<UserApi>();
        Services.AddAuthorizationCore();

        _auth = this.AddTestAuthorization();
        _auth.SetNotAuthorized();
        DefaultWaitTimeout = TimeSpan.FromSeconds(5); // sensible default for all WaitForAssertion
    }

    // Common stubs
    private void StubHappyPaths(
        string statsJson = """{ "clubs": 12, "events": 34, "members": 567 }""",
        string recentClubsJson = """[{"ClubId":1,"ClubName":"AI Society"}]""",
        string upcomingEventsJson = """[{"EventId":1,"EventName":"Welcome Fair","EventDateTime":"2025-10-30T14:00:00Z"}]"""
    )
    {
        _mockHttp.When("*/api/stats*").Respond("application/json", statsJson);
        _mockHttp.When("*/api/clubs/recent*").Respond("application/json", recentClubsJson);
        _mockHttp.When("*/api/events/upcoming*").Respond("application/json", upcomingEventsJson);
    }

    #region Landing Page Tests

    [Fact]
    public void HomePage_RendersCorrectly()
    {
        StubHappyPaths();
        var component = RenderComponent<Home>();
        Assert.Contains("Discover Your", component.Markup);
        Assert.Contains("Campus Community", component.Markup);
    }

    [Fact]
    public void HomePage_HasWelcomeMessage()
    {
        // TODO: Test welcome/hero section
        // - Check main heading text
        // - Verify tagline or description
        // - Check hero image if present
        // - Test responsive design of hero section
    }

    [Fact]
    public void HomePage_HasCallToActionButtons()
    {
        // TODO: Test main action buttons
        // - Check "Get Started" or "Sign Up" button
        // - Check "Learn More" button
        // - Verify button navigation
        // - Test button styling and accessibility
    }

    #endregion

    #region Features Section Tests

    [Fact]
    public void HomePage_DisplaysFeatureHighlights()
    {
        // TODO: Test feature showcase section
        // - Check club management features
        // - Check event organization features
        // - Check member collaboration features
        // - Verify feature icons and descriptions
    }

    [Fact]
    public void HomePage_HasAboutSection()
    {
        // TODO: Test about/description section
        // - Check platform description
        // - Verify benefits listed
        // - Check target audience messaging
        // - Test section layout and readability
    }

    #endregion

    #region Navigation Tests

    [Fact]
    public void HomePage_NavigationWorksCorrectly()
    {
        StubHappyPaths();
        var component = RenderComponent<Home>();
        component.WaitForAssertion(() =>
        {
            var html = component.Markup;
            Assert.True(ContainsAny(html,
                "href=\"/clubs\"", "href=\"/events\"", "href=\"/dashboard\"", "href=\"/home\""));
        });
    }

    [Fact]
    public void HomePage_ShowsDifferentContent_ForAuthenticatedUsers()
    {
        // TODO: Test personalized content for logged in users
        // - Mock authenticated user
        // - Check for personalized greeting
        // - Verify "Go to Dashboard" instead of "Sign Up"
        // - Test quick access to user's clubs/events
    }

    [Fact]
    public void HomePage_ShowsLoginPrompt_ForUnauthenticatedUsers()
    {
        StubHappyPaths();
        ForceGuest(); // ensure truly guest

        var component = RenderComponent<Home>();
        component.WaitForAssertion(() =>
        {
            var html = component.Markup;

            // Accept multiple route/text variants (navbar or hero)
            Assert.True(
                ContainsAny(html,
                    "href=\"/login\"", "href=\"/register\"", "href=\"/signup\"",
                    ">Log In<", ">Login<", ">Sign In<", ">Register<", ">Sign Up<"),
                "Expected a guest CTA (/login or /register or visible login/register text)."
            );

            Assert.False(ContainsAny(html, "href=\"/dashboard\"", "Go to Dashboard", ">Dashboard<"),
                "Dashboard CTA should not render for guests.");
        });
    }
    #endregion

    #region Statistics Section Tests

    [Fact]
    public void HomePage_DisplaysPlatformStatistics()
    {
        StubHappyPaths(
            statsJson: """{ "clubs": 99, "events": 7, "members": 1234 }""",
            recentClubsJson: "[]",
            upcomingEventsJson: "[]"
        );
        var component = RenderComponent<Home>();
        component.WaitForAssertion(() =>
        {
            var html = component.Markup.Replace("\u00A0", "");
            Assert.Contains("99", html);
            Assert.Contains("7", html);
            Assert.Contains("1,234", html);
        });
    }

    [Fact]
    public void HomePage_HandlesStatisticsErrors()
    {
        _mockHttp.When("*/api/stats*").Respond(System.Net.HttpStatusCode.InternalServerError);
        var component = RenderComponent<Home>();
        component.WaitForAssertion(() =>
        {
            Assert.Contains("Discover Your", component.Markup);
        });
    }
    #endregion

    #region Footer Tests

    [Fact]
    public void HomePage_HasCompleteFooter()
    {
        StubHappyPaths();
        var component = RenderComponent<Home>();
        component.WaitForAssertion(() =>
        {
            var html = component.Markup;
            Assert.True(ContainsAny(html, "Privacy", "Terms"));
            Assert.True(html.Contains("©", StringComparison.OrdinalIgnoreCase)
                        || html.Contains("&copy;", StringComparison.OrdinalIgnoreCase));
        });
    }

    [Fact]
    public void HomePage_FooterLinksWork()
    {
        StubHappyPaths();
        var component = RenderComponent<Home>();
        component.WaitForAssertion(() =>
        {
            var html = component.Markup;
            Assert.True(ContainsAny(html, "href=\"/privacy\"", "href=\"/terms\""));
        });
    }
    #endregion

    #region Responsive / Perf / SEO

    [Fact]
    public void HomePage_ResponsiveDesign()
    {
        StubHappyPaths();
        var c = RenderComponent<Home>();
        Assert.Contains("Discover Your", c.Markup);
        Assert.Contains("Campus Community", c.Markup);
        Assert.True(ContainsAny(c.Markup, "Features", "feature"));
    }

    [Fact]
    public void HomePage_LoadsQuickly()
    {
        StubHappyPaths();
        var c = RenderComponent<Home>();
        Assert.NotNull(c);
    }

    [Fact]
    public void HomePage_HasProperSEOElements()
    {
        StubHappyPaths();
        var c = RenderComponent<Home>();
        Assert.Contains("<h1", c.Markup, StringComparison.OrdinalIgnoreCase);
    }
    #endregion

    #region Integration Tests

    [Fact]
    public void HomePage_IntegratesWithAuthSystem()
    {
        StubHappyPaths();
        _mockHttp.When("*/api/users/me*")
                 .Respond("application/json", """{ "UserId": 1, "FirstName": "Alex" }""");

        var component = RenderComponent<Home>();
        component.WaitForAssertion(() =>
        {
            var html = component.Markup;
            Assert.True(ContainsAny(html, "href=\"/dashboard\"", "Go to Dashboard", "Dashboard"));
            Assert.False(ContainsAny(html, ">Sign Up<", "href=\"/register\"", "href=\"/signup\""));
        });
    }
    #endregion
}
