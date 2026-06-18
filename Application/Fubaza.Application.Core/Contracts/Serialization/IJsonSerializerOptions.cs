using System.Text.Json;

namespace Fubaza.Application.Core.Contracts.Serialization
{
	public interface IJsonSerializerOptions
	{
		/// <summary>
		/// Options for <see cref="System.Text.Json"/>.
		/// </summary>
		public JsonSerializerOptions JsonSerializerOptions { get; }
	}
}
