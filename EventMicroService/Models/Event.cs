using System;
using System.Collections.Generic;

namespace EventMicroService.Models
{
    public class Event
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; }
        public string Location { get; set; }
        public DateTime StartDate { get; set; }
        public string OwnerId { get; set; }

        public ICollection<Attendee> Attendees { get; set; }
    }
}
