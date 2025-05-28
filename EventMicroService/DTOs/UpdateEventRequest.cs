namespace EventMicroService.DTOs
{
    public class UpdateEventRequest
    {
        public string Title { get; set; }
        public string Location { get; set; }
        public DateTime StartDate { get; set; }
        public string ImageUrl { get; set; }
    }

}
