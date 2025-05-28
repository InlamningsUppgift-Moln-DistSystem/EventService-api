using System;

namespace EventMicroService.Models
{
    public class Attendee
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UserId { get; set; }
        public Guid EventId { get; set; }

        public Event Event { get; set; }
    }
}
