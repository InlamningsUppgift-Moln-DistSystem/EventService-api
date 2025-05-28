using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventMicroService.Data;
using EventMicroService.Models;
using Microsoft.EntityFrameworkCore;

namespace EventMicroService.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly ApplicationDbContext _context;

        public EventRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Event> CreateAsync(Event newEvent)
        {
            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();
            return newEvent;
        }

        public async Task<IEnumerable<Event>> GetAllByOwnerAsync(string ownerId)
        {
            return await _context.Events
                .Where(e => e.OwnerId == ownerId)
                .Include(e => e.Attendees)
                .ToListAsync();
        }

        public async Task<Event> GetByIdAsync(Guid id)
        {
            return await _context.Events
                .Include(e => e.Attendees)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task DeleteAsync(Event evnt)
        {
            _context.Events.Remove(evnt);
            await _context.SaveChangesAsync();
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<Event>> GetByMonthAsync(int year, int month)
        {
            return await _context.Events
                .Where(e => e.StartDate.Year == year && e.StartDate.Month == month)
                .Include(e => e.Attendees)
                .ToListAsync();
        }
        public async Task AddAttendeeAsync(Attendee attendee)
        {
            _context.Attendees.Add(attendee);
            await _context.SaveChangesAsync();
        }
        public async Task RemoveAttendeeAsync(Attendee attendee)
        {
            _context.Attendees.Remove(attendee);
            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<Event>> GetEventsUserIsAttending(string userId)
        {
            return await _context.Attendees
                .Where(a => a.UserId == userId)
                .Select(a => a.Event)
                .ToListAsync();
        }
        public async Task<IEnumerable<Event>> GetFullEventsUserIsAttending(string userId)
        {
            return await _context.Attendees
                .Where(a => a.UserId == userId)
                .Include(a => a.Event)
                .ThenInclude(e => e.Attendees)
                .Select(a => a.Event)
                .ToListAsync();
        }


    }
}
