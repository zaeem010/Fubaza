namespace Fubaza.Application.DTO.Services
{
    public class MatchLineUpRequest
    {
        public Guid MatchdayId { get; set; }
        public List<Guid> PlayerIds { get; set; } = new();
    }
}
