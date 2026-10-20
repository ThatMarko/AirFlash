namespace AirFlash.App.Ui;

internal static class PinInput
{
    public static bool AcceptsText(string? text) => text is not null && text.All(IsAllowed);

    public static string? Normalize(string? text)
    {
        if (!AcceptsText(text)) return null;
        var digits = text!.Replace("-", "", StringComparison.Ordinal);
        return digits.Length is >= 4 and <= 8 ? digits : null;
    }

    private static bool IsAllowed(char character) => char.IsAsciiDigit(character) || character == '-';
}
