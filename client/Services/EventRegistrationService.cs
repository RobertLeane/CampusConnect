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
            await _js.InvokeVoidAsync("alert", "Please log in to RSVP.");
            return;
        }

        try 
        {
            // First try to RSVP through the event system
            var resp = await _eventApi.RsvpAsync(eventId, currentUserId);
            
            if (!resp.IsSuccessStatusCode)
            {
                var msg = ((int)resp.StatusCode) switch
                {
                    401 => "Please register or sign in to RSVP.",
                    403 => "You must join this club to RSVP.",
                    404 => "Event not found.",
                    409 => "You already RSVPed.",
                    _   => $"RSVP failed: {resp.StatusCode}"
                };
                await _js.InvokeVoidAsync("alert", msg);
                return;
            }

            // If RSVP successful, register the user for the event
            var participant = new EventParticipant
            {
                EventId = eventId,
                ClubId = clubId
                // MemberId will be determined server-side based on authenticated user
            };

            var result = await _eventParticipantApi.RegisterForEventAsync(participant);
            if (result != null)
            {
                await _js.InvokeVoidAsync("alert", "RSVP successful 🎉");
            }
            else 
            {
                await _js.InvokeVoidAsync("alert", "RSVP recorded but event registration failed. Please try again.");
            }
        }
        catch (Exception ex)
        {
            await _js.InvokeVoidAsync("alert", $"Error during RSVP: {ex.Message}");
        }
    }
}