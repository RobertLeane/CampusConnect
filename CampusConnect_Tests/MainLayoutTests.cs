using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Client.Layout;
using Client.Services;
using RichardSzalay.MockHttp;
using Xunit;

namespace CampusConnect_Tests;

public class MainLayoutTests : TestContext
{
    public MainLayoutTests()
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
    public void MainLayout_RendersCorrectly()
    {
        // Act
        var component = RenderComponent<MainLayout>();

        // Assert - Check basic structure
        Assert.Contains("CampusConnect", component.Markup);
    }

    [Fact]
    public void MainLayout_HasHeader()
    {
        // Act
        var component = RenderComponent<MainLayout>();

        // Assert
        Assert.Contains("<header", component.Markup);
        Assert.Contains("class=\"nav\"", component.Markup);
    }

    [Fact]
    public void MainLayout_HasMainSection()
    {
        // Act
        var component = RenderComponent<MainLayout>();

        // Assert
        Assert.Contains("<main", component.Markup);
    }

    [Fact]
    public void MainLayout_HasFooter()
    {
        // Act
        var component = RenderComponent<MainLayout>();

        // Assert
        Assert.Contains("<footer", component.Markup);
        Assert.Contains("class=\"footer\"", component.Markup);
    }

    #endregion

    #region Header Tests

    [Fact]
    public void MainLayout_Header_ShowsBrandName()
    {
        // Act
        var component = RenderComponent<MainLayout>();

        // Assert
        Assert.Contains("class=\"brand\"", component.Markup);
        Assert.Contains("CampusConnect", component.Markup);
    }

    [Fact]
    public void MainLayout_Header_ContainsNavMenu()
    {
        // Act
        var component = RenderComponent<MainLayout>();

        // Assert - NavMenu should render its navigation items
        var markup = component.Markup;
        Assert.Contains("Home", markup);
        Assert.Contains("Clubs", markup);
        Assert.Contains("Events", markup);
    }

    [Fact]
    public void MainLayout_Header_HasContainerClass()
    {
        // Act
        var component = RenderComponent<MainLayout>();

        // Assert
        Assert.Contains("class=\"container nav__inner\"", component.Markup);
    }

    #endregion

    #region Footer Tests

    [Fact]
    public void MainLayout_Footer_ShowsTagline()
    {
        // Act
        var component = RenderComponent<MainLayout>();

        // Assert
        Assert.Contains("Connecting students through clubs and events", component.Markup);
    }

    [Fact]
    public void MainLayout_Footer_HasBrowseClubsLink()
    {
        // Act
        var component = RenderComponent<MainLayout>();

        // Assert
        Assert.Contains("href=\"/clubs\"", component.Markup);
        Assert.Contains("Browse Clubs", component.Markup);
    }

    [Fact]
    public void MainLayout_Footer_HasFindEventsLink()
    {
        // Act
        var component = RenderComponent<MainLayout>();

        // Assert
        Assert.Contains("href=\"/events\"", component.Markup);
        Assert.Contains("Find Events", component.Markup);
    }

    [Fact]
    public void MainLayout_Footer_HasDashboardLink()
    {
        // Act
        var component = RenderComponent<MainLayout>();

        // Assert
        Assert.Contains("href=\"/dashboard\"", component.Markup);
        Assert.Contains("My Dashboard", component.Markup);
    }

    [Fact]
    public void MainLayout_Footer_HasAboutUsLink()
    {
        // Act
        var component = RenderComponent<MainLayout>();

        // Assert
        Assert.Contains("href=\"/about\"", component.Markup);
        Assert.Contains("About Us", component.Markup);
    }

    #endregion

    #region CSS Structure Tests

    [Fact]
    public void MainLayout_HasCorrectCssClasses()
    {
        // Act
        var component = RenderComponent<MainLayout>();

        // Assert - Check for key CSS classes
        var markup = component.Markup;
        Assert.Contains("class=\"nav\"", markup);
        Assert.Contains("class=\"brand\"", markup);
        Assert.Contains("class=\"footer\"", markup);
        Assert.Contains("class=\"container", markup);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void MainLayout_IntegratesWithNavMenu()
    {
        // Act
        var component = RenderComponent<MainLayout>();

        // Assert - NavMenu component should be rendered with its navigation items
        var markup = component.Markup;
        Assert.Contains("Home", markup);
        Assert.Contains("Clubs", markup);
        Assert.Contains("Events", markup);
        Assert.Contains("class=\"tabs\"", markup); // NavMenu's CSS class
    }

    [Fact]
    public void MainLayout_HasCompleteStructure()
    {
        // Act
        var component = RenderComponent<MainLayout>();

        // Assert - Verify complete structure exists
        var markup = component.Markup;
        
        // Header with brand and navigation
        Assert.Contains("<header", markup);
        Assert.Contains("CampusConnect", markup);
        
        // Main content area
        Assert.Contains("<main", markup);
        
        // Footer with links
        Assert.Contains("<footer", markup);
        Assert.Contains("Browse Clubs", markup);
    }

    #endregion
}
