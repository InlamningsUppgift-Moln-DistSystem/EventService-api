using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using EventMicroService.DTOs;
using EventMicroService.Models;
using EventMicroService.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace EventMicroService.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;
        private readonly BlobServiceClient _blobServiceClient;

        public EventService(IEventRepository eventRepository, BlobServiceClient blobServiceClient)
        {
            _eventRepository = eventRepository;
            _blobServiceClient = blobServiceClient;
        }

        public async Task<EventResponse> CreateEventAsync(string ownerId, CreateEventRequest request)
        {
            try
            {
                var newEvent = new Event
                {
                    Title = request.Title,
                    Location = request.Location,
                    StartDate = request.StartDate,
                    OwnerId = ownerId,
                    ImageUrl = request.ImageUrl,
                    Attendees = new List<Attendee>() // Viktigt! Undvik null
                };

                var created = await _eventRepository.CreateAsync(newEvent);

                // Lägg till detta loggobjekt:
                return new EventResponse
                {
                    Id = created.Id,
                    Title = created.Title,
                    Location = created.Location,
                    StartDate = created.StartDate,
                    OwnerId = created.OwnerId,
                    AttendeeCount = created.Attendees?.Count ?? 0,
                    ImageUrl = created.ImageUrl
                };
            }
            catch (Exception ex)
            {
                throw new Exception("❌ CreateEventAsync crashed: " + ex.Message, ex);
            }
        }


        public async Task<IEnumerable<EventResponse>> GetEventsByOwnerAsync(string ownerId)
        {
            var events = await _eventRepository.GetAllByOwnerAsync(ownerId);
            return events.Select(e => ToResponse(e));
        }

        public async Task<EventResponse> GetEventByIdAsync(Guid id)
        {
            var evnt = await _eventRepository.GetByIdAsync(id);
            return evnt == null ? null : ToResponse(evnt);
        }

        public async Task<bool> DeleteEventAsync(Guid id, string requesterId)
        {
            var evnt = await _eventRepository.GetByIdAsync(id);
            if (evnt == null || evnt.OwnerId != requesterId)
                return false;

            if (!string.IsNullOrEmpty(evnt.ImageUrl))
                await DeleteBlobAsync(evnt.ImageUrl);

            await _eventRepository.DeleteAsync(evnt);
            return true;
        }

        public async Task<EventResponse> UpdateEventAsync(Guid id, string userId, UpdateEventRequest request)
        {
            var evnt = await _eventRepository.GetByIdAsync(id);
            if (evnt == null || evnt.OwnerId != userId)
                return null;

            evnt.Title = request.Title;
            evnt.Location = request.Location;
            evnt.StartDate = request.StartDate;

            if (!string.IsNullOrEmpty(request.ImageUrl) && evnt.ImageUrl != request.ImageUrl)
            {
                if (!string.IsNullOrEmpty(evnt.ImageUrl))
                    await DeleteBlobAsync(evnt.ImageUrl);

                evnt.ImageUrl = request.ImageUrl;
            }

            await _eventRepository.SaveChangesAsync();

            var full = await _eventRepository.GetByIdAsync(evnt.Id);
            return ToResponse(full);
        }

        public async Task<string> UploadEventImageAsync(string userId, IFormFile file, bool deleteOldImage = false, string? oldImageUrl = null)
        {
            if (file == null || file.Length == 0)
                throw new Exception("No file provided.");

            if (file.Length > 4 * 1024 * 1024)
                throw new Exception("File too large. Max size is 4MB.");

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!new[] { ".jpg", ".jpeg", ".png" }.Contains(extension))
                throw new Exception("Invalid file format. Only .jpg, .jpeg, and .png are allowed.");

            var container = _blobServiceClient.GetBlobContainerClient("eventimages");
            await container.CreateIfNotExistsAsync();
            await container.SetAccessPolicyAsync(PublicAccessType.Blob);

            if (deleteOldImage && !string.IsNullOrEmpty(oldImageUrl))
                await DeleteBlobAsync(oldImageUrl);

            var blobName = $"{userId}-{Guid.NewGuid()}{extension}";
            var blobClient = container.GetBlobClient(blobName);

            using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });

            return blobClient.Uri.ToString();
        }

        private async Task DeleteBlobAsync(string imageUrl)
        {
            try
            {
                var container = _blobServiceClient.GetBlobContainerClient("eventimages");
                var uri = new Uri(imageUrl);
                var blobName = Path.GetFileName(uri.LocalPath);
                var blobClient = container.GetBlobClient(blobName);
                await blobClient.DeleteIfExistsAsync();
            }
            catch { /* ev. logga fel */ }
        }

        private static EventResponse ToResponse(Event e) => new EventResponse
        {
            Id = e.Id,
            Title = e.Title,
            Location = e.Location,
            StartDate = e.StartDate,
            OwnerId = e.OwnerId,
            AttendeeCount = e.Attendees?.Count ?? 0,
            ImageUrl = e.ImageUrl
        };
        public async Task<IEnumerable<EventResponse>> GetEventsByMonthAsync(int year, int month)
        {
            var events = await _eventRepository.GetByMonthAsync(year, month);
            return events
                .OrderBy(e => e.StartDate)
                .Select(ToResponse);
        }
        public async Task<bool> AttendEventAsync(string userId, Guid eventId)
        {
            var evnt = await _eventRepository.GetByIdAsync(eventId);
            if (evnt == null) return false;

            var already = evnt.Attendees?.Any(a => a.UserId == userId) ?? false;
            if (already) return false;

            var attendee = new Attendee
            {
                UserId = userId,
                EventId = eventId
            };

            await _eventRepository.AddAttendeeAsync(attendee);
            return true;
        }


        public async Task<bool> UnattendEventAsync(string userId, Guid eventId)
        {
            var evnt = await _eventRepository.GetByIdAsync(eventId);
            var attendee = evnt?.Attendees.FirstOrDefault(a => a.UserId == userId);
            if (attendee == null) return false;

            await _eventRepository.RemoveAttendeeAsync(attendee);
            return true;
        }

        public async Task<IEnumerable<Guid>> GetEventIdsUserIsAttending(string userId)
        {
            var all = await _eventRepository.GetEventsUserIsAttending(userId);
            return all.Select(e => e.Id);
        }
        public async Task<IEnumerable<EventResponse>> GetEventsUserIsAttending(string userId)
        {
            var events = await _eventRepository.GetEventsUserIsAttending(userId);
            return events.Select(ToResponse);
        }

    }
}
