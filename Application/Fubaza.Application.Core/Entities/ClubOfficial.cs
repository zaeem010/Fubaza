using Fubaza.Application.DTO.Enums;

namespace Fubaza.Application.Core.Entities
{
    public class ClubOfficial
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? Name { get; set; }
        public Gender? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime? JoiningDate { get; set; }
        public Guid DesignationId { get; set; }
        public Guid ClubId { get; set; }
        public bool IsDeleted { get; set; } 
        public Designation? Designation { get; set; }
        public Club? Club { get; set; }
        public virtual ClubOfficialDocument? Document { get; set; }
    }
}
