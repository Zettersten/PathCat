using System.Dynamic;
using System.Net;

namespace PathCat.Tests;

public class PathCatTests
{
    [Fact]
    public void BuildUrl_WithDictionary_ReturnsCorrectUrl()
    {
        var result = PathCat.BuildUrl("/:id", new Dictionary<string, object>
        {
            { "id", 123 },
            { "foo", "bar" }
        });

        Assert.Equal("/123?foo=bar", result);
    }

    [Fact]
    public void BuildUrl_WithAnonymousObject_ReturnsCorrectUrl()
    {
        var result = PathCat.BuildUrl("/users/:user_id/posts/:post_id", new
        {
            user_id = "123",
            post_id = 456,
            cool_flag = true,
            fields = new string[] { "title", "body" }
        });

        Assert.Equal("/users/123/posts/456?cool_flag=True&fields=title&fields=body", result);
    }

    [Fact]
    public void BuildUrl_WithClass_ReturnsCorrectUrl()
    {
        var result = PathCat.BuildUrl("/api/:version/resource/:id", new RequestParams
        {
            Version = "v1",
            Id = 789,
            Filter = "active",
            Nested = new NestedParams
            {
                SubFilter = "sub",
                Depth = 2
            }
        });

        Assert.Equal("/api/v1/resource/789?Filter=active&Nested.SubFilter=sub&Nested.Depth=2", result);
    }

    [Fact]
    public void BuildUrl_WithExpandoObject_ReturnsCorrectUrl()
    {
        dynamic expando = new ExpandoObject();
        expando.id = 123;
        expando.foo = "bar";

        var result = PathCat.BuildUrl("/:id", expando);

        Assert.Equal("/123?foo=bar", result);
    }

    [Fact]
    public void BuildUrl_WithNestedObject_ReturnsCorrectUrl()
    {
        var result = PathCat.BuildUrl("/api/:version/resource/:id", new
        {
            version = "v2",
            id = 789,
            nested = new
            {
                sub = "subValue",
                depth = 3
            }
        });

        Assert.Equal("/api/v2/resource/789?nested.sub=subValue&nested.depth=3", result);
    }

    [Fact]
    public void BuildUrl_WithEnum_ReturnsCorrectUrl()
    {
        var result = PathCat.BuildUrl("/status/:code", new
        {
            code = HttpStatusCode.OK
        });

        Assert.Equal("/status/OK", result);
    }

    [Fact]
    public void BuildUrl_WithNullParameters_ReturnsOriginalUrl()
    {
        var result = PathCat.BuildUrl("/api/resource", null);

        Assert.Equal("/api/resource", result);
    }

    [Fact]
    public void BuildUrl_WithListParameters_ReturnsCorrectUrl()
    {
        var result = PathCat.BuildUrl("/items", new
        {
            ids = new List<int> { 1, 2, 3 }
        });

        Assert.Equal("/items?ids=1&ids=2&ids=3", result);
    }

    [Fact]
    public void BuildUrl_WithMixedParameters_ReturnsCorrectUrl()
    {
        var result = PathCat.BuildUrl("/search", new
        {
            query = "test",
            page = 2,
            filters = new { category = "books", price = "cheap" }
        });

        Assert.Equal("/search?query=test&page=2&filters.category=books&filters.price=cheap", result);
    }

