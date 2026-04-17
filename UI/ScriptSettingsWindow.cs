using Runbook.Interfaces;
using Runbook.Models;
using Terminal.Gui;

namespace Runbook.UI;

public class ScriptSettingsWindow(Script script, ITheme theme)
{
    private readonly Script script = script;
    private readonly ITheme theme = theme;

    public void Show()
    {
        var dialog = new Dialog()
        {
            Title = $"Settings - {script.Name}",
            Width = 40,
            Height = 10,
            ShadowStyle = ShadowStyle.None,
            BorderStyle = LineStyle.Heavy,
            ColorScheme = theme.Main(),
        };

        var loopCheckbox = new CheckBox()
        {
            Text = " Loop",
            X = 1,
            Y = 1,
            CheckedState = script.Loop ? CheckState.Checked : CheckState.UnChecked,
            ColorScheme = theme.Main(),
        };

        var backgroundCheckbox = new CheckBox()
        {
            Text = " Run in background",
            X = 1,
            Y = 3,
            CheckedState = script.Background ? CheckState.Checked : CheckState.UnChecked,
            ColorScheme = theme.Main(),
        };

        var saveButton = new Button()
        {
            Text = "Save",
            X = Pos.Center() - 6,
            Y = 5,
            ShadowStyle = ShadowStyle.None,
            ColorScheme = theme.SideBar(),
        };

        var cancelButton = new Button()
        {
            Text = "Cancel",
            X = Pos.Center() + 4,
            Y = 5,
            ShadowStyle = ShadowStyle.None,
            ColorScheme = theme.SideBar(),
        };

        cancelButton.Accepting += (sender, e) => Application.RequestStop();

        dialog.Add(loopCheckbox, backgroundCheckbox, saveButton, cancelButton);
        Application.Run(dialog);
    }
}
