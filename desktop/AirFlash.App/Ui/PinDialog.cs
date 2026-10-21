using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AirFlash.App.Services;
using AirFlash.Core;
namespace AirFlash.App.Ui;

public sealed class PinDialog : Window
{
    private readonly TextBox _code = new() { MaxLength = 16, FontSize = 24, HorizontalContentAlignment = HorizontalAlignment.Center };
    private readonly PasswordBox _password = new() { MaxLength = 128, FontSize = 24 };
    private PinDialog(Receiver receiver)
    {
        Title = L.Get("Pair receiver"); Width = 390; SizeToContent = SizeToContent.Height; ResizeMode = ResizeMode.NoResize; WindowStartupLocation = WindowStartupLocation.CenterOwner;
        WindowThemeService.Track(this);
        AppIconService.Track(this);
        var panel = new StackPanel { Margin = new(24) };
        panel.Children.Add(new TextBlock { Text = L.Format(receiver.RequiresPassword ? "Enter the AirPlay password for {0} ({1})" : "Enter the PIN provided by {0} ({1})", receiver.Name, receiver.Address), TextWrapping = TextWrapping.Wrap, Margin = new(0, 0, 0, 18) });
        _code.PreviewTextInput += (_, args) => args.Handled = !PinInput.AcceptsText(args.Text);
        panel.Children.Add(receiver.RequiresPassword ? _password : _code);
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new(0, 18, 0, 0) };
        var ok = new Button { Content = L.Get("Pair"), IsDefault = true, IsEnabled = false, MinWidth = 78, Margin = new(0, 0, 8, 0) };
        _code.TextChanged += (_, _) => ok.IsEnabled = PinInput.Normalize(_code.Text) is not null;
        _password.PasswordChanged += (_, _) => ok.IsEnabled = ReadValidPassword() is not null;
        ok.Click += (_, _) => DialogResult = true;
        buttons.Children.Add(ok); buttons.Children.Add(new Button { Content = L.Get("Cancel"), IsCancel = true, MinWidth = 78 }); panel.Children.Add(buttons); Content = panel;
        Loaded += (_, _) => { if (receiver.RequiresPassword) _password.Focus(); else _code.Focus(); };
    }
    public static async Task<string?> RequestAsync(Window owner, Receiver receiver, CancellationToken cancellation)
    {
        return await owner.Dispatcher.InvokeAsync(() =>
        {
            cancellation.ThrowIfCancellationRequested();
            var dialog = new PinDialog(receiver) { Owner = owner };
            using var registration = cancellation.Register(() => owner.Dispatcher.BeginInvoke(() => dialog.Close()));
            string? result = null;
            if (dialog.ShowDialog() == true)
            {
                if (receiver.RequiresPassword) result = dialog.ReadValidPassword();
                else result = PinInput.Normalize(dialog._code.Text);
            }
            dialog._password.Clear(); dialog._code.Clear();
            cancellation.ThrowIfCancellationRequested();
            return result;
        });
    }

    private string? ReadValidPassword()
    {
        var password = _password.Password;
        using var securePassword = _password.SecurePassword;
        // WPF's plaintext getter stops at NUL while its backing value retains
        // every character. Reject that mismatch rather than submit a prefix.
        return securePassword.Length == password.Length && PasswordInput.IsValid(password) ? password : null;
    }
}
