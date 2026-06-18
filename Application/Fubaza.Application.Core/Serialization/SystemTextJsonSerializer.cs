using Microsoft.Extensions.Options;

using System.Text.Json;

using Fubaza.Application.Core.Contracts.Serialization;

namespace Fubaza.Application.Core.Serialization
{
    public class SystemTextJsonSerializer : IJsonSerializer
    {
        private readonly JsonSerializerOptions _defaultOptions;

        public SystemTextJsonSerializer(IOptions<JsonSerializerSettingsOptions> options)
        {
            _defaultOptions = options?.Value?.JsonSerializerOptions
                              ?? throw new ArgumentNullException(nameof(options),
                                  "JsonSerializerOptions cannot be null.");
        }

        public T Deserialize<T>(string data, IJsonSerializerSettingsOptions? options = null)
        {
            if (string.IsNullOrWhiteSpace(data))
                throw new ArgumentException("Input data cannot be null or whitespace.", nameof(data));

            var jsonOptions = options?.JsonSerializerOptions ?? _defaultOptions;

            return JsonSerializer.Deserialize<T>(data, jsonOptions)
                   ?? throw new JsonException($"Deserialization failed for type {typeof(T).FullName}.");
        }

        public string Serialize<T>(T data, IJsonSerializerSettingsOptions? options = null)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Cannot serialize a null object.");

            var jsonOptions = options?.JsonSerializerOptions ?? _defaultOptions;

            return JsonSerializer.Serialize(data, jsonOptions);
        }
    }
}
