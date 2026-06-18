namespace Fubaza.Application.DTO.DTO
{
    public class SportDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public bool IsDeleted { get; set; }
        public string? NormalizedName { get; set; }
    }
}
