using Runbook.Interfaces;
using Terminal.Gui;

namespace Runbook.UI;

public class MessageDialog(ITheme theme)
{
    private readonly ITheme theme = theme;

    public void Show(string title, string message)
    {
        var dialog = new Dialog
        {
            Title = title,
            Width = 53,
            Height = 10,
            ShadowStyle = ShadowStyle.None,
            BorderStyle = LineStyle.Heavy,
            ColorScheme = theme.Main(),
        };

        var label = new Label
        {
            Text = message,
            X = Pos.Center(),
            Y = 1,
            Width = Dim.Fill(2),
            ColorScheme = theme.SideBar(),
        };

        var okBtn = new Button
        {
            Text = "Ok",
            X = Pos.Center(),
            Y = 5,
            ShadowStyle = ShadowStyle.None,
            ColorScheme = theme.SideBar(),
        };

        okBtn.Accepting += (s, e) => Application.RequestStop();

        dialog.Add(label, okBtn);
        Application.Run(dialog);
    }
}
