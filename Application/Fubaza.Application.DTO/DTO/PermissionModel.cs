namespace Fubaza.Application.DTO.DTO
{
    public class PermissionModel
    {
        public Guid RoleId { get; set; }
        public IList<RoleClaimsModel>? RoleClaims { get; set; }
    }
    public class RoleClaimsModel
    {
        public Guid RoleId { get; set; }
        public string? Type { get; set; }
        public string? Value { get; set; }
        public bool Selected { get; set; }
        public string? Description { get; set; }
        public string? Group { get; set; }
    }
}
