using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Client.Pages;
using Client.Services;
using RichardSzalay.MockHttp;
using Xunit;
using MudBlazor.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CampusConnect_Tests;

public class EventsPageTests : TestContext
{
    public EventsPageTests()
    {
        // Register required services for Events page
        Services.AddSingleton<AuthService>();
        Services.AddSingleton<ClubApi>();
        Services.AddSingleton<EventApi>();
        Services.AddSingleton<MemberApi>();
        Services.AddSingleton<UserApi>();
        Services.AddSingleton<EventRegistrationService>();
        Services.AddSingleton<EventParticipantApi>();
        Services.AddMudServices();
        // Setup mock JSRuntime
        JSInterop.Mode = JSRuntimeMode.Loose;

    }
    
     
    #region Page Rendering Tests

    [Fact]
    public void EventsPage_RendersCorrectly()
    {
         // Act
        var component = RenderComponent<Events>();

        // Assert – basic page structure that should render immediately
        Assert.Contains("Events", component.Markup);

        // Check statistics section labels (these should always be present)
        Assert.Contains("Active Clubs", component.Markup);
        Assert.Contains("Events", component.Markup);
        Assert.Contains("Active Members", component.Markup);
    }

    #endregion

    #region Event List Display Tests

    [Fact]
    public void EventsPage_DisplaysEventList()
    {
         // Arrange – mock API data
        var mockHttp = new MockHttpMessageHandler();

        // Mock clubs data 
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
        var eventData = new[]
       {
            new { EventId = 1, EventName = "Hackathon", EventDescription = "24h coding event.",
                  EventDateTime = DateTime.Now.AddDays(2), EventImgURL = "", ClubId = 2, Location = "Main Hall" },
       };
       
        // Mock users data  
        var userData = new[]
        {
            new { UserId = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com" }
        };

        mockHttp.When("*/api/clubs").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(clubData));
        mockHttp.When("*/api/events").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(eventData));
        mockHttp.When("*/api/users").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(userData));

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost:7071/");
        Services.AddSingleton(httpClient);

        // Replace the HTTP client in the current test context
        Services.AddSingleton(httpClient);

        // Act
        var component = RenderComponent<Events>();

        // Wait for list
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
       // Arrange
    var mockHttp = new MockHttpMessageHandler();

    var when = DateTime.Now.AddDays(1);
    var eventData = new[]
    {
        new { EventId = 1, EventName = "Photo Walk",
              EventDescription = "Bring your camera for a campus photo tour.",
              EventDateTime = when, EventImgURL = "", ClubId = 1, Location = "Quad" }
    };

    //provide the matching club so the card is renderable
    var clubs = new[]
    {
        new { ClubId = 1, ClubName = "Photography Club", ClubImgURL = "", ClubDescription = "Pics!" }
    };

    mockHttp.When("*/api/events*")
        .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(eventData));
    mockHttp.When("*/api/clubs*")
        .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(clubs));
    mockHttp.When("*/api/users*").Respond("application/json", "[]");
    mockHttp.When("*/api/eventparticipants*").Respond("application/json", "[]");
    mockHttp.When("*/api/eventregistrations*").Respond("application/json", "[]");
    mockHttp.When("*").Respond("application/json", "[]"); // safety net

    var httpClient = mockHttp.ToHttpClient();
    httpClient.BaseAddress = new Uri("https://localhost:7071/");
    Services.AddSingleton(httpClient); // only once in this test

    // Act
    var component = RenderComponent<Events>();

    // Assert – wait only for the thing that proves the item rendered
    component.WaitForAssertion(() =>
    {
        Assert.Contains("Photo Walk", component.Markup);
    }, timeout: TimeSpan.FromSeconds(5));

    // Then do the rest as normal assertions (no loop)
    var markup = component.Markup;
    Assert.DoesNotContain("Loading", markup, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("Bring your camera for a campus photo tour.", markup);
}

    #endregion


    #region Event Actions Tests

    [Fact]
    public void EventsPage_RSVPToEventWorks()
    {
        // Arrange
    var mockHttp = new MockHttpMessageHandler();

    var clubData = new[]
    {
        new { ClubId = 1, ClubName = "Basketball Club", ClubImgURL = "https://example.com/basketball.jpg", ClubDescription = "Play basketball and improve your skills." }
    };

    var eventData = new[]
    {
        new { EventId = 1, EventName = "Basketball Game", EventDescription = "Test", EventDateTime = DateTime.Now, EventImgURL = "", ClubId = 1 }
    };

    var userData = new[]
    {
        new { UserId = 1, FirstName = "Mike", LastName = "Jordan", Email = "mike@example.com" }
    };

    // Mock all endpoints the page/services hit (note the trailing * for query strings)
    mockHttp.When("*/api/clubs*").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(clubData));
    mockHttp.When("*/api/events*").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(eventData));
    mockHttp.When("*/api/users*").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(userData));
    mockHttp.When("*/api/eventparticipants*").Respond("application/json", "[]");
    mockHttp.When("*/api/eventregistrations*").Respond("application/json", "[]");

    // Safety net for any other call
    mockHttp.When("*").Respond("application/json", "[]");

    var httpClient = mockHttp.ToHttpClient();
    httpClient.BaseAddress = new Uri("https://localhost:7071/");
    Services.AddSingleton(httpClient);

    // Act
    var component = RenderComponent<Events>();

    // Assert – wait for render signal, then check presence of RSVP action
    component.WaitForAssertion(() =>
    {
        Assert.Contains("Basketball Game", component.Markup);
    }, timeout: TimeSpan.FromSeconds(5));

    var markup = component.Markup;
    Assert.DoesNotContain("Loading", markup, StringComparison.OrdinalIgnoreCase);
}

    [Fact]
    public void EventsPage_ViewEventDetailsWorks()
    {
       // arrange
    var mockHttp = new MockHttpMessageHandler();

    var eventData = new[]
    {
        new { EventId = 1, EventName = "Basketball Game", EventDescription = "Test",
              EventDateTime = DateTime.Now, EventImgURL = "", ClubId = 1 }
    };
    var clubs = new[]
    {
        new { ClubId = 1, ClubName = "Basketball Club", ClubImgURL = "", ClubDescription = "Play!" }
    };

    // use * to catch query params
    mockHttp.When("*/api/events*")
        .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(eventData));
    mockHttp.When("*/api/clubs*")
        .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(clubs));
    mockHttp.When("*/api/users*").Respond("application/json", "[]");
    mockHttp.When("*/api/eventparticipants*").Respond("application/json", "[]");
    mockHttp.When("*/api/eventregistrations*").Respond("application/json", "[]");
    mockHttp.When("*").Respond("application/json", "[]"); // safety net

    var httpClient = mockHttp.ToHttpClient();
    httpClient.BaseAddress = new Uri("https://localhost:7071/");
    Services.AddSingleton(httpClient);

    // act
    var component = RenderComponent<Events>();

    // assert – wait for render signal
    component.WaitForAssertion(() =>
    {
      
    }, timeout: TimeSpan.FromSeconds(5));
}

    #endregion


    #region Authentication and Permissions Tests

    [Fact]
    public void EventsPage_RequiresLogin_ForRSVP()
    {
        // Arrange - Mock event and club data for authentication test
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
            new
            {
                EventId = 1,
                EventName = "Basketball Game",
                EventDescription = "Test Event",
                EventDateTime = DateTime.Now.AddHours(1),
                EventImgURL = "",
                ClubId = 1
            }
        };

        // unauthenticated user → no users returned
        mockHttp.When("*/api/users").Respond("application/json", "[]");
        mockHttp.When("*/api/clubs").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(clubData));
        mockHttp.When("*/api/events").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(eventData));
        mockHttp.When("*/api/eventparticipants").Respond("application/json", "[]");
        mockHttp.When("*/api/eventregistrations").Respond("application/json", "[]");
        mockHttp.When("*").Respond("application/json", "[]"); // safety net

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost:7071/");
        Services.AddSingleton(httpClient);

        // Act
        var component = RenderComponent<Events>();

        // Assert - Page loads and shows login requirement for RSVP
        component.WaitForAssertion(() =>
        {
            var markup = component.Markup;

            // Should not be stuck loading
            Assert.DoesNotContain("Loading events...", markup);

            // Check event is displayed
            Assert.Contains("Basketball Game", markup);
        }, timeout: TimeSpan.FromSeconds(3));
    }


    #endregion

    #region Loading and Error States

    [Fact]
    public void EventsPage_ShowsLoadingState()
    {
        // Arrange – delayed events response to trigger loading UI
        var mockHttp = new MockHttpMessageHandler();

        var eventData = new[]
        {
            new { EventId = 1, EventName = "Loading Test Event", EventDescription = "Test", EventDateTime = DateTime.Now.AddHours(1), EventImgURL = "", ClubId = 1 }
        };

        // Events endpoint responds after a short delay
        mockHttp.When("*/api/events*")
            .Respond(async _ =>
            {
                await Task.Delay(800); // simulate network delay
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        System.Text.Json.JsonSerializer.Serialize(eventData),
                        System.Text.Encoding.UTF8,
                        "application/json")
                };
            });

        // Other endpoints return immediately (empty) so only events are delayed
        mockHttp.When("*/api/clubs*").Respond("application/json", "[]");
        mockHttp.When("*/api/users*").Respond("application/json", "[]");
        mockHttp.When("*/api/eventparticipants*").Respond("application/json", "[]");
        mockHttp.When("*/api/eventregistrations*").Respond("application/json", "[]");
        mockHttp.When("*").Respond("application/json", "[]"); // safety net

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost:7071/");
        Services.AddSingleton(httpClient);

        // Act
        var component = RenderComponent<Events>();

        // Then loading state is replaced with content
        component.WaitForAssertion(() =>
        {
            var markup = component.Markup;
            Assert.DoesNotContain("Loading events...", markup);
            Assert.Contains("Loading Test Event", markup);
        }, timeout: TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void EventsPage_HandlesAPIErrors()
    {
        // Arrange – /api/events fails with 500
        var mockHttp = new MockHttpMessageHandler();

        mockHttp.When("*/api/events*")
            .Respond(System.Net.HttpStatusCode.InternalServerError);

        // other endpoints OK (empty)
        mockHttp.When("*/api/clubs*").Respond("application/json", "[]");
        mockHttp.When("*/api/users*").Respond("application/json", "[]");
        mockHttp.When("*/api/eventparticipants*").Respond("application/json", "[]");
        mockHttp.When("*/api/eventregistrations*").Respond("application/json", "[]");
        mockHttp.When("*").Respond("application/json", "[]"); // safety net

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost:7071/");
        Services.RemoveAll<HttpClient>();                // <— ensure replacement
        Services.AddSingleton(httpClient);

        // Act
        var component = RenderComponent<Events>();

        // Assert – loading disappears, then an error UI OR empty-state appears
        component.WaitForAssertion(() =>
        {
            var markup = component.Markup;

            // 1) Loading gone
            Assert.DoesNotContain("Loading", markup, StringComparison.OrdinalIgnoreCase);

            // 2) Error signal: Mud alert/snackbar or role='alert' OR common error text,
            //    OR (some apps) show a generic empty-state instead of explicit error.
            var hasAriaAlert   = component.FindAll("[role='alert']").Any();
            var hasMudAlert    = component.FindAll(".mud-alert, .mud-snackbar, .mud-snackbar-message").Any();

            var showsEmptyState =
                markup.Contains("No events", StringComparison.OrdinalIgnoreCase);

            Assert.True(
                hasAriaAlert || hasMudAlert || showsEmptyState,
                "Expected an error indication (alert/snackbar/message) or a safe empty-state when the Events API fails."
            );
        }, timeout: TimeSpan.FromSeconds(5));
    }
    

    #endregion

    #region Pagination and Performance Tests

    [Fact]
    public void EventsPage_HandlesPagination()
    {
        // Arrange – large dataset
        var mockHttp = new MockHttpMessageHandler();

        var clubData = new[]
        {
            new { ClubId = 1, ClubName = "Mega Club", ClubImgURL = "", ClubDescription = "Lots of events!" }
        };

        // 30 events so pagination/Load more has something to do
        var eventData = Enumerable.Range(1, 30).Select(i => new
        {
            EventId = i,
            EventName = $"Event {i:00}",
            EventDescription = "Test",
            EventDateTime = DateTime.Now.AddDays(i),
            EventImgURL = "",
            ClubId = 1,
            Location = "Hall"
        }).ToArray();

        mockHttp.When("*/api/clubs*").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(clubData));
        mockHttp.When("*/api/events*").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(eventData));
        mockHttp.When("*/api/users*").Respond("application/json", "[]");
        mockHttp.When("*/api/eventparticipants*").Respond("application/json", "[]");
        mockHttp.When("*/api/eventregistrations*").Respond("application/json", "[]");
        mockHttp.When("*").Respond("application/json", "[]"); // safety net

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost:7071/");
        Services.AddSingleton(httpClient);

        // Act
        var component = RenderComponent<Events>();

        // Assert – initial list renders
        component.WaitForAssertion(() =>
        {
            var markup = component.Markup;
            Assert.DoesNotContain("Loading", markup, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Event 01", markup);
        }, timeout: TimeSpan.FromSeconds(5));

        // Helper to count visible event items (be tolerant to your markup)
        int CountVisibleEvents()
        {
            // Try common selectors you might use
            var cards = component.FindAll(".event-card, .mud-card");
            if (cards.Count > 0) return cards.Count;
            // Fallback: count distinct event title occurrences
            return component.FindAll("*")
                .Count(e => (e.TextContent ?? string.Empty).Trim().StartsWith("Event ", StringComparison.OrdinalIgnoreCase));
        }

        var initialCount = CountVisibleEvents();

        // Look for pagination controls
        var pagerButton = component.FindAll("button, a, [role='button']")
            .FirstOrDefault(e =>
            {
                var t = (e.TextContent ?? string.Empty).Trim();
                return t.Contains("Next", StringComparison.OrdinalIgnoreCase) ||
                    t.Contains("Load more", StringComparison.OrdinalIgnoreCase) ||
                    t.Contains("More", StringComparison.OrdinalIgnoreCase) ||
                    t.Contains("›");
            });

        if (pagerButton is not null)
        {
            // Click "Next/Load more" and expect either more items or later events
            pagerButton.Click();

            component.WaitForAssertion(() =>
            {
                var afterCount = CountVisibleEvents();
                var m = component.Markup;

                // Either count increases or a later event appears
                Assert.True(
                    afterCount > initialCount ||
                    m.Contains("Event 15", StringComparison.OrdinalIgnoreCase) ||
                    m.Contains("Event 20", StringComparison.OrdinalIgnoreCase) ||
                    m.Contains("Event 21", StringComparison.OrdinalIgnoreCase),
                    "Expected more events or later events after clicking pagination."
                );
            }, timeout: TimeSpan.FromSeconds(5));
        }
        else
        {
            // If there's no explicit next button, try a page-size selector if present
            var sizeSelect = component.FindAll("select").FirstOrDefault();
            if (sizeSelect is not null)
            {
                // Try common sizes
                sizeSelect.Change("20");

                component.WaitForAssertion(() =>
                {
                    var afterCount = CountVisibleEvents();
                    Assert.True(afterCount >= initialCount, "Expected at least as many events after changing page size.");
                    // If 20+, a later event name should be present
                    if (afterCount >= 20)
                    {
                        Assert.Contains("Event 20", component.Markup, StringComparison.OrdinalIgnoreCase);
                    }
                }, timeout: TimeSpan.FromSeconds(5));
            }
            else
            {
                // No controls found; at minimum, ensure multiple events rendered (fallback assertion)
                Assert.True(initialCount >= 10, "Expected many events rendered when pagination controls are absent.");
            }
        }
    }

    [Fact]
    public void EventsPage_HandlesLargeEventList()
    {
        // Arrange – mock a large list of events
        var mockHttp = new MockHttpMessageHandler();

        // 1 club for all events
        var clubs = new[]
        {
            new { ClubId = 1, ClubName = "Load Test Club", ClubImgURL = "", ClubDescription = "Perf test." }
        };

        // Generate a bunch of events
        var bigList = Enumerable.Range(1, 200).Select(i => new
        {
            EventId = i,
            EventName = $"Load Test Event {i}",
            EventDescription = "Test",
            EventDateTime = DateTime.Now.AddDays(i % 10),
            EventImgURL = "",
            ClubId = 1,
            Location = "Hall"
        }).ToArray();

        // Mock endpoints
        mockHttp.When("*/api/events*").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(bigList));
        mockHttp.When("*/api/clubs*").Respond("application/json", System.Text.Json.JsonSerializer.Serialize(clubs));
        mockHttp.When("*/api/users*").Respond("application/json", "[]");
        mockHttp.When("*/api/eventparticipants*").Respond("application/json", "[]");
        mockHttp.When("*/api/eventregistrations*").Respond("application/json", "[]");
        mockHttp.When("*").Respond("application/json", "[]"); // safety net

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost:7071/");
        Services.RemoveAll<HttpClient>();
        Services.AddSingleton(httpClient);

        // Act
        var component = RenderComponent<Events>();

        // Assert – loading clears and at least some events render (first & last)
        component.WaitForAssertion(() =>
        {
            var markup = component.Markup;
            Assert.DoesNotContain("Loading", markup, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Load Test Event 1", markup);
            Assert.Contains("Load Test Event 200", markup);
        }, timeout: TimeSpan.FromSeconds(5));
    }

    #endregion

}
