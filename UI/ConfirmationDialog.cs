using Runbook.Interfaces;
using Terminal.Gui;

namespace Runbook.UI;

// A reusable yes/no confirmation dialog
public class ConfirmationDialog(ITheme theme)
{
    private readonly ITheme theme = theme;

    // Shows the dialog and returns true if the user confirmed, false if cancelled
    public bool Show(string title, string message)
    {
        bool confirmed = false;

        // Build the dialog window
        var dialog = new Dialog
        {
            Title = title,
            Width = 40,
            Height = 8,
            ShadowStyle = ShadowStyle.None,
            BorderStyle = LineStyle.Heavy,
            ColorScheme = theme.Main(),
        };

        // Message label centered in the dialog
        var label = new Label()
        {
            Text = message,
            X = Pos.Center(),
            Y = 1,
            ShadowStyle = ShadowStyle.None,
            ColorScheme = theme.SideBar(),
        };

        // Yes and No buttons positioned side by side
        var yesBtn = new Button()
        {
            Text = "Yes",
            X = Pos.Center() - 6,
            Y = 3,
            ShadowStyle = ShadowStyle.None,
            ColorScheme = theme.SideBar(),
        };
        var noBtn = new Button()
        {
            Text = "No",
            X = Pos.Center() + 4,
            Y = 3,
            ShadowStyle = ShadowStyle.None,
            ColorScheme = theme.SideBar(),
        };

        // Set confirmed and close the dialog on button press
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
