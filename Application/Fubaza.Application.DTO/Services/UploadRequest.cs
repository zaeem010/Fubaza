using Fubaza.Application.DTO.Enums;

namespace Fubaza.Application.Dto.Services
{
	public class UploadRequest
	{
		public string? FileName { get; set; }
		public string? Extension { get; set; }
		public UploadType UploadType { get; set; }
		public byte[]? FileContent { get; set; }

		public bool IsBase64 { get; set; }
		public string? Base64 { get; set; }
	}
}
