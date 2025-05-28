using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventMicroService.Models;

namespace EventMicroService.Repositories
{
    public interface IEventRepository
    {
        Task<Event> CreateAsync(Event newEvent);
        Task<IEnumerable<Event>> GetAllByOwnerAsync(string ownerId);
        Task<Event> GetByIdAsync(Guid id);
        Task DeleteAsync(Event evnt);
        Task<IEnumerable<Event>> GetByMonthAsync(int year, int month);
        Task SaveChangesAsync();
    }

}
