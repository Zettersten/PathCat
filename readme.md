<img src="https://raw.githubusercontent.com/Zettersten/PathCat/main/icon.png" alt="PathCat Icon" width="100" height="100">


# PathCat 🐾

[![NuGet Badge](https://buildstats.info/nuget/PathCat)](https://www.nuget.org/packages/PathCat/)

PathCat is a powerful and flexible URL building library for .NET, inspired by the popular JavaScript library [pathcat](https://github.com/alii/pathcat). It provides an intuitive way to construct URLs with dynamic parameters, offering extensive configuration options to suit various serialization needs.

## Features

- Simple and expressive API for URL construction
- Support for path placeholders and query parameters
- Flexible parameter handling (dictionaries, anonymous objects, strongly-typed classes)
- Customizable serialization formats for various data types
- Efficient string manipulation using `Span<T>` and `ReadOnlySpan<T>`

## Installation

Install PathCat via NuGet:

```
dotnet add package PathCat
```

## Quick Start

```csharp
using PathCat;

// Basic usage
string url = PathCat.BuildUrl("/users/:id", new { id = 123, filter = "active" });
// Result: "/users/123?filter=active"

// With nested objects
string url = PathCat.BuildUrl("/api/:version/resource/:id", new
{
    version = "v2",
    id = 789,
    options = new { sort = "asc", limit = 10 }
});
// Result: "/api/v2/resource/789?options.sort=asc&options.limit=10"
```

## Advanced Usage

### Configuration Options

PathCat offers various configuration options to customize its behavior:

```csharp
var config = new PathCatConfig
{
    BooleanSerializationFormat = PathCatConfig.BooleanFormat.LowerCase,
    ArraySerializationFormat = PathCatConfig.ArrayFormat.Indexed,
    PropertyNameSerializationFormat = PathCatConfig.PropertyNameFormat.CamelCase,
    ObjectAccessorSerializationFormat = PathCatConfig.ObjectAccessorFormat.IndexBrackets,
    ArrayDelimiter = '|'
};

string url = PathCat.BuildUrl("/api", parameters, config);
```

#### Boolean Serialization

- `Default`: Uses .NET's default boolean representation
- `LowerCase`: Uses "true" or "false"
- `Numeric`: Uses "1" or "0"
- `OnOff`: Uses "on" or "off"

#### Array Serialization

- `Default`: Repeats the key for each array element
- `Indexed`: Uses indexed notation (e.g., `items[0]=value`)
- `Delimited`: Joins array elements with a specified delimiter

#### Property Name Serialization

- `Default`: Uses the original property names
- `CamelCase`: Converts property names to camelCase
- `SnakeCase`: Converts property names to snake_case

#### Object Accessor Serialization

- `DotNotation`: Uses dot notation for nested objects
- `IndexBrackets`: Uses bracket notation for nested objects
- `OmitParent`: Flattens nested object structure

### Examples

1. Using different boolean formats:

```csharp
var config = new PathCatConfig
{
    BooleanSerializationFormat = PathCatConfig.BooleanFormat.Numeric
};
string url = PathCat.BuildUrl("/api", new { enabled = true }, config);
// Result: "/api?enabled=1"
```

2. Customizing array serialization:

```csharp
var config = new PathCatConfig
{
    ArraySerializationFormat = PathCatConfig.ArrayFormat.Delimited,
    ArrayDelimiter = '|'
};
string url = PathCat.BuildUrl("/api", new { tags = new[] { "urgent", "important" } }, config);
// Result: "/api?tags=urgent|important"
```

3. Using different property name formats:

```csharp
var config = new PathCatConfig
{
    PropertyNameSerializationFormat = PathCatConfig.PropertyNameFormat.SnakeCase
};
string url = PathCat.BuildUrl("/api", new { FirstName = "John", LastName = "Doe" }, config);
// Result: "/api?first_name=John&last_name=Doe"
```

4. Customizing object accessor format:

```csharp
var config = new PathCatConfig
{
    ObjectAccessorSerializationFormat = PathCatConfig.ObjectAccessorFormat.IndexBrackets
};
string url = PathCat.BuildUrl("/api", new { user = new { name = "Alice", age = 30 } }, config);
// Result: "/api?user[name]=Alice&user[age]=30"
```

### System.Text.Json Integration

PathCat now supports System.Text.Json serialization settings for property names. This feature allows you to leverage existing System.Text.Json configurations in your URL building process.

To use this feature:

```csharp
var config = new PathCatConfig
{
    UseSystemTextJsonSerialization = true,
    SystemTextJsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    }
};

var parameters = new
{
    UserName = "JohnDoe",
    IsActive = true,
    LastLoginDate = DateTime.Now,
    UserType = UserType.Admin
};

string url = PathCat.BuildUrl("/api/users", parameters, config);
// Result: "/api/users?userName=JohnDoe&isActive=true&lastLoginDate=2023-05-15T10:30:00&userType=Admin"
```

## Performance

PathCat is designed with performance in mind, utilizing `Span<T>` and `ReadOnlySpan<T>` for efficient string manipulation. It employs a pre-allocated buffer to minimize memory allocations during URL construction.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License.

---

PathCat is a partial port of and inspired by the [pathcat](https://github.com/alii/pathcat) JavaScript library. While maintaining the core concepts, it has been adapted and enhanced for the .NET ecosystem.