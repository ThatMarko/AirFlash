using AirFlash.App.Ui;
using Xunit;

namespace AirFlash.Tests;

public sealed class PinInputTests
{
    [Theory]
    [InlineData("1234", "1234")]
    [InlineData("12345", "12345")]
    [InlineData("123456", "123456")]
    [InlineData("1234567", "1234567")]
    [InlineData("12345678", "12345678")]
    [InlineData("12-34", "1234")]
    [InlineData("-12--34-", "1234")]
    [InlineData("12-34-56-78", "12345678")]
    [InlineData("00-00", "0000")]
    public void CompleteAsciiInputProducesOnlyTheNormalizedDigits(string input, string expected)
    {
        Assert.True(PinInput.AcceptsText(input));
        Assert.Equal(expected, PinInput.Normalize(input));
        Assert.Equal(expected, PinInput.Normalize(expected));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("-")]
    [InlineData("----")]
    [InlineData("1")]
    [InlineData("12")]
    [InlineData("123")]
    [InlineData("1-2-3")]
    [InlineData("123456789")]
    [InlineData("12-34-56-789")]
    [InlineData(" 1234")]
    [InlineData("1234 ")]
    [InlineData("12 34")]
    [InlineData("12\t34")]
    [InlineData("1234\r\n")]
    [InlineData("12a34")]
    [InlineData("１２３４")]
    [InlineData("١٢٣٤")]
    [InlineData("१२३४")]
    [InlineData("12²34")]
    [InlineData("12\u200b34")]
    [InlineData("12\u00a034")]
    [InlineData("12–34")]
    [InlineData("12—34")]
    [InlineData("12−34")]
    [InlineData("+1234")]
    public void PartialOutOfRangeAndNonAsciiInputCannotSubmit(string? input)
        => Assert.Null(PinInput.Normalize(input));

    [Theory]
    [InlineData("", true)]
    [InlineData("0", true)]
    [InlineData("9", true)]
    [InlineData("-", true)]
    [InlineData("12-", true)]
    [InlineData("12-34", true)]
    [InlineData(null, false)]
    [InlineData(" ", false)]
    [InlineData("\n", false)]
    [InlineData("１２", false)]
    [InlineData("١٢", false)]
    [InlineData("–", false)]
    [InlineData("—", false)]
    [InlineData("−", false)]
    [InlineData("12a", false)]
    [InlineData("12-34\n", false)]
    public void TextFragmentsUseTheSameAsciiCharacterRule(string? fragment, bool accepted)
        => Assert.Equal(accepted, PinInput.AcceptsText(fragment));
}
