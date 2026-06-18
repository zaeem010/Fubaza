using Fubaza.Application.DTO.Services;

namespace Fubaza.Application.Core.Contracts.Services
{
	public interface IMailService
	{
		Task SendAsync(MailRequest request);
	}
}
