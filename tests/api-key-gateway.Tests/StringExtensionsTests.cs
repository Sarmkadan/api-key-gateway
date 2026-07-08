// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Xunit;
using ApiKeyGateway.Extensions;
using FluentAssertions;

namespace ApiKeyGateway.Tests;

public class StringExtensionsTests
{
    // -------------------------------------------------------------------------
    // Truncate
    // -------------------------------------------------------------------------

    [Fact]
    public void Truncate_NullString_ReturnsNull()
    {
        string? value = null;
        value!.Truncate(10).Should().BeNull();
    }

    [Fact]
    public void Truncate_EmptyString_ReturnsEmpty()
    {
        "".Truncate(5).Should().BeEmpty();
    }

    [Fact]
    public void Truncate_ShorterThanMax_ReturnsOriginal()
    {
        "abc".Truncate(10).Should().Be("abc");
    }

    [Fact]
    public void Truncate_ExactlyMaxLength_ReturnsOriginal()
    {
        "abcde".Truncate(5).Should().Be("abcde");
    }

    [Fact]
    public void Truncate_LongerThanMax_TruncatesToMaxLength()
    {
        "abcdefghij".Truncate(5).Should().Be("abcde");
    }

    [Fact]
    public void Truncate_ZeroMaxLength_ReturnsEmptyString()
    {
        "anything".Truncate(0).Should().BeEmpty();
    }

    // -------------------------------------------------------------------------
    // TruncateWithEllipsis
    // -------------------------------------------------------------------------

    [Fact]
    public void TruncateWithEllipsis_NullString_ReturnsNull()
    {
        string? value = null;
        value!.TruncateWithEllipsis(10).Should().BeNull();
    }

    [Fact]
    public void TruncateWithEllipsis_ShortString_ReturnsOriginal()
    {
        "hi".TruncateWithEllipsis(10).Should().Be("hi");
    }

    [Fact]
    public void TruncateWithEllipsis_LongString_TruncatesWithEllipsis()
    {
        "Hello World!".TruncateWithEllipsis(8).Should().Be("Hello...");
    }

    [Fact]
    public void TruncateWithEllipsis_ExactMaxLength_ReturnsOriginal()
    {
        "12345".TruncateWithEllipsis(5).Should().Be("12345");
    }

    // -------------------------------------------------------------------------
    // ContainsAny
    // -------------------------------------------------------------------------

    [Fact]
    public void ContainsAny_MatchExists_ReturnsTrue()
    {
        "Hello World".ContainsAny("world", "missing").Should().BeTrue();
    }

    [Fact]
    public void ContainsAny_NoMatch_ReturnsFalse()
    {
        "Hello World".ContainsAny("xyz", "abc").Should().BeFalse();
    }

