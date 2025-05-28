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
        Task<EventResponse> UpdateEventAsync(Guid id, string userId, UpdateEventRequest request);

        Task<IEnumerable<EventResponse>> GetEventsByMonthAsync(int year, int month);

        Task<string> UploadEventImageAsync(string userId, IFormFile file, bool deleteOldImage = false, string? oldImageUrl = null);
    }
}
