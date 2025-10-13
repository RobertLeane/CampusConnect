# CampusConnect Tests

This project contains unit tests for the CampusConnect Blazor WebAssembly application using bUnit.

## Running Tests

From the repository root or the test directory:

```powershell
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run only Login page tests
dotnet test --filter "LoginPageTests"

# Run tests with coverage (if coverage tools are installed)
dotnet test --collect:"XPlat Code Coverage"
```

## Test Structure

### LoginPageTests.cs ✅ COMPLETED
Tests for the Login page (`client/Pages/Login.razor`):

- **Rendering Tests**: Verifies the page renders correctly with expected elements
- **Interaction Tests**: Tests form input, validation, and user interactions
- **Integration Tests**: Tests API calls, navigation, and error handling
- **State Tests**: Verifies submitting states and async operations

#### Test Coverage:
- ✅ Page renders with correct content and elements
- ✅ Form inputs work correctly (email, password)
- ✅ Navigation links are present and correct
- ✅ Form validation shows errors for empty fields
- ✅ Submit button states (enabled/disabled, text changes)
- ✅ Successful login navigation to home page
- ✅ Failed login shows error message
- ✅ Network errors show appropriate error messages
- ✅ Submitting state shows loading indicator

### RegisterPageTests.cs 📋 SCAFFOLD
Test scaffolds for the Register page (`client/Pages/Register.razor`):
- Rendering tests (form fields, navigation links)
- Validation tests (email format, password requirements, confirmation matching)
- Interaction tests (form input handling, submit button states)
- API integration tests (successful/failed registration, error handling)
- Security tests (sensitive data handling)

### DashboardPageTests.cs 📋 SCAFFOLD
Test scaffolds for the Dashboard page (`client/Pages/Dashboard.razor`):
- Authentication tests (requires login, displays user info)
- Layout and navigation tests
- Data display tests (user clubs, upcoming events, recent activity)
- Loading and error state tests
- Quick actions and statistics tests
- Personalization tests

### ClubsPageTests.cs 📋 SCAFFOLD
Test scaffolds for the Clubs page (`client/Pages/Clubs.razor`):
- Page rendering and club list display tests
- Search and filter functionality tests
- Club actions tests (join/leave clubs, view details)
- Permission and authentication tests
- Loading states and error handling
- Club creation tests (if applicable)

### EventsPageTests.cs 📋 SCAFFOLD
Test scaffolds for the Events page (`client/Pages/Events.razor`):
- Event list display and filtering tests
- Date and category filtering tests
- RSVP/registration functionality tests
- Calendar view tests (if implemented)
- Event creation and management tests
- Event status tests (past/upcoming/cancelled)

### HomePageTests.cs 📋 SCAFFOLD
Test scaffolds for the Home page (`client/Pages/Home.razor`):
- Landing page and welcome message tests
- Features section and call-to-action tests
- Navigation tests for authenticated/unauthenticated users
- Statistics and recent activity tests
- Responsive design and accessibility tests
- SEO and performance tests

## Dependencies

- **bUnit 1.40.0**: Blazor component testing framework
- **bunit.web 1.40.0**: Web-specific bUnit extensions
- **RichardSzalay.MockHttp 7.0.0**: HTTP mocking for testing API calls
- **xUnit**: Test framework
- **Microsoft.NET.Test.Sdk**: Test SDK

## Adding New Tests

1. Create a new test class in the `CampusConnect_Tests` project
2. Inherit from `TestContext` for component tests
3. Register required services in the constructor
4. Use bUnit's `RenderComponent<T>()` to render Blazor components
5. Assert against the rendered component using bUnit's assertion helpers

## Example Test:

```csharp
[Fact]
public void MyComponent_RendersCorrectly()
{
    // Act
    var component = RenderComponent<MyComponent>();

    // Assert
    Assert.Contains("Expected Text", component.Markup);
    var button = component.Find("button");
    Assert.NotNull(button);
}
```

## Running in CI/CD

These tests will run automatically in GitHub Actions or any CI/CD pipeline that executes `dotnet test`.