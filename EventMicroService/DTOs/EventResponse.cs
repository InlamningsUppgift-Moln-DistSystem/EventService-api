using System;

namespace EventMicroService.DTOs
{
    public class EventResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Location { get; set; }
        public DateTime StartDate { get; set; }
        public string OwnerId { get; set; }
        public int AttendeeCount { get; set; }
    }
}
