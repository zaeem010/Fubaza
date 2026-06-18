using Microsoft.Extensions.Options;
using Newtonsoft.Json;

using Fubaza.Application.Core.Contracts.Serialization;


namespace Fubaza.Application.Core.Serialization
{
    public class NewtonSoftJsonSerializer : IJsonSerializer
    {
        private readonly JsonSerializerSettings _defaultSettings;

        public NewtonSoftJsonSerializer(IOptions<JsonSerializerSettingsOptions> settings)
        {
            _defaultSettings = settings.Value.JsonSerializerSettings
                               ?? throw new ArgumentNullException(nameof(settings));
        }

        public T Deserialize<T>(string text, IJsonSerializerSettingsOptions? settings = null)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Input text cannot be null or whitespace.", nameof(text));

            var jsonSettings = settings?.JsonSerializerSettings ?? _defaultSettings;
            return JsonConvert.DeserializeObject<T>(text, jsonSettings)
                   ?? throw new JsonSerializationException($"Deserialization failed for type {typeof(T).FullName}");
        }

        public string Serialize<T>(T obj, IJsonSerializerSettingsOptions? settings = null)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj), "Cannot serialize a null object.");

            var jsonSettings = settings?.JsonSerializerSettings ?? _defaultSettings;
            return JsonConvert.SerializeObject(obj, jsonSettings);
        }
    }
}

