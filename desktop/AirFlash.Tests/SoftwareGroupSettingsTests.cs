using System.Text;
using System.Text.Json;
using AirFlash.Core;
using Xunit;

namespace AirFlash.Tests;

// Invalid software-group shapes must use controlled recovery at both startup loads.
// Temporary files preserve the original encoding and unknown-field contract.
public sealed class SoftwareGroupSettingsTests
{
    [Theory]
    [InlineData("utf8-bom", "null")]
    [InlineData("utf16-le-bom", "null")]
    [InlineData("utf8-bom", "blank")]
    [InlineData("utf16-le-bom", "blank")]
    [InlineData("utf8-bom", "bad-prefix")]
    [InlineData("utf16-le-bom", "bad-prefix")]
    [InlineData("utf8-bom", "whitespace")]
    [InlineData("utf16-le-bom", "whitespace")]
    [InlineData("utf8-bom", "null-name")]
    [InlineData("utf16-le-bom", "null-name")]
    [InlineData("utf8-bom", "null-rate")]
    [InlineData("utf16-le-bom", "null-rate")]
    [InlineData("utf8-bom", "null-left")]
    [InlineData("utf16-le-bom", "null-left")]
    [InlineData("utf8-bom", "null-right")]
    [InlineData("utf16-le-bom", "null-right")]
    public void InvalidSoftwareGroupRecoversAcrossBothLoadsAndPreservesOriginalBytes(string encodingName, string shape)
    {
        using var fixture = new TemporaryStore();
        string? id = shape switch
        {
            "null" => null,
            "blank" => "",
            "whitespace" => " \t",
            "bad-prefix" => "stereo:invalid-software-group",
            "null-name" or "null-rate" or "null-left" or "null-right" => "software:synthetic-group",
            _ => throw new ArgumentException(shape)
        };
        var original = Encode(Configuration(id,
            name: shape == "null-name" ? null : "Synthetic group",
            rate: shape == "null-rate" ? null : "44100",
            left: shape == "null-left" ? null : "synthetic-left",
            right: shape == "null-right" ? null : "synthetic-right"), encodingName);
        File.WriteAllBytes(fixture.Path, original);
        var store = new SettingsStore(fixture.Path);

        // Production uses the same store for shell preferences and AppViewModel.
        AssertSafeDefaults(store.Load());
        Assert.False(string.IsNullOrWhiteSpace(store.LoadWarning));
        AssertSafeDefaults(store.Load());
        Assert.False(string.IsNullOrWhiteSpace(store.LoadWarning));
        Assert.Equal(original, File.ReadAllBytes(fixture.Path));
        Assert.False(File.Exists(fixture.Path + ".pre-wpf.bak"));

        store.Save(new AppSettings { MasterVolume = 29 });
        Assert.Equal(original, File.ReadAllBytes(fixture.Path + ".pre-wpf.bak"));
        var saved = new SettingsStore(fixture.Path).Load();
        Assert.Null(saved.Validate());
        Assert.Equal(29, saved.MasterVolume);
        Assert.Empty(saved.SoftwareGroups);
        Assert.False(saved.AutoConnectOnDiscover);
        Assert.False(saved.StartAtLogin);
        Assert.Empty(Directory.GetFiles(fixture.DirectoryPath, "*.tmp"));
    }

    [Theory]
    [InlineData("utf8-bom")]
    [InlineData("utf16-le-bom")]
    public void OrdinarySoftwareGroupRemainsValidAcrossBothLoadsAndSave(string encodingName)
    {
        using var fixture = new TemporaryStore();
        var original = Encode(Configuration("software:synthetic-group"), encodingName);
        File.WriteAllBytes(fixture.Path, original);
        var store = new SettingsStore(fixture.Path);
        foreach (var settings in new[] { store.Load(), store.Load() })
        {
            Assert.Null(settings.Validate());
            Assert.Null(store.LoadWarning);
            var group = Assert.Single(settings.SoftwareGroups);
            Assert.Equal("software:synthetic-group", group.Id);
            Assert.Equal("44100", group.SampleRate);
            Assert.True(settings.AutoConnectOnDiscover);
            Assert.True(settings.StartAtLogin);
            Assert.Equal("retained", settings.Extra!["synthetic_extension"].GetString());
        }
        Assert.Equal(original, File.ReadAllBytes(fixture.Path));
        var edited = store.Load(); edited.MasterVolume = 31; store.Save(edited);
        var saved = new SettingsStore(fixture.Path).Load();
        Assert.Null(saved.Validate());
        Assert.Equal(31, saved.MasterVolume);
        Assert.Equal("software:synthetic-group", Assert.Single(saved.SoftwareGroups).Id);
        Assert.Equal("retained", saved.Extra!["synthetic_extension"].GetString());
        Assert.False(File.Exists(fixture.Path + ".pre-wpf.bak"));
        Assert.Empty(Directory.GetFiles(fixture.DirectoryPath, "*.tmp"));
    }

    [Fact]
    public void NullSoftwareGroupIdentityReturnsAValidationDiagnostic()
    {
        var settings = JsonSerializer.Deserialize<AppSettings>(Configuration(null), AppSettings.JsonOptions)!;
        Assert.False(string.IsNullOrWhiteSpace(settings.Validate()));
    }

    private static string Configuration(string? id, string? name = "Synthetic group", string? rate = "44100",
        string? left = "synthetic-left", string? right = "synthetic-right") => JsonSerializer.Serialize(new
    {
        schema_version = 4,
        auto_connect_on_discover = true,
        start_at_login = true,
        synthetic_extension = "retained",
        software_groups = new[] { new { id, name, left_id = left, right_id = right, sample_rate = rate } }
    });

    private static byte[] Encode(string text, string name)
    {
        Encoding encoding = name switch
        {
            "utf8-bom" => new UTF8Encoding(true),
            "utf16-le-bom" => new UnicodeEncoding(false, true),
            _ => throw new ArgumentException(name)
        };
        return [.. encoding.GetPreamble(), .. encoding.GetBytes(text)];
    }

    private static void AssertSafeDefaults(AppSettings settings)
    {
        Assert.Null(settings.Validate());
        Assert.Equal(AppSettings.CurrentSchemaVersion, settings.SchemaVersion);
        Assert.Equal(100, settings.MasterVolume);
        Assert.False(settings.AutoConnectOnDiscover);
        Assert.False(settings.StartAtLogin);
        Assert.Empty(settings.SoftwareGroups);
        Assert.Empty(settings.ManualReceivers);
        Assert.Empty(settings.Receivers);
    }

    private sealed class TemporaryStore : IDisposable
    {
        public string DirectoryPath { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "airflash-software-group-settings", Guid.NewGuid().ToString("N"));
        public string Path => System.IO.Path.Combine(DirectoryPath, "config.json");
        public TemporaryStore() => Directory.CreateDirectory(DirectoryPath);
        public void Dispose() => Directory.Delete(DirectoryPath, recursive: true);
    }
}