    [Fact]
    public void ContainsAny_EmptySearchArray_ReturnsFalse()
    {
        "Hello".ContainsAny().Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // StartsWithAny
    // -------------------------------------------------------------------------

    [Fact]
    public void StartsWithAny_MatchingPrefix_ReturnsTrue()
    {
        "sk_abc123".StartsWithAny("pk_", "sk_").Should().BeTrue();
    }

    [Fact]
    public void StartsWithAny_NoMatchingPrefix_ReturnsFalse()
    {
        "sk_abc123".StartsWithAny("pk_", "rk_").Should().BeFalse();
    }

    [Fact]
    public void StartsWithAny_CaseInsensitive_ReturnsTrue()
    {
        "SK_ABC".StartsWithAny("sk_").Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // ToSlug
    // -------------------------------------------------------------------------

    [Fact]
    public void ToSlug_NullInput_ReturnsEmpty()
    {
        string? value = null;
        value!.ToSlug().Should().BeEmpty();
    }

    [Fact]
    public void ToSlug_EmptyInput_ReturnsEmpty()
    {
        "".ToSlug().Should().BeEmpty();
    }

    [Fact]
    public void ToSlug_SpacesConvertedToDashes()
    {
        "Hello World Test".ToSlug().Should().Be("hello-world-test");
    }

    [Fact]
    public void ToSlug_SpecialCharsRemoved()
    {
        "Hello@World#2026!".ToSlug().Should().Be("helloworld2026");
    }

    [Fact]
    public void ToSlug_ConsecutiveDashesCollapsed()
    {
        "Hello   World".ToSlug().Should().Be("hello-world");
    }

    [Fact]
    public void ToSlug_LeadingTrailingDashesRemoved()
    {
        " Hello World ".ToSlug().Should().Be("hello-world");
    }

    [Fact]
    public void ToSlug_UnderscoresPreserved()
    {
        "my_api_key".ToSlug().Should().Be("my_api_key");
    }

    // -------------------------------------------------------------------------
    // CapitalizeFirst
    // -------------------------------------------------------------------------

    [Fact]
    public void CapitalizeFirst_NullInput_ReturnsNull()
    {
        string? value = null;
        value!.CapitalizeFirst().Should().BeNull();
    }

    [Fact]
    public void CapitalizeFirst_EmptyInput_ReturnsEmpty()
    {
        "".CapitalizeFirst().Should().BeEmpty();
    }

    [Fact]
    public void CapitalizeFirst_LowercaseFirstChar_Capitalizes()
    {
        "hello".CapitalizeFirst().Should().Be("Hello");
    }

    [Fact]
    public void CapitalizeFirst_AlreadyCapitalized_NoChange()
    {
        "Hello".CapitalizeFirst().Should().Be("Hello");
    }

    [Fact]
    public void CapitalizeFirst_SingleChar_Capitalizes()
    {
        "h".CapitalizeFirst().Should().Be("H");
    }

    // -------------------------------------------------------------------------
    // ToList (delimiter split)
    // -------------------------------------------------------------------------

    [Fact]
    public void ToList_NullString_ReturnsEmptyList()
    {
        string? value = null;
        value!.ToList().Should().BeEmpty();
    }

    [Fact]
    public void ToList_EmptyString_ReturnsEmptyList()
    {
        "".ToList().Should().BeEmpty();
    }

    [Fact]
    public void ToList_CommaSeparated_SplitsAndTrims()
    {
        " a , b , c ".ToList().Should().BeEquivalentTo(new[] { "a", "b", "c" });
    }

    [Fact]
    public void ToList_CustomDelimiter_SplitsCorrectly()
    {
        "one|two|three".ToList('|').Should().BeEquivalentTo(new[] { "one", "two", "three" });
    }

    // -------------------------------------------------------------------------
    // IsNumeric
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("12345", true)]
    [InlineData("0", true)]
    [InlineData("", false)]
    [InlineData("12.5", false)]
    [InlineData("abc", false)]
    [InlineData("-1", false)]
    public void IsNumeric_VariousInputs_ReturnsExpected(string input, bool expected)
    {
        input.IsNumeric().Should().Be(expected);
    }

    // -------------------------------------------------------------------------
    // TryParseInt / TryParseLong
    // -------------------------------------------------------------------------

    [Fact]
    public void TryParseInt_ValidInteger_ReturnsParsedValue()
    {
        "42".TryParseInt().Should().Be(42);
    }

    [Fact]
    public void TryParseInt_NonNumeric_ReturnsNull()
    {
        "abc".TryParseInt().Should().BeNull();
    }

    [Fact]
    public void TryParseInt_Overflow_ReturnsNull()
    {
        "99999999999999999".TryParseInt().Should().BeNull();
    }

    [Fact]
    public void TryParseLong_ValidLong_ReturnsParsedValue()
    {
        "9999999999".TryParseLong().Should().Be(9999999999L);
    }

    [Fact]
    public void TryParseLong_NonNumeric_ReturnsNull()
    {
        "xyz".TryParseLong().Should().BeNull();
    }
}
