namespace Fubaza.Application.DTO.DTO
{
    public class TempleteDto
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public bool IsApproved { get; set; }
        public bool IsDraft { get; set; }
        public string? SportName { get; set; }
        public Guid? SportId { get; set; }
        public int? TempleteTypeId { get; set; }
        public string? TempleteTypeName { get; set; }
        public string? TempleteUrl { get; set; }
        public string? FileUrl { get; set; }
    }
}
