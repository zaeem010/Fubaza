namespace Fubaza.Application.DTO.DTO
{
    public class PlayerCountBySportDto
    {
        public Guid SportId { get; set; }
        public string SportName { get; set; } = string.Empty;
        public int PlayerCount { get; set; }
    }
}
