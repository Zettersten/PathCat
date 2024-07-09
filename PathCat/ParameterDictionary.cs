using System.Collections;
using System.Reflection;

namespace PathCat;

/// <summary>
/// Provides utility methods for converting objects to parameter dictionaries.
/// </summary>
internal static class ParameterDictionary
{
    /// <summary>
    /// Converts an object to a dictionary of parameters.
    /// </summary>
    /// <param name="parameters">The object to convert.</param>
    /// <param name="config">The PathCat configuration.</param>
    /// <returns>A dictionary of parameter key-value pairs.</returns>
    public static Dictionary<string, object> GetParameterDictionary(object parameters, PathCatConfig config)
    {
        var result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        if (parameters is IDictionary<string, object> dict)
        {
            foreach (var kvp in dict)
            {
                result[kvp.Key] = kvp.Value;
            }
        }
        else
        {
            FillDictionary(result, string.Empty, parameters, config);
        }

        return result;
    }

    private static void FillDictionary(Dictionary<string, object> dict, string prefix, object obj, PathCatConfig config)
    {
        if (obj == null) return;

        if (obj is IEnumerable enumerable && obj is not string)
        {
            HandleEnumerable(dict, prefix, enumerable);
            return;
        }

        foreach (var prop in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var value = prop.GetValue(obj);
            if (value == null) continue;

            var propName = FormatPropertyName(prop.Name, config.PropertyNameSerializationFormat);
            var key = string.IsNullOrEmpty(prefix) ? propName : CombineKeys(prefix, propName, config.ObjectAccessorSerializationFormat);

            if (IsSimpleType(value.GetType()))
            {
                dict[key] = value;
            }
            else if (value is IEnumerable e && value is not string)
            {
                HandleEnumerable(dict, key, e);
            }
            else
            {
                FillDictionary(dict, key, value, config);
            }
        }
    }

    private static void HandleEnumerable(Dictionary<string, object> dict, string key, IEnumerable enumerable)
    {
        var list = new List<object>();
        foreach (var item in enumerable)
        {
            list.Add(item);
        }
        dict[key] = list;
    }

    private static string FormatPropertyName(string name, PathCatConfig.PropertyNameFormat format) => format switch
    {
        PathCatConfig.PropertyNameFormat.CamelCase => char.ToLowerInvariant(name[0]) + name[1..],
        PathCatConfig.PropertyNameFormat.SnakeCase => string.Concat(name.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToLower(),
        _ => name
    };

    private static string CombineKeys(string prefix, string key, PathCatConfig.ObjectAccessorFormat format) => format switch
    {
        PathCatConfig.ObjectAccessorFormat.IndexBrackets => $"{prefix}[{key}]",
        PathCatConfig.ObjectAccessorFormat.OmitParent => key,
        _ => $"{prefix}.{key}"
    };

    private static bool IsSimpleType(Type type) =>
        type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal) || type == typeof(DateTime);
}