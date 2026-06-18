using Newtonsoft.Json;
using System.Text.Json;

using Fubaza.Application.Core.Contracts.Serialization;

namespace Fubaza.Application.Core.Serialization
{
	public class JsonSerializerSettingsOptions : IJsonSerializerSettingsOptions
	{
		public JsonSerializerOptions JsonSerializerOptions { get; } = new();
		public JsonSerializerSettings JsonSerializerSettings { get; } = new();
	}
}
