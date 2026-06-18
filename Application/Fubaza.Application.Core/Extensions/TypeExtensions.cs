using System.ComponentModel;
using System.Reflection;

public static class TypeExtensions
{
    public static List<T> GetAllPublicConstantValues<T>(this Type type)
    {
        return type
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(T))
            .Select(fi => (T)(fi.GetRawConstantValue() ?? throw new InvalidOperationException("Constant value is null.")))
            .ToList();
    }

    public static List<(string Value, string Group, string Description)>
    GetPermissionsWithGroup(this Type type)
    {
        var list = new List<(string Value, string Group, string Description)>();

        // Get nested permission groups
        var nestedTypes = type.GetNestedTypes(BindingFlags.Public | BindingFlags.Static);

        foreach (var nested in nestedTypes)
        {
            // Group name from DisplayName attribute or class name
            var group = nested
                .GetCustomAttribute<DisplayNameAttribute>()?
                .DisplayName
                ?? nested.Name;

            // Description from Description attribute
            var description = nested
                .GetCustomAttribute<DescriptionAttribute>()?
                .Description
                ?? string.Empty;

            // Constant fields in the nested Permission class
            var fields = nested.GetFields(BindingFlags.Public | BindingFlags.Static);

            foreach (var field in fields)
            {
                if (field.GetValue(null) is string value)
                {
                    list.Add((value, group, description));
                }
            }
        }

        return list;
    }

}
