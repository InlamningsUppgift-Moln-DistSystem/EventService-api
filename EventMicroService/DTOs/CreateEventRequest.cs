using System;
using System.ComponentModel.DataAnnotations;

namespace EventMicroService.DTOs
{
    public class CreateEventRequest
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Location { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Url]
        public string ImageUrl { get; set; }

    }
}
