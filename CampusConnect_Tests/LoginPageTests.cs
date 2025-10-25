using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Client.Pages;
using Client.Services;
using RichardSzalay.MockHttp;
using Xunit;

namespace CampusConnect_Tests;

public class LoginPageTests : TestContext
{
    public LoginPageTests()
    {
        // Register required services
        Services.AddSingleton<AuthService>();
        
        // Add mock JSRuntime (bUnit provides this automatically)
        JSInterop.Mode = JSRuntimeMode.Loose;
        
        // Setup basic HTTP client
        var mockHttp = new MockHttpMessageHandler();
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost:7071/");
        Services.AddSingleton(httpClient);
    }

    #region Rendering Tests

    [Fact]
    public void LoginPage_RendersCorrectly()
    {
        // Act
        var component = RenderComponent<Login>();

        // Assert
        Assert.Contains("Welcome Back", component.Markup);
        Assert.Contains("Log in to access your clubs, events, and dashboard.", component.Markup);
        
        // Check form elements are present
        var emailInput = component.Find("input[type='email']");
        var passwordInput = component.Find("input[type='password']");
        var submitButton = component.Find("button[type='submit']");
        
        Assert.NotNull(emailInput);
        Assert.NotNull(passwordInput);
        Assert.NotNull(submitButton);
        Assert.Contains("Login", submitButton.TextContent);
    }

    [Fact]
    public void LoginPage_HasCorrectPlaceholders()
    {
        // Act
        var component = RenderComponent<Login>();

        // Assert
        var emailInput = component.Find("input[type='email']");
        var passwordInput = component.Find("input[type='password']");
        
        Assert.Equal("you@example.com", emailInput.GetAttribute("placeholder"));
        Assert.Equal("Enter your password", passwordInput.GetAttribute("placeholder"));
    }

    [Fact]
    public void LoginPage_HasNavigationLinks()
    {
        // Act
        var component = RenderComponent<Login>();

        // Assert
        var forgotPasswordLink = component.Find("a[href='/forgot-password']");
        var registerLink = component.Find("a[href='/register']");

        Assert.NotNull(forgotPasswordLink);
        Assert.NotNull(registerLink);
        Assert.Equal("Forgot password?", forgotPasswordLink.TextContent);
        Assert.Equal("Create an account", registerLink.TextContent);
    }
    
    #endregion

    #region Validation Tests

    [Fact]
    public void LoginPage_ShowsValidationErrors_WhenFieldsEmpty()
    {
        // Act
        var component = RenderComponent<Login>();
        var form = component.Find("form");
        
        // Submit form without filling fields
        form.Submit();

        // Assert - validation messages should appear after submission
        component.WaitForAssertion(() =>
        {
            var validationElements = component.FindAll(".validation-message, [class*='validation']");
            Assert.NotEmpty(validationElements);
        });
    }

    [Fact]
    public void LoginPage_EnablesSubmitButton_WhenNotSubmitting()
    {
        // Act
        var component = RenderComponent<Login>();
        var submitButton = component.Find("button[type='submit']");

        // Assert
        Assert.Null(submitButton.GetAttribute("disabled"));
        Assert.Contains("Login", submitButton.TextContent);
    }

    [Fact]
    public void LoginPage_CanEnterEmailAndPassword()
    {
        // Act
        var component = RenderComponent<Login>();
        var emailInput = component.Find("input[type='email']");
        var passwordInput = component.Find("input[type='password']");

        // Enter values
        emailInput.Change("test@example.com");
        passwordInput.Change("password123");

        // Assert
        Assert.Equal("test@example.com", emailInput.GetAttribute("value"));
        Assert.Equal("password123", passwordInput.GetAttribute("value"));
    }

    #endregion

    [Fact]
    public void LoginPage_SuccessfulLogin_NavigatesToHome()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var successResponse = new
        {
            UserId = 1,
            Email = "test@example.com",
            Message = "Login successful"
        };
        
        mockHttp.When("/api/auth/login")
               .Respond("application/json", JsonSerializer.Serialize(successResponse));

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost:7071/");
        
        // Replace the HTTP client
        Services.AddSingleton(httpClient);
        
        var navManager = Services.GetRequiredService<NavigationManager>();

        // Act
        var component = RenderComponent<Login>();
        var emailInput = component.Find("input[type='email']");
        var passwordInput = component.Find("input[type='password']");
        var form = component.Find("form");

        emailInput.Change("test@example.com");
        passwordInput.Change("password123");
        
        form.Submit();

        // Assert - Wait for navigation to occur
        component.WaitForAssertion(() =>
        {
            Assert.Equal("http://localhost/", navManager.Uri);
        });
    }

    [Fact]
    public void LoginPage_FailedLogin_ShowsErrorMessage()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("/api/auth/login")
               .Respond(HttpStatusCode.Unauthorized);

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost:7071/");
        
        Services.AddSingleton(httpClient);

        // Act
        var component = RenderComponent<Login>();
        var emailInput = component.Find("input[type='email']");
        var passwordInput = component.Find("input[type='password']");
        var form = component.Find("form");

        emailInput.Change("test@example.com");
        passwordInput.Change("wrongpassword");
        
        form.Submit();

        // Assert - Wait for error message to appear
        component.WaitForAssertion(() =>
        {
            var errorText = component.Find("p[style*='color: red']");
            Assert.Contains("Invalid email or password", errorText.TextContent);
        });
    }

    [Fact]
    public void LoginPage_NetworkError_ShowsErrorMessage()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("/api/auth/login")
               .Throw(new HttpRequestException("Network error"));

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost:7071/");
        
        Services.AddSingleton(httpClient);

        // Act
        var component = RenderComponent<Login>();
        var emailInput = component.Find("input[type='email']");
        var passwordInput = component.Find("input[type='password']");
        var form = component.Find("form");

        emailInput.Change("test@example.com");
        passwordInput.Change("password123");
        
        form.Submit();

        // Assert - Wait for error message to appear
        component.WaitForAssertion(() =>
        {
            var errorText = component.Find("p[style*='color: red']");
            Assert.Contains("Invalid email or password", errorText.TextContent);
        });
    }

    [Fact]
    public void LoginPage_ShowsSubmittingState_WhenFormSubmitted()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        // Create a delayed response to test submitting state
        mockHttp.When("/api/auth/login")
               .Respond(async () =>
               {
                   await Task.Delay(500); // Short delay for testing
                   return new HttpResponseMessage(HttpStatusCode.OK)
                   {
                       Content = new StringContent(JsonSerializer.Serialize(new { UserId = 1, Email = "test@example.com" }))
                   };
               });

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost:7071/");
        
        Services.AddSingleton(httpClient);

        // Act
        var component = RenderComponent<Login>();
        var emailInput = component.Find("input[type='email']");
        var passwordInput = component.Find("input[type='password']");
        var form = component.Find("form");

        emailInput.Change("test@example.com");
        passwordInput.Change("password123");
        
        form.Submit();

        // Assert - Check submitting state appears briefly
        component.WaitForAssertion(() =>
        {
            var submitButton = component.Find("button[type='submit']");
            Assert.NotNull(submitButton.GetAttribute("disabled"));
            Assert.Contains("Signing in...", submitButton.TextContent);
        });
    }
}