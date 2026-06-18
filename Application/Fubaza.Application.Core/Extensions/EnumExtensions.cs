using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;


namespace Fubaza.Application.Core.Extensions
{
    public static class EnumExtensions
    {
        public static string ToDescriptionString(this Enum value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var fieldInfo = value.GetType().GetField(value.ToString());
            if (fieldInfo == null)
                return value.ToString();

            var descriptionAttribute = fieldInfo.GetCustomAttribute<DescriptionAttribute>();

            return descriptionAttribute?.Description ?? value.ToString();
        }

        public static string GetDisplayName(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = field?.GetCustomAttribute<DisplayAttribute>();
            return attribute?.GetName() ?? value.ToString();
        }

        public static string GetLocalizedEnum(this Enum value)
        {
            var name = GetDisplayName(value); 

            // Split by |
            var parts = name.Split('|');
            var english = parts[0];
            var german = parts.Length > 1 ? parts[1] : english;

            var lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLower();

            return lang switch
            {
                "de" => german,
                _ => english
            };
        }
    }

}
