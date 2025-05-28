using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using EventMicroService.DTOs;
using EventMicroService.Services;
using Microsoft.AspNetCore.Authorization;

namespace EventMicroService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;

        public EventsController(IEventService eventService)
        {
            _eventService = eventService;
        }

        // GET /api/events/mine
        [HttpGet("mine")]
        [Authorize]
        public async Task<IActionResult> GetMyEvents()
        {
            var userId = User.FindFirst("sub")?.Value;
            if (userId == null) return Unauthorized();

            var events = await _eventService.GetEventsByOwnerAsync(userId);
            return Ok(events);
        }

        // POST /api/events
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEventRequest request)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (userId == null) return Unauthorized();

            var created = await _eventService.CreateEventAsync(userId, request);
            return CreatedAtAction(nameof(GetEventById), new { id = created.Id }, created);
        }

        // GET /api/events/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEventById(Guid id)
        {
            var evnt = await _eventService.GetEventByIdAsync(id);
            if (evnt == null) return NotFound();
            return Ok(evnt);
        }

        // DELETE /api/events/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteEvent(Guid id)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (userId == null) return Unauthorized();

            var success = await _eventService.DeleteEventAsync(id, userId);
            if (!success) return Forbid();

            return NoContent();
        }
    }
}
