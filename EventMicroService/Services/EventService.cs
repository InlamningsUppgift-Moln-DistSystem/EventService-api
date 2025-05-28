using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventMicroService.DTOs;
using EventMicroService.Models;
using EventMicroService.Repositories;

namespace EventMicroService.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;

        public EventService(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public async Task<EventResponse> CreateEventAsync(string ownerId, CreateEventRequest request)
        {
            var newEvent = new Event
            {
                Title = request.Title,
                Location = request.Location,
                StartDate = request.StartDate,
                OwnerId = ownerId
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

            await _eventRepository.DeleteAsync(evnt);
            return true;
        }

        private static EventResponse ToResponse(Event e) => new EventResponse
        {
            Id = e.Id,
            Title = e.Title,
            Location = e.Location,
            StartDate = e.StartDate,
            OwnerId = e.OwnerId,
            AttendeeCount = e.Attendees?.Count ?? 0
        };
    }
}
