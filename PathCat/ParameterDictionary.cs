using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;

namespace PathCat;

/// <summary>
/// Provides utility methods for converting objects to parameter dictionaries.
/// </summary>
internal static class ParameterDictionary
{
    private static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new();

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
        else if (config.UseSystemTextJsonSerialization)
        {
            FillDictionaryUsingSystemTextJson(result, parameters, config);
        }
        else
        {
            FillDictionary(result, string.Empty, parameters, config);
        }

        return result;
    }

    [RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializer.SerializeToElement<TValue>(TValue, JsonSerializerOptions)")]
    [RequiresDynamicCode("Calls System.Text.Json.JsonSerializer.SerializeToElement<TValue>(TValue, JsonSerializerOptions)")]
    private static void FillDictionaryUsingSystemTextJson(Dictionary<string, object> dict, object parameters, PathCatConfig config)
    {
        var options = config.SystemTextJsonOptions ?? DefaultJsonSerializerOptions;
        var jsonElement = JsonSerializer.SerializeToElement(parameters, options);

        if (jsonElement.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in jsonElement.EnumerateObject())
            {
                var value = GetValueFromJsonElement(property.Value);

                if (value is null)
                {
                    continue;
                }

                dict[property.Name] = value;
            }
        }
    }

    private static object? GetValueFromJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? string.Empty,
            JsonValueKind.Number => element.TryGetInt64(out long longValue) ? longValue : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            JsonValueKind.Array => element.EnumerateArray().Select(GetValueFromJsonElement).ToList(),
            JsonValueKind.Object => element.EnumerateObject().ToDictionary(p => p.Name, p => GetValueFromJsonElement(p.Value)),
            _ => element.ToString()
        };
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