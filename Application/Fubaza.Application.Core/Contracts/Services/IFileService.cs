using Fubaza.Application.Dto.Services;

namespace Fubaza.Application.Core.Contracts.Services
{
	public interface IFileService
	{
		Task<string> GetTemplateContent(string template, Dictionary<string, string> placeholders);
        Task<string> UploadAsync(UploadRequest request);

    }
}
