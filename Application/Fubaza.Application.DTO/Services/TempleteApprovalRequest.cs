namespace Fubaza.Application.DTO.Services
{
    public class TempleteApprovalRequest
    {
        public List<Guid> TemplateIds { get; set; } = new();
        public bool IsApproved { get; set; }
    }
}
