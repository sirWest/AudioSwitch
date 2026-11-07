using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using AudioSwitch.Osd;

namespace AudioSwitch.Settings;

internal sealed partial class SettingsWindow
{
    private void OsdMoved(int x, int y)
    {
        settings.Osd.Left = x;
        settings.Osd.Top = y;
    }

    private void InitializeOsdTab()
    {
        skins.ItemsSource = OsdWindow.Skins;
        timeoutInput.Text = settings.Osd.Timeout.ToString();
    }

    private void CustomOsdChecked(object sender, RoutedEventArgs e)
    {
        if (initialized)
            Dispatcher.BeginInvoke(new Action(Preview));
    }

    private void CustomOsdUnchecked(object sender, RoutedEventArgs e)
    {
        if (initialized)
            app.Osd.EndPreview();
    }

    private void PreviewSelectionChanged(object sender, SelectionChangedEventArgs e) => Preview();

    private void PreviewValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) =>
        Preview();

    private void TimeoutTextChanged(object sender, TextChangedEventArgs e)
    {
        if (int.TryParse(timeoutInput.Text, out var timeout) && timeout is >= 100 and <= 100000)
            settings.Osd.Timeout = timeout;
    }

    private void SkinWebsiteClick(object sender, RoutedEventArgs e)
    {
        if (
            Uri.TryCreate(app.Osd.Website, UriKind.Absolute, out var uri)
            && uri.Scheme is "http" or "https"
        )
        {
            using var process = Process.Start(
                new ProcessStartInfo(uri.ToString()) { UseShellExecute = true }
            );
        }
    }

    private void VolumePreviewClick(object sender, RoutedEventArgs e) => Preview();

    private void MutePreviewClick(object sender, RoutedEventArgs e) => PreviewState(true);

    private void DevicePreviewClick(object sender, RoutedEventArgs e)
    {
        foreach (var commit in commitEditors)
            commit();
        PreviewState(
            false,
            app.Audio.List(settings.DefaultDirection, settings)
                .FirstOrDefault()
                ?.DisplayName(settings)
                ?? "AudioSwitch"
        );
    }

    private void ResetPositionClick(object sender, RoutedEventArgs e)
    {
        settings.Osd.Left = 61;
        settings.Osd.Top = 26;
        Preview();
    }

    private void Preview() => PreviewState(false);

    private void PreviewState(bool muted, string? device = null)
    {
        if (!initialized || !ReferenceEquals(tabs.SelectedItem, osdTab))
        {
            return;
        }

        if (!settings.CustomOsd)
        {
            app.Osd.EndPreview();
            return;
        }

        try
        {
            app.Osd.Display(settings.Osd, .75f, muted, device, true);
            skinInfo.Text = $"{app.Osd.Author}   /   {app.Osd.SkinVersion}";
            statusText.Text = "";
        }
        catch (Exception ex)
        {
            statusText.Text = ex.Message;
            app.Osd.EndPreview();
        }
    }
}
