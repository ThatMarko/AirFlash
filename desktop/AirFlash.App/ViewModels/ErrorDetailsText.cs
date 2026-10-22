using AirFlash.Core;

namespace AirFlash.App.ViewModels;

internal static class ErrorDetailsText
{
    public static string For(SessionSnapshot snapshot, string notice)
    {
        // Retry publishes Connecting immediately after Error. Keep its cause
        // accessible without replacing an independent discovery/settings notice.
        var fault = snapshot.State switch
        {
            PlaybackState.Error when !string.IsNullOrWhiteSpace(snapshot.Message) => snapshot.Message,
            PlaybackState.Error or PlaybackState.Connecting => snapshot.Diagnostics?.LastFault?.Detail ?? "",
            _ => ""
        };
        if (string.IsNullOrWhiteSpace(fault)) return notice;
        if (string.IsNullOrWhiteSpace(notice) || notice == fault) return fault;
        return fault + Environment.NewLine + Environment.NewLine + notice;
    }
}
