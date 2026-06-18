namespace Fubaza.Application.DTO.DTO
{
    public class ClubCountBySportDto
    {
        public Guid SportId { get; set; }
        public string SportName { get; set; } = string.Empty;
        public int ClubCount { get; set; }
    }
}
