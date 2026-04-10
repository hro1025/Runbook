using Runbook.Interfaces;
using Terminal.Gui;

namespace Runbook.Core;

public class Dashboard
{
    private const string V = ", ";

    public Window Window { get; }
    public ListView ListView { get; }
    public TextView TextView { get; }
    public Label LineNumbers { get; }

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
            ColorScheme = theme.Sidebar(),
        };
        ListView.SetSource(
            new System.Collections.ObjectModel.ObservableCollection<string>(displayNames)
        );
        LineNumbers = new Label
        {
            X = 0,
            Y = 0,
            Width = 5,
            Height = Dim.Fill(),
            CanFocus = false,
            ColorScheme = theme.NumberBar(),
        };

        TextView = new TextView()
        {
            X = 5,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            ReadOnly = true,
            CanFocus = false,
        };

        var sidebar = new FrameView()
        {
            Title = "Scripts",
            X = 0,
            Y = 0,
            BorderStyle = LineStyle.Rounded,
            Width = Dim.Percent(30),
            Height = Dim.Fill(1),
        };

        var detail = new FrameView()
        {
            Title = "Preview",
            X = Pos.Right(sidebar),
            Y = 0,
            BorderStyle = LineStyle.Rounded,
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
            ColorScheme = theme.StatusBar(),
        };

        sidebar.Add(ListView);
        detail.Add(LineNumbers, TextView);
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
