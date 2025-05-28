using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventMicroService.DTOs;

namespace EventMicroService.Services
{
    public interface IEventService
    {
        Task<EventResponse> CreateEventAsync(string ownerId, CreateEventRequest request);
        Task<IEnumerable<EventResponse>> GetEventsByOwnerAsync(string ownerId);
        Task<EventResponse> GetEventByIdAsync(Guid id);
        Task<bool> DeleteEventAsync(Guid id, string requesterId);
    }
}
