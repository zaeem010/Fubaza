using Microsoft.AspNetCore.Authorization;

namespace Fubaza.Application.Infrastructure.Permissions
{
	internal class PermissionRequirement : IAuthorizationRequirement
	{
		public string Permission { get; private set; }

		public PermissionRequirement(string permission)
		{
			Permission = permission;
		}
	}
}
