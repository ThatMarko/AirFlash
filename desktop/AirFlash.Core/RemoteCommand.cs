namespace AirFlash.Core;

/// <summary>
/// A command the receiver sent back over the event channel: the speaker's own
/// buttons, or a remote that talks to it.
/// </summary>
/// <remarks>
/// The engine already answers the acknowledgement, so nothing here has to reply.
/// AirFlash acts on transport commands in the media app the user is playing from:
/// <c>play</c> and <c>pause</c> go to the current SMTC session (there is no
/// discrete media key for either), and <c>toggle</c>, <c>stop</c>, <c>next</c> and
/// <c>previous</c> are synthesised as media keys, as the reference sender does
/// instead of reaching for the MediaRemote COM API.
/// <para>
/// <c>mute</c>, <c>volume_up</c>, <c>volume_down</c>, <c>shuffle_next</c>,
/// <c>repeat_next</c> and the absolute <see cref="Volume"/> are deliberately not
/// acted on (design note Q25). A receiver's own volume buttons already change the
/// receiver's volume, which the engine reads back from <c>/info</c> and reports as
/// <c>device_volume</c>; applying the command again here would change it twice.
/// Windows has no media key for shuffle or repeat. These commands are still parsed
/// and logged for diagnostics.
/// </para>
/// </remarks>
public sealed record RemoteCommand(string Name, double? Volume = null, string? Host = null)
{
    /// <summary>Virtual-key code for <c>SendInput</c>, or null when the command is not
    /// sent as a media key: <c>play</c>/<c>pause</c> use SMTC instead, and volume,
    /// mute, shuffle and repeat are intentionally ignored (see the remarks).</summary>
    public int? VirtualKey => Name switch
    {
        "toggle" => 0xB3,                       // VK_MEDIA_PLAY_PAUSE
        "stop" => 0xB2,                          // VK_MEDIA_STOP
        "next" => 0xB0,                         // VK_MEDIA_NEXT_TRACK
        "previous" => 0xB1,                      // VK_MEDIA_PREV_TRACK
        _ => null,
    };
    /// <summary>One <see cref="RemoteCommand"/> per string in the event's <c>commands</c>
    /// array, each carrying the event's <c>host</c> and <c>volume</c>. An event with
    /// only a <c>volume</c> (no commands) yields nothing: event-channel volume is
    /// not acted on (see the remarks).</summary>
    public static IReadOnlyList<RemoteCommand> Parse(System.Text.Json.JsonElement item)
    {
        var host = item.TryGetProperty("host", out var hostValue) && hostValue.ValueKind == System.Text.Json.JsonValueKind.String ? hostValue.GetString() : null;
        var volume = item.Number("volume");
        if (!item.TryGetProperty("commands", out var commands) || commands.ValueKind != System.Text.Json.JsonValueKind.Array)
            return [];
        return commands.EnumerateArray()
            .Where(command => command.ValueKind == System.Text.Json.JsonValueKind.String)
            .Select(command => new RemoteCommand(command.GetString()!, volume, host))
            .ToArray();
    }
}
