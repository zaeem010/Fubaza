using Fubaza.Application.DTO.Enums;

namespace Fubaza.Application.DTO.Services
{
    public class PaginationRequest
    {
        public Guid? SportId { get; set; }
        public Guid? UserId { get; set; }
        public Guid? ClubId { get; set; }
        public Guid? RoleId { get; set; }
        public bool? IsApproved { get; set; }
        public TempleteType? TempleteType { get; set; }
        public int PageNumber { get; set; } = 1; 
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; } // Add this property
    }
}
