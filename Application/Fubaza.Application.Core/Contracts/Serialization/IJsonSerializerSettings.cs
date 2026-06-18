using Newtonsoft.Json;

namespace Fubaza.Application.Core.Contracts.Serialization
{
	public interface IJsonSerializerSettings
	{
		/// <summary>
		/// Settings for <see cref="Newtonsoft.Json"/>.
		/// </summary>
		public JsonSerializerSettings JsonSerializerSettings { get; }
	}
}
