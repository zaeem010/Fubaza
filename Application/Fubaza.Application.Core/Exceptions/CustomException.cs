using System.Net;

namespace Fubaza.Application.Core.Exceptions
{
    public class CustomException : Exception
    {
        public List<string> ErrorMessages { get; } = new();
        public HttpStatusCode StatusCode { get; }

        public CustomException(
            string message,
            List<string>? errors = null,
            HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
            : base(message)
        {
            ErrorMessages = errors ?? new List<string>();
            StatusCode = statusCode;
        }
    }
}
