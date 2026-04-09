using Runbook.Interfaces;
using Terminal.Gui;

namespace Runbook.Core;

public class Dashboard
{
    private const string V = ", ";

    public Window Window { get; }
    public ListView ListView { get; }
    public TextView TextView { get; }

    public Dashboard(List<string> displayNames, ITheme theme)
    {
        Window = new Window
        {
            Title = "",
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            BorderStyle = LineStyle.None,
            ColorScheme = theme.Main(),
        };

        ListView = new ListView()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
        };
        ListView.SetSource(
            new System.Collections.ObjectModel.ObservableCollection<string>(displayNames)
        );

        TextView = new TextView()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            ReadOnly = true,
            CanFocus = false,
        };

        var sideMenu = new Label { };

        var sidebar = new FrameView()
        {
            Title = "Scripts",
            X = 0,
            Y = 0,
            Width = Dim.Percent(25),
            Height = Dim.Fill(1),
        };

        var detail = new FrameView()
        {
            Title = "Preview",
            X = Pos.Right(sidebar),
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(1),
            CanFocus = false,
        };

        var statusBar = new Label
        {
            Text = " Esc: Quit | Enter: Run | /: Search | Tab: Switch",
            X = 0,
            Y = Pos.AnchorEnd(1),
            Width = Dim.Fill(),
            CanFocus = false,
            ColorScheme = theme.Main(),
        };

        sidebar.Add(ListView);
        detail.Add(TextView);
        Window.Add(sidebar, detail, statusBar);

        Window.KeyDown += (sender, e) =>
        {
            if (e.KeyCode == KeyCode.Esc)
            {
                Application.RequestStop();
                e.Handled = true;
            }
        };

        ListView.SetFocus();
    }
}
