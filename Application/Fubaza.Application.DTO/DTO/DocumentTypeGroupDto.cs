namespace Fubaza.Application.DTO.DTO
{
    public class DocumentTypeGroupDto
    {
        public int Count { get; set; }
        public List<DocumentPhotoDto> Photos { get; set; } = new();
    }
    public class DocumentPhotoDto
    {
        public string Name { get; set; } = null!;
        public string Url { get; set; } = null!;
    }
}
