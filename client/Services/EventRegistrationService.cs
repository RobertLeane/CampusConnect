using Microsoft.JSInterop;
using Client.Models;

namespace Client.Services;

public class EventRegistrationService
{
    private readonly EventApi _eventApi;
    private readonly EventParticipantApi _eventParticipantApi;
    private readonly IJSRuntime _js;

    public EventRegistrationService(EventApi eventApi, EventParticipantApi eventParticipantApi, IJSRuntime js)
    {
        _eventApi = eventApi;
        _eventParticipantApi = eventParticipantApi;
        _js = js;
    }

    public async Task HandleRsvpAsync(int eventId, int clubId, int currentUserId)
    {
        if (currentUserId == 0)
        {
            await _js.InvokeVoidAsync("alert", "Please log in to RSVP.").ConfigureAwait(false);
            return;
        }

        try 
        {
            // RSVP through the event system - handles everything including registration
            var resp = await _eventApi.RsvpAsync(eventId, currentUserId).ConfigureAwait(false);
            
            if (!resp.IsSuccessStatusCode)
            {
                var msg = ((int)resp.StatusCode) switch
                {
                    403 => "You must join this club to RSVP.",
                    404 => "Event or user not found.",
                    409 => "You already RSVPed.",
                    _   => $"RSVP failed: {resp.StatusCode}"
                };
                await _js.InvokeVoidAsync("alert", msg).ConfigureAwait(false);
                return;
            }

            // RSVP was successful
            await _js.InvokeVoidAsync("alert", "RSVP successful 🎉").ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            await _js.InvokeVoidAsync("alert", $"Network error during RSVP: {ex.Message}").ConfigureAwait(false);
        }
        catch (Exception)
        {
            throw;
        }
    }
}