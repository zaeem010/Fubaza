using Fubaza.Application.DTO.Enums;

using Microsoft.AspNetCore.Http;


namespace Fubaza.Application.DTO.Services
{
    public class AddOrUpdatePlayerRequest
    {
        public Guid? PlayerId { get; set; }
        public Guid ClubId { get; set; }
        public string? FullName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Nationality { get; set; }
        public int? Gender { get; set; }
        public Guid? PlayingPositionId { get; set; }
        public bool IsCaption { get; set; }
        public int? JerseyNumber { get; set; }
        public int? StrongFoot { get; set; }
        public int? ThrowingHand { get; set; }
        public virtual List<PlayerDocumentRequest>? Images { get; set; } 
    }
    public class PlayerDocumentRequest
    {
        public IFormFile? File { get; set; }
        public PlayerDocumentType ImageType { get; set; }
    }
}
