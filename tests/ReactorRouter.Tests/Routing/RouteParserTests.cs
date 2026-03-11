namespace ReactorRouter.Tests.Routing;

public class RouteParserTests
{
    [Fact]
    public void Parse_SimpleSegments()
    {
        var (segments, query) = RouteParser.Parse("/dashboard/settings");
        segments.Should().Equal("dashboard", "settings");
        query.Contains("theme").Should().BeFalse();
    }

    [Fact]
    public void Parse_WithQueryString()
    {
        var (segments, query) = RouteParser.Parse("/users/123?tab=posts&sort=date");
        segments.Should().Equal("users", "123");
        query.GetOrDefault("tab").Should().Be("posts");
        query.GetOrDefault("sort").Should().Be("date");
    }

    [Fact]
    public void Parse_RootPath()
    {
        var (segments, query) = RouteParser.Parse("/");
        segments.Should().BeEmpty();
        query.Contains("x").Should().BeFalse();
    }

    [Fact]
    public void Parse_QueryOnly()
    {
        var (_, query) = RouteParser.Parse("/?foo=bar");
        query.GetOrDefault("foo").Should().Be("bar");
    }

    [Fact]
    public void TryMatchSegment_Static()
    {
        RouteParser.TryMatchSegment("dashboard", "dashboard", out var pName, out var pVal)
            .Should().BeTrue();
        pName.Should().BeNull();
        pVal.Should().BeNull();
    }

    [Fact]
    public void TryMatchSegment_Static_CaseInsensitive()
    {
        RouteParser.TryMatchSegment("Dashboard", "dashboard", out _, out _)
            .Should().BeTrue();
    }

    [Fact]
    public void TryMatchSegment_Static_Mismatch()
    {
        RouteParser.TryMatchSegment("settings", "profile", out _, out _)
            .Should().BeFalse();
    }

    [Fact]
    public void TryMatchSegment_Dynamic()
    {
        RouteParser.TryMatchSegment(":userId", "42", out var pName, out var pVal)
            .Should().BeTrue();
        pName.Should().Be("userId");
        pVal.Should().Be("42");
    }

    [Fact]
    public void TryMatchSegment_Wildcard()
    {
        RouteParser.TryMatchSegment("*", "anything", out var pName, out var pVal)
            .Should().BeTrue();
        pName.Should().BeNull();
        pVal.Should().BeNull();
    }

    [Theory]
    [InlineData("dashboard", 3)]
    [InlineData(":id", 2)]
    [InlineData("*", 1)]
    public void ScoreSegment(string pattern, int expected)
    {
        RouteParser.ScoreSegment(pattern).Should().Be(expected);
    }
}
