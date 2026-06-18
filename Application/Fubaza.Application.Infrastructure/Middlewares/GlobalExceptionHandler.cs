using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Newtonsoft.Json.Serialization;

using System.Net;
using System.Text.Json;

using Fubaza.Application.Core.Contracts.Serialization;
using Fubaza.Application.Core.Exceptions;
using Fubaza.Application.Core.Serialization;
using Fubaza.Application.Core.Settings;
using Fubaza.Application.Core.Wrapper;



namespace Fubaza.Application.Infrastructure.Middlewares
{
	public class GlobalExceptionHandler : IMiddleware
	{
		private readonly ILogger<GlobalExceptionHandler> _logger;
		private readonly SerializationSettings _serializationSettings;
		private readonly IJsonSerializer _jsonSerializer;

		public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IOptions<SerializationSettings> serializationSettings, IJsonSerializer jsonSerializer)
		{
			_logger = logger;
			_serializationSettings = serializationSettings.Value;
			_jsonSerializer = jsonSerializer;
		}

		public async Task InvokeAsync(HttpContext context, RequestDelegate next)
		{
			try
			{
				await next(context);
			}
			catch (Exception exception)
			{
				var response = context.Response;
				response.ContentType = "application/json";
				var responseModel = await ErrorResult<string>.ReturnErrorAsync(exception.Message);
				responseModel.Source = exception.Source;
				responseModel.Exception = exception.Message;
				_logger.LogError(exception.Message);
				switch (exception)
				{
					case CustomException e:
						response.StatusCode = responseModel.ErrorCode = (int)e.StatusCode;
						responseModel.Messages = e.ErrorMessages;
						break;

					case KeyNotFoundException e:
						response.StatusCode = responseModel.ErrorCode = (int)HttpStatusCode.NotFound;
						break;

					default:
						response.StatusCode = responseModel.ErrorCode = (int)HttpStatusCode.InternalServerError;
						break;
				}

				var result = string.Empty;
				if (_serializationSettings.UseNewtonsoftJson)
				{
					result = _jsonSerializer.Serialize(responseModel, new JsonSerializerSettingsOptions
					{
						JsonSerializerSettings = { ContractResolver = new CamelCasePropertyNamesContractResolver() }
					});
				}
				else if (_serializationSettings.UseSystemTextJson)
				{
					result = _jsonSerializer.Serialize(responseModel, new JsonSerializerSettingsOptions
					{
						JsonSerializerOptions = { DictionaryKeyPolicy = JsonNamingPolicy.CamelCase, PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
					});
				}

				await response.WriteAsync(result);
			}
		}


	}
}
