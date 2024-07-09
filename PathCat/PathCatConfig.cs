using System.Text.Json;

namespace PathCat;

/// <summary>
/// Configuration options for PathCat.
/// </summary>
public sealed class PathCatConfig
{
    /// <summary>
    /// Gets or sets whether to use System.Text.Json for property serialization.
    /// </summary>
    public bool UseSystemTextJsonSerialization { get; set; } = false;

    /// <summary>
    /// Gets or sets the System.Text.Json serialization options.
    /// </summary>
    public JsonSerializerOptions? SystemTextJsonOptions { get; set; }

    /// <summary>
    /// Defines the format for boolean value serialization.
    /// </summary>
    public enum BooleanFormat
    {
        /// <summary>
        /// Use the default .NET boolean string representation.
        /// </summary>
        Default,

        /// <summary>
        /// Use lowercase "true" or "false".
        /// </summary>
        LowerCase,

        /// <summary>
        /// Use "1" for true and "0" for false.
        /// </summary>
        Numeric,

        /// <summary>
        /// Use "on" for true and "off" for false.
        /// </summary>
        OnOff
    }

    /// <summary>
    /// Defines the format for array serialization.
    /// </summary>
    public enum ArrayFormat
    {
        /// <summary>
        /// Use the default array serialization.
        /// </summary>
        Default,

        /// <summary>
        /// Use indexed notation for array elements.
        /// </summary>
        Indexed,

        /// <summary>
        /// Use a delimiter to separate array elements.
        /// </summary>
        Delimited
    }

    /// <summary>
    /// Defines the format for property name serialization.
    /// </summary>
    public enum PropertyNameFormat
    {
        /// <summary>
        /// Use the default property name format.
        /// </summary>
        Default,

        /// <summary>
        /// Use camelCase for property names.
        /// </summary>
        CamelCase,

        /// <summary>
        /// Use snake_case for property names.
        /// </summary>
        SnakeCase
    }

    /// <summary>
    /// Defines the format for object accessor serialization.
    /// </summary>
    public enum ObjectAccessorFormat
    {
        /// <summary>
        /// Use dot notation for nested objects.
        /// </summary>
        DotNotation,

        /// <summary>
        /// Use index brackets for nested objects.
        /// </summary>
        IndexBrackets,

        /// <summary>
        /// Omit parent object names in nested object serialization.
        /// </summary>
        OmitParent
    }

    /// <summary>
    /// Gets or sets the boolean serialization format.
    /// </summary>
    public BooleanFormat BooleanSerializationFormat { get; set; } = BooleanFormat.Default;

    /// <summary>
    /// Gets or sets the array serialization format.
    /// </summary>
    public ArrayFormat ArraySerializationFormat { get; set; } = ArrayFormat.Default;

    /// <summary>
    /// Gets or sets the delimiter used for array serialization when using ArrayFormat.Delimited.
    /// </summary>
    public char ArrayDelimiter { get; set; } = ',';

    /// <summary>
    /// Gets or sets the property name serialization format.
    /// </summary>
    public PropertyNameFormat PropertyNameSerializationFormat { get; set; } = PropertyNameFormat.Default;

    /// <summary>
    /// Gets or sets the object accessor serialization format.
    /// </summary>
    public ObjectAccessorFormat ObjectAccessorSerializationFormat { get; set; } = ObjectAccessorFormat.DotNotation;
}