// 📁 EventsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using EventMicroService.DTOs;
using EventMicroService.Services;
using Microsoft.AspNetCore.Http;

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

        private string? GetUserId() =>
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        [HttpGet("mine")]
        [Authorize]
        public async Task<IActionResult> GetMyEvents()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var events = await _eventService.GetEventsByOwnerAsync(userId);
            return Ok(events);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEventRequest request)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var created = await _eventService.CreateEventAsync(userId, request);
            return CreatedAtAction(nameof(GetEventById), new { id = created.Id }, created);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEventById(Guid id)
        {
            var evnt = await _eventService.GetEventByIdAsync(id);
            if (evnt == null) return NotFound();
            return Ok(evnt);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteEvent(Guid id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var success = await _eventService.DeleteEventAsync(id, userId);
            return success ? NoContent() : Forbid();
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateEvent(Guid id, [FromBody] UpdateEventRequest request)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var updated = await _eventService.UpdateEventAsync(id, userId, request);
            return updated == null ? Forbid() : Ok(updated);
        }

        [HttpPut("upload-image")]
        [Authorize]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile file, [FromQuery] string? oldImageUrl = null)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            try
            {
                var imageUrl = await _eventService.UploadEventImageAsync(userId, file, deleteOldImage: true, oldImageUrl);
                return Ok(new { imageUrl });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }


        [HttpGet("month")]
        public async Task<IActionResult> GetEventsByMonth([FromQuery] int year, [FromQuery] int month)
        {
            var events = await _eventService.GetEventsByMonthAsync(year, month);
            return Ok(events);
        }

        [HttpPost("{eventId}/attend")]
        [Authorize]
        public async Task<IActionResult> AttendEvent(Guid eventId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            try
            {
                var success = await _eventService.AttendEventAsync(userId, eventId);
                return success
                    ? Ok(new { message = "You are now attending the event." })
                    : BadRequest(new { error = "Already attending or event not found." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpDelete("{eventId}/attend")]
        [Authorize]
        public async Task<IActionResult> UnattendEvent(Guid eventId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var success = await _eventService.UnattendEventAsync(userId, eventId);
            return success ? Ok(new { message = "You are no longer attending the event." }) : BadRequest(new { error = "Unable to unattend." });
        }

        [HttpGet("attending")]
        [Authorize]
        public async Task<IActionResult> GetAttendingEventIds()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var ids = await _eventService.GetEventIdsUserIsAttending(userId);
            return Ok(ids); // Returnera lista med GUIDs
        }
        [HttpGet("my-attending")]
        [Authorize]
        public async Task<IActionResult> GetMyAttendingEvents()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var events = await _eventService.GetEventsUserIsAttending(userId);
            return Ok(events); // → List<EventResponse>
        }

    }
}
