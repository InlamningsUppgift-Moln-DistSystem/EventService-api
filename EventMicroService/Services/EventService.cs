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
            var newEvent = new Event
            {
                Title = request.Title,
                Location = request.Location,
                StartDate = request.StartDate,
                OwnerId = ownerId,
                ImageUrl = request.ImageUrl
            };

            var created = await _eventRepository.CreateAsync(newEvent);
            return ToResponse(created);
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

            // Delete image if exists
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

            // ta bort gammal bild om ny sätts
            if (!string.IsNullOrEmpty(request.ImageUrl) && evnt.ImageUrl != request.ImageUrl)
            {
                if (!string.IsNullOrEmpty(evnt.ImageUrl))
                    await DeleteBlobAsync(evnt.ImageUrl);

                evnt.ImageUrl = request.ImageUrl;
            }

            await _eventRepository.SaveChangesAsync();
            return ToResponse(evnt);
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
            catch { /* logga ev. fel */ }
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
    }
}