    [Fact]
    public void BuildUrl_WithInvalidUrl_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => PathCat.BuildUrl("not a valid url", null));
    }

    [Fact]
    public void BuildUrl_WithBooleanFormatLowerCase_ReturnsCorrectUrl()
    {
        var result = PathCat.BuildUrl("/api", new { flag = true }, new PathCatConfig
        {
            BooleanSerializationFormat = PathCatConfig.BooleanFormat.LowerCase
        });

        Assert.Equal("/api?flag=true", result);
    }

    [Fact]
    public void BuildUrl_WithBooleanFormatNumeric_ReturnsCorrectUrl()
    {
        var result = PathCat.BuildUrl("/api", new { flag = true }, new PathCatConfig
        {
            BooleanSerializationFormat = PathCatConfig.BooleanFormat.Numeric
        });

        Assert.Equal("/api?flag=1", result);
    }

    [Fact]
    public void BuildUrl_WithBooleanFormatOnOff_ReturnsCorrectUrl()
    {
        var result = PathCat.BuildUrl("/api", new { flag = true }, new PathCatConfig
        {
            BooleanSerializationFormat = PathCatConfig.BooleanFormat.OnOff
        });

        Assert.Equal("/api?flag=on", result);
    }

    [Fact]
    public void BuildUrl_WithPropertyNameFormatCamelCase_ReturnsCorrectUrl()
    {
        var result = PathCat.BuildUrl("/api", new { UserName = "John", IsAdmin = true }, new PathCatConfig
        {
            PropertyNameSerializationFormat = PathCatConfig.PropertyNameFormat.CamelCase
        });

        Assert.Equal("/api?userName=John&isAdmin=True", result);
    }

    [Fact]
    public void BuildUrl_WithPropertyNameFormatSnakeCase_ReturnsCorrectUrl()
    {
        var result = PathCat.BuildUrl("/api", new { UserName = "John", IsAdmin = true }, new PathCatConfig
        {
            PropertyNameSerializationFormat = PathCatConfig.PropertyNameFormat.SnakeCase
        });

        Assert.Equal("/api?user_name=John&is_admin=True", result);
    }

    [Fact]
    public void BuildUrl_WithObjectAccessorFormatIndexBrackets_ReturnsCorrectUrl()
    {
        var result = PathCat.BuildUrl("/api", new { User = new { Name = "John", Age = 30 } }, new PathCatConfig
        {
            ObjectAccessorSerializationFormat = PathCatConfig.ObjectAccessorFormat.IndexBrackets
        });

        Assert.Equal("/api?User[Name]=John&User[Age]=30", result);
    }

    [Fact]
    public void BuildUrl_WithObjectAccessorFormatOmitParent_ReturnsCorrectUrl()
    {
        var result = PathCat.BuildUrl("/api", new { User = new { Name = "John", Age = 30 } }, new PathCatConfig
        {
            ObjectAccessorSerializationFormat = PathCatConfig.ObjectAccessorFormat.OmitParent
        });

        Assert.Equal("/api?Name=John&Age=30", result);
    }

    [Fact]
    public void BuildUrl_WithArrayFormatIndexed_ReturnsCorrectUrl()
    {
        var result = PathCat.BuildUrl("/api", new { items = new[] { "a", "b", "c" } }, new PathCatConfig
        {
            ArraySerializationFormat = PathCatConfig.ArrayFormat.Indexed
        });

        Assert.Equal("/api?items[0]=a&items[1]=b&items[2]=c", result);
    }

    [Fact]
    public void BuildUrl_WithArrayFormatDelimited_ReturnsCorrectUrl()
    {
        var result = PathCat.BuildUrl("/api", new { items = new[] { "a", "b", "c" } }, new PathCatConfig
        {
            ArraySerializationFormat = PathCatConfig.ArrayFormat.Delimited,
            ArrayDelimiter = '|'
        });

        Assert.Equal("/api?items=a|b|c", result);
    }

    [Fact]
    public void BuildUrl_WithCombinedConfigurations_ReturnsCorrectUrl()
    {
        var result = PathCat.BuildUrl("/api", new
        {
            UserName = "John",
            IsAdmin = true,
            Preferences = new { Theme = "dark", ShowNotifications = true },
            Roles = new[] { "user", "editor" }
        }, new PathCatConfig
        {
            BooleanSerializationFormat = PathCatConfig.BooleanFormat.OnOff,
            PropertyNameSerializationFormat = PathCatConfig.PropertyNameFormat.CamelCase,
            ObjectAccessorSerializationFormat = PathCatConfig.ObjectAccessorFormat.IndexBrackets,
            ArraySerializationFormat = PathCatConfig.ArrayFormat.Delimited,
            ArrayDelimiter = ','
        });

        Assert.Equal("/api?userName=John&isAdmin=on&preferences[theme]=dark&preferences[showNotifications]=on&roles=user,editor", result);
    }

    private class RequestParams
    {
        public string? Version { get; set; }
        public int Id { get; set; }
        public string? Filter { get; set; }
        public NestedParams? Nested { get; set; }
    }

    private class NestedParams
    {
        public string? SubFilter { get; set; }
        public int Depth { get; set; }
    }
}