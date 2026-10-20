using System.Text;

namespace AirFlash.App.Ui;

internal static class PasswordInput
{
    private static readonly Encoding StrictUtf8 = new UTF8Encoding(false, true);

    // Match the native password contract without trimming or rewriting secrets.
    // The PasswordBox's separate editing limit remains 128 UTF-16 code units.
    public static bool IsValid(string? password)
    {
        if (string.IsNullOrEmpty(password) || password.IndexOfAny(['\r', '\n', '\0']) >= 0) return false;
        try { return StrictUtf8.GetByteCount(password) <= 256; }
        catch (EncoderFallbackException) { return false; }
    }
}
