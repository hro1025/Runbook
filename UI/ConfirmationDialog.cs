using Runbook.Interfaces;
using Terminal.Gui;

namespace Runbook.UI;

public class ConfirmationDialog(ITheme theme)
{
    private readonly ITheme theme = theme;

    public bool Show(string title, string message)
    {
        bool confirmed = false;

        var dialog = new Dialog
        {
            Title = title,
            Width = 40,
            Height = 8,
            ShadowStyle = ShadowStyle.None,
            BorderStyle = LineStyle.Heavy,
            ColorScheme = theme.Main(),
        };

        var label = new Label()
        {
            Text = message,
            X = Pos.Center(),
            Y = 1,
            ShadowStyle = ShadowStyle.None,
            ColorScheme = theme.Sidebar(),
        };

        var yesBtn = new Button()
        {
            Text = "Yes",
            X = Pos.Center() - 6,
            Y = 3,
            ShadowStyle = ShadowStyle.None,
            ColorScheme = theme.Sidebar(),
        };

        var noBtn = new Button()
        {
            Text = "No",
            X = Pos.Center() + 4,
            Y = 3,
            ShadowStyle = ShadowStyle.None,
            ColorScheme = theme.Sidebar(),
        };

        yesBtn.Accepting += (sender, e) =>
        {
            confirmed = true;
            Application.RequestStop();
        };

        noBtn.Accepting += (sender, e) =>
        {
            confirmed = false;
            Application.RequestStop();
        };

        dialog.Add(label, yesBtn, noBtn);
        Application.Run(dialog);

        return confirmed;
    }
}
