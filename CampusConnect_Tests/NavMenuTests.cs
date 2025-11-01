using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Client.Layout;
using Client.Services;
using RichardSzalay.MockHttp;
using Xunit;

namespace CampusConnect_Tests;

public class NavMenuTests : TestContext
{
    public NavMenuTests()
    {
        // Setup mock HTTP client
        var mockHttp = new MockHttpMessageHandler();
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost:7071/");
        
        Services.AddSingleton(httpClient);
        Services.AddSingleton<AuthService>();
        
        // Setup mock JSRuntime
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    #region Rendering Tests

    [Fact]
    public void NavMenu_RendersCorrectly()
    {
        // Act
        var component = RenderComponent<NavMenu>();

        // Assert - Check that navigation links are present
        Assert.Contains("Home", component.Markup);
        Assert.Contains("Clubs", component.Markup);
        Assert.Contains("Events", component.Markup);
    }

    [Fact]
    public void NavMenu_ShowsLoginLink_WhenNotAuthenticated()
    {
        // Act
        var component = RenderComponent<NavMenu>();

        // Assert - Default state should show login
        Assert.Contains("Login", component.Markup);
    }

    #endregion

    #region Navigation Link Tests

    [Fact]
    public void NavMenu_HomeLink_HasCorrectHref()
    {
        // Act
        var component = RenderComponent<NavMenu>();

        // Assert
        var markup = component.Markup;
        Assert.Contains("href=\"/\"", markup);
    }

    [Fact]
    public void NavMenu_ClubsLink_HasCorrectHref()
    {
        // Act
        var component = RenderComponent<NavMenu>();

        // Assert
        Assert.Contains("href=\"/clubs\"", component.Markup);
    }

    [Fact]
    public void NavMenu_EventsLink_HasCorrectHref()
    {
        // Act
        var component = RenderComponent<NavMenu>();

        // Assert
        Assert.Contains("href=\"/events\"", component.Markup);
    }

    [Fact]
    public void NavMenu_LoginLink_HasCorrectHref_WhenNotAuthenticated()
    {
        // Act
        var component = RenderComponent<NavMenu>();

        // Assert
        Assert.Contains("href=\"/login\"", component.Markup);
    }

    #endregion

    #region Accessibility Tests

    [Fact]
    public void NavMenu_LoginLink_HasAriaLabel()
    {
        // Act
        var component = RenderComponent<NavMenu>();

        // Assert
        Assert.Contains("aria-label=\"Login\"", component.Markup);
    }

    [Fact]
    public void NavMenu_HasSvgIcons()
    {
        // Act
        var component = RenderComponent<NavMenu>();

        // Assert - Check for SVG icons
        Assert.Contains("<svg", component.Markup);
        Assert.Contains("viewBox=\"0 0 24 24\"", component.Markup);
        Assert.Contains("aria-hidden=\"true\"", component.Markup);
    }

    #endregion

    #region CSS Class Tests

    [Fact]
    public void NavMenu_HasTabsClass()
    {
        // Act
        var component = RenderComponent<NavMenu>();

        // Assert
        Assert.Contains("class=\"tabs\"", component.Markup);
    }

    [Fact]
    public void NavMenu_HasAuthSectionClass()
    {
        // Act
        var component = RenderComponent<NavMenu>();

        // Assert
        Assert.Contains("class=\"auth-section\"", component.Markup);
    }

    [Fact]
    public void NavMenu_LoginLink_HasCorrectClass()
    {
        // Act
        var component = RenderComponent<NavMenu>();

        // Assert
        Assert.Contains("class=\"login-link\"", component.Markup);
    }

    #endregion
}
