using System.Globalization;


namespace Fubaza.Application.Core.Extensions
{
    public static class LocalizationExtensions
    {
        /// <summary>
        /// Returns the localized version of a property based on the current UI culture.
        /// Automatically falls back to the English text if the localized value is empty or null.
        /// </summary>
        public static string Localize(this string? english, string? german)
        {
            var lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLower();

            return lang switch
            {
                "de" => !string.IsNullOrWhiteSpace(german) ? german! : english ?? string.Empty,
                _ => english ?? string.Empty
            };
        }
    }
}
