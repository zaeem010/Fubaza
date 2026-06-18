namespace Fubaza.Application.Core.Extensions
{
	public static class ExceptionExtensions
	{
		public static string GetMessage(this Exception ex)
		{
			return ex.InnerException != null && string.IsNullOrEmpty(ex.InnerException.Message) ? ex.InnerException.Message : ex.Message;
		}
	}
}
