

namespace Fubaza.Application.DTO.DTO
{
    public class UpcomingPostDto
    {
        public Guid Id { get; set; }
        public string? Caption { get; set; }
        public DateTime ScheduleDateTime { get; set; }
        public string? PostUrl { get; set; }
    }
}
