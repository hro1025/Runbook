using Runbook.Interfaces;
using Terminal.Gui;

namespace Runbook.UI;

// A dialog that prompts the user to enter a name for a new script
public class NameDialog(ITheme theme)
{
    private readonly ITheme theme = theme;

    public string? Show(string title)
    {
        string? result = null;

        var dialog = new Dialog
        {
            Title = title,
            Width = 52,
            Height = 10,
            ShadowStyle = ShadowStyle.None,
            BorderStyle = LineStyle.Heavy,
            ColorScheme = theme.Main(),
        };

        // Left side — name input
        var nameLabel = new Label()
        {
            Text = "Script name:",
            X = 2,
            Y = 1,
            ColorScheme = theme.SideBar(),
        };

        var nameField = new TextField()
        {
            X = 2,
            Y = 2,
            Width = 28,
            ColorScheme = theme.SideBar(),
        };

        nameField.KeyDown += (s, e) =>
        {
            if (nameField.Text?.Length >= 28 && e.KeyCode != KeyCode.Backspace)
                e.Handled = true;
        };

        // Vertical divider
        var divider1 = new Label()
        {
            Text = "│",
            X = 32,
            Y = 1,
            ColorScheme = theme.SideBar(),
        };
        var divider2 = new Label()
        {
            Text = "│",
            X = 32,
            Y = 2,
            ColorScheme = theme.SideBar(),
        };
        var divider3 = new Label()
        {
            Text = "│",
            X = 32,
            Y = 3,
            ColorScheme = theme.SideBar(),
        };
        var divider4 = new Label()
        {
            Text = "│",
            X = 32,
            Y = 4,
            ColorScheme = theme.SideBar(),
        };
        var divider5 = new Label()
        {
            Text = "│",
            X = 32,
            Y = 5,
            ColorScheme = theme.SideBar(),
        };
        var divider6 = new Label()
        {
            Text = "│",
            X = 32,
            Y = 6,
            ColorScheme = theme.SideBar(),
        };

        // Right side — extensions info
        var extLabel = new Label()
        {
            Text = "Extensions",
            X = 34,
            Y = 1,
            ColorScheme = theme.SideBar(),
        };

        var extText = new Label()
        {
            Text = ".sh  = Bash\n.py  = Python\n.csx = C#",
            X = 34,
            Y = 3,
            ColorScheme = theme.SideBar(),
        };

        var confirmBtn = new Button()
        {
            Text = "Create",
            X = 4,
            Y = 7,
            ShadowStyle = ShadowStyle.None,
            ColorScheme = theme.SideBar(),
        };

        var cancelBtn = new Button()
        {
            Text = "Cancel",
            X = 14,
            Y = 7,
            ShadowStyle = ShadowStyle.None,
            ColorScheme = theme.SideBar(),
        };

        confirmBtn.Accepting += (s, e) =>
        {
            var name = nameField.Text?.Trim();
            if (!string.IsNullOrEmpty(name))
                result = name.Length > 28 ? name[..28] : name;
            Application.RequestStop();
        };

        cancelBtn.Accepting += (s, e) => Application.RequestStop();

        dialog.Add(
            nameLabel,
            nameField,
            divider1,
            divider2,
            divider3,
            divider4,
            divider5,
            divider6,
            extLabel,
            extText,
            confirmBtn,
            cancelBtn
        );

        Application.Run(dialog);
        return result;
    }
}
