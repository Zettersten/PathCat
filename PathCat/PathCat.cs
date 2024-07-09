using System.Collections;
using System.Runtime.CompilerServices;

namespace PathCat;

/// <summary>
/// Provides URL building functionality with customizable parameter handling.
/// </summary>
public static class PathCat
{
    private const int BufferSize = 2048;
    private const char QuestionMark = '?';
    private const char Ampersand = '&';
    private const char EqualsSign = '=';
    private const char Colon = ':';

    [ThreadStatic]
    private static char[] buffer;

    static PathCat()
    {
        buffer = new char[BufferSize];
    }

    /// <summary>
    /// Builds a URL by combining the base URL, path, and parameters.
    /// </summary>
    /// <param name="pathOrUrl">The absolute or relative path with placeholders for parameters.</param>
    /// <param name="parameters">The parameters to replace in the path and append as query parameters.</param>
    /// <param name="config">Configuration options for PathCat.</param>
    /// <returns>The combined URL.</returns>
    /// <exception cref="ArgumentException">Thrown when the path or URL is not well-formed.</exception>
    public static string BuildUrl(string pathOrUrl, object? parameters = null, PathCatConfig? config = null)
    {
        if (!Uri.IsWellFormedUriString(pathOrUrl, UriKind.RelativeOrAbsolute))
        {
            throw new ArgumentException("The path or URL is not well-formed.", nameof(pathOrUrl));
        }

        int bufferIndex = 0;
        var pathSpan = pathOrUrl.AsSpan();
        var conf = config ?? new PathCatConfig();

        AppendToBuffer(ref bufferIndex, pathSpan);

        if (parameters != null)
        {
            var parameterDict = ParameterDictionary.GetParameterDictionary(parameters, conf);
            ReplacePlaceholders(ref bufferIndex, parameterDict);
            AppendQueryParameters(ref bufferIndex, parameterDict, conf);
        }

        return new string(buffer, 0, bufferIndex);
    }

    private static void ReplacePlaceholders(ref int bufferIndex, Dictionary<string, object> parameterDict)
    {
        for (int i = 0; i < bufferIndex; i++)
        {
            if (buffer[i] == Colon)
            {
                int start = i;
                while (++i < bufferIndex && (char.IsLetterOrDigit(buffer[i]) || buffer[i] == '_')) { }
                var paramName = new string(buffer, start + 1, i - start - 1);

                if (parameterDict.TryGetValue(paramName, out var value))
                {
                    var valueStr = value.ToString();

                    if (string.IsNullOrEmpty(valueStr))
                    {
                        continue;
                    }

                    ReplaceInBuffer(ref bufferIndex, start, i, valueStr.AsSpan());

                    i = start + valueStr.Length - 1;

                    parameterDict.Remove(paramName);
                }
            }
        }
    }

    private static void AppendQueryParameters(ref int bufferIndex, Dictionary<string, object> parameterDict, PathCatConfig config)
    {
        var hasExistingParams = false;

        foreach (var param in parameterDict)
        {
            if (param.Value is IEnumerable enumerable && param.Value is not string)
            {
                switch (config.ArraySerializationFormat)
                {
                    case PathCatConfig.ArrayFormat.Delimited:
                        var values = new List<string>();
                        foreach (var item in enumerable)
                        {
                            values.Add(item?.ToString() ?? string.Empty);
                        }
                        var delimitedValue = string.Join(config.ArrayDelimiter.ToString(), values);
                        AppendQueryParam(ref bufferIndex, param.Key, delimitedValue, ref hasExistingParams);
                        break;

                    case PathCatConfig.ArrayFormat.Indexed:
                        int index = 0;
                        foreach (var item in enumerable)
                        {
                            AppendQueryParam(ref bufferIndex, $"{param.Key}[{index}]", item?.ToString() ?? string.Empty, ref hasExistingParams);
                            index++;
                        }
                        break;

                    default: // Default behavior (repeat the key for each value)
                        foreach (var item in enumerable)
                        {
                            AppendQueryParam(ref bufferIndex, param.Key, item?.ToString() ?? string.Empty, ref hasExistingParams);
                        }
                        break;
                }
            }
            else if (param.Value is bool boolValue)
            {
                AppendQueryParam(ref bufferIndex, param.Key, SerializeBoolean(boolValue, config), ref hasExistingParams);
            }
            else
            {
                AppendQueryParam(ref bufferIndex, param.Key, param.Value?.ToString() ?? string.Empty, ref hasExistingParams);
            }
        }
    }

    private static void AppendQueryParam(ref int bufferIndex, string key, string value, ref bool hasExistingParams)
    {
        if (hasExistingParams)
        {
            buffer[bufferIndex++] = Ampersand;
        }
        else
        {
            buffer[bufferIndex++] = QuestionMark;
            hasExistingParams = true;
        }

        AppendToBuffer(ref bufferIndex, key.AsSpan());

        buffer[bufferIndex++] = EqualsSign;

        AppendToBuffer(ref bufferIndex, value.AsSpan());
    }

    private static void ReplaceInBuffer(ref int bufferIndex, int start, int end, ReadOnlySpan<char> newValue)
    {
        var oldLength = end - start;
        var newLength = newValue.Length;
        var diff = newLength - oldLength;

        if (diff != 0)
        {
            buffer
                .AsSpan(end, bufferIndex - end)
                .CopyTo(buffer.AsSpan(start + newLength));
        }

        newValue
            .CopyTo(buffer.AsSpan(start));

        bufferIndex += diff;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AppendToBuffer(ref int bufferIndex, ReadOnlySpan<char> value)
    {
        if (bufferIndex + value.Length > BufferSize)
        {
            throw new InvalidOperationException("Buffer overflow");
        }

        value
            .CopyTo(buffer.AsSpan(bufferIndex));

        bufferIndex += value.Length;
    }

    private static string SerializeBoolean(bool value, PathCatConfig config) => config.BooleanSerializationFormat switch
    {
        PathCatConfig.BooleanFormat.LowerCase => value ? "true" : "false",
        PathCatConfig.BooleanFormat.Numeric => value ? "1" : "0",
        PathCatConfig.BooleanFormat.OnOff => value ? "on" : "off",
        _ => value.ToString()
    };
}