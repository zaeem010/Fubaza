using Microsoft.Extensions.Localization;

namespace Fubaza.API.Extensions
{
    public static class StringExtensions
    {
        public static string GetCulturedString<T>(
        this List<string> keys,
        IStringLocalizer<T> localizer,
        object[]? arguments = null)
        {
            if (keys == null || !keys.Any())
                return string.Empty;

            var messages = keys.Select(key =>
            {
                return arguments == null || arguments.Length == 0
                    ? localizer[key].Value
                    : localizer[key, arguments].Value;
            });

            return string.Join(", ", messages);
        }

        public static List<string> GetCulturedList<T>(
        this List<string>? keys,
        IStringLocalizer<T> localizer,
        object[]? arguments = null)
        {
            if (keys == null || !keys.Any())
                return new List<string>();

            return keys.Select(key =>
                (arguments == null || arguments.Length == 0)
                    ? localizer[key].Value
                    : localizer[key, arguments].Value
            ).ToList();
        }

        public static string ErrorsToString(this List<string> args)
        {
            return args.Aggregate(string.Empty, (current, arg) => current + (arg + ","));
        }
    }
}
