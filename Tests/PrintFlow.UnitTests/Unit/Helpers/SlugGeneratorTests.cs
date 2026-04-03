using PrintFlow.Application.Services;

namespace PrintFlow.UnitTests.Unit.Helpers;

public class SlugGeneratorTests
{
    [Theory]
    [InlineData("Business Cards", "business-cards")]
    [InlineData("Banners & Signs", "banners-signs")]
    [InlineData("T-Shirts & Apparel", "t-shirts-apparel")]
    [InlineData("Stickers & Labels", "stickers-labels")]
    [InlineData("  Spaced Out  ", "spaced-out")]
    [InlineData("UPPERCASE NAME", "uppercase-name")]
    [InlineData("Special!@#Characters$%", "specialcharacters")]
    [InlineData("Multiple   Spaces   Here", "multiple-spaces-here")]
    public void Generate_ReturnsExpectedSlug(string input, string expected)
    {
        var result = SlugGenerator.Generate(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Generate_EmptyString_ReturnsEmpty()
    {
        var result = SlugGenerator.Generate("");
        Assert.Equal("", result);
    }

    [Fact]
    public void Generate_ReturnsLowercase()
    {
        var result = SlugGenerator.Generate("HELLO WORLD");
        Assert.Equal(result, result.ToLowerInvariant());
    }

    [Fact]
    public void Generate_NoLeadingOrTrailingDashes()
    {
        var result = SlugGenerator.Generate("---test---");
        Assert.False(result.StartsWith('-'));
        Assert.False(result.EndsWith('-'));
    }
}