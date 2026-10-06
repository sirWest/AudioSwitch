using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using AudioSwitch.Osd;

namespace AudioSwitch.Settings;

internal sealed partial class SettingsWindow
{
    private void OsdMoved(int x, int y)
    {
        draft.Osd.Left = x;
        draft.Osd.Top = y;
    }

    private UIElement CreateOsdTab()
    {
        var panel = new StackPanel();
        panel.Children.Add(
            Check("Use custom OSD (otherwise Windows notification)", draft, nameof(draft.CustomOsd))
        );
        var skins = Choices(panel, "Skin", draft.Osd, nameof(draft.Osd.Skin), OsdWindow.Skins);
        skins.SelectionChanged += (_, _) => Preview();
        panel.Children.Add(skinInfo);
        panel.Children.Add(
            Button(
                "Skin website",
                () =>
                {
                    if (
                        Uri.TryCreate(app.Osd.Website, UriKind.Absolute, out var uri)
                        && uri.Scheme is "http" or "https"
                    )
                    {
                        Process.Start(
                            new ProcessStartInfo(uri.ToString()) { UseShellExecute = true }
                        );
                    }
                }
            )
        );
        var opacity = Slider(panel, "Opacity", draft.Osd, nameof(draft.Osd.Opacity), 0, 1, .01);
        opacity.ValueChanged += (_, _) => Preview();
        Label(panel, "Duration (milliseconds)");
        timeoutInput = new TextBox
        {
            Text = draft.Osd.Timeout.ToString(),
            Width = 120,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(0, 0, 0, 10),
        };
        panel.Children.Add(timeoutInput);
        timeoutInput.TextChanged += (_, _) =>
        {
            if (int.TryParse(timeoutInput.Text, out var timeout) && timeout is >= 100 and <= 100000)
            {
                draft.Osd.Timeout = timeout;
            }
        };
        var buttons = new WrapPanel();
        panel.Children.Add(buttons);
        buttons.Children.Add(Button("Volume preview", Preview));
        buttons.Children.Add(Button("Mute preview", () => PreviewState(true)));
        buttons.Children.Add(
            Button(
                "Device preview",
                () =>
                {
                    foreach (var commit in commitEditors)
                    {
                        commit();
                    }

                    PreviewState(
                        false,
                        app.Audio.List(draft.DefaultDirection, draft)
                            .FirstOrDefault()
                            ?.DisplayName(draft)
                            ?? "AudioSwitch"
                    );
                }
            )
        );
        buttons.Children.Add(
            Button(
                "Reset position",
                () =>
                {
                    draft.Osd.Left = 61;
                    draft.Osd.Top = 26;
                    Preview();
                }
            )
        );
        return panel;
    }

    private void Preview() => PreviewState(false);

    private void PreviewState(bool muted, string? device = null)
    {
        if (!initialized || tabs.SelectedItem != osdTab)
        {
            return;
        }

        try
        {
            app.Osd.Display(draft.Osd, .75f, muted, device, true);
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
