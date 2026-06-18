using Fubaza.Application.DTO.Enums;

namespace Fubaza.Application.DTO.DTO
{
    public class DepartmentOfficialDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? DesignationTitle { get; set; }
        public Gender? Gender { get; set; }
        public Guid DesignationId { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime? JoiningDate { get; set; }
        public string? DocumentUrl { get; set; }
    }
}
