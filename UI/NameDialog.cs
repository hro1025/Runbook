using Runbook.Interfaces;
using Terminal.Gui;

namespace Runbook.UI;

// A dialog that prompts the user to enter a name for a new script
public class NameDialog(ITheme theme)
{
    private readonly ITheme theme = theme;

    // Shows the dialog and returns the entered name, or null if cancelled
    public string? Show(string title)
    {
        string? result = null;

        // Build the dialog window
        var dialog = new Dialog
        {
            Title = title,
            Width = 40,
            Height = 10,
            ShadowStyle = ShadowStyle.None,
            BorderStyle = LineStyle.Heavy,
            ColorScheme = theme.Main(),
        };

        var label = new Label()
        {
            Text = "Script name:",
            X = Pos.Center(),
            Y = 1,
            ColorScheme = theme.SideBar(),
        };

        var nameField = new TextField()
        {
            X = 2,
            Y = 3,
            Width = Dim.Fill(3),
            ColorScheme = theme.SideBar(),
        };

        // Prevent input beyond 35 characters
        nameField.KeyDown += (s, e) =>
        {
            if (nameField.Text?.Length >= 35 && e.KeyCode != KeyCode.Backspace)
                e.Handled = true;
        };

        var confirmBtn = new Button()
        {
            Text = "Create",
            X = Pos.Center() - 6,
            Y = 5,
            ShadowStyle = ShadowStyle.None,
            ColorScheme = theme.SideBar(),
        };
        var cancelBtn = new Button()
        {
            Text = "Cancel",
            X = Pos.Center() + 4,
            Y = 5,
            ShadowStyle = ShadowStyle.None,
            ColorScheme = theme.SideBar(),
        };

        // On confirm, trim and cap the name at 35 characters, then close
        confirmBtn.Accepting += (s, e) =>
        {
            var name = nameField.Text?.Trim();
            if (!string.IsNullOrEmpty(name))
                result = name.Length > 35 ? name[..35] : name;
            Application.RequestStop();
        };

        // On cancel, close without setting a result
        cancelBtn.Accepting += (s, e) => Application.RequestStop();

        dialog.Add(label, nameField, confirmBtn, cancelBtn);
        Application.Run(dialog);
        return result;
    }
}
