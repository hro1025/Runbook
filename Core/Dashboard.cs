using Terminal.Gui;

namespace Runbook.Core;

public class Dashboard
{
    public Window Window { get; }
    public ListView ListView { get; }
    public TextView TextView { get; }

    public Dashboard(List<string> displayNames, ColorScheme colorScheme)
    {
        Window = new Window
        {
            Title = "",
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            ColorScheme = colorScheme,
        };

        var titleLabel = new Label()
        {
            Text = "╔══════════ Runbook ══════════╗",
            X = Pos.Center(),
            Y = 0,
            CanFocus = false,
        };

        ListView = new ListView()
        {
            X = 0,
            Y = 1,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
        };
        ListView.SetSource(
            new System.Collections.ObjectModel.ObservableCollection<string>(displayNames)
        );

        TextView = new TextView()
        {
            X = 0,
            Y = 1,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            ReadOnly = true,
            CanFocus = false,
        };

        var sidebar = new FrameView()
        {
            Title = "Scripts",
            X = 0,
            Y = 1,
            Width = Dim.Percent(25),
            Height = Dim.Fill(1),
        };

        var detail = new FrameView()
        {
            Title = "Preview",
            X = Pos.Right(sidebar),
            Y = 1,
            Width = Dim.Fill(),
            Height = Dim.Fill(1),
            CanFocus = false,
        };

        var quitBtn = new Button()
        {
            Text = "Quit",
            X = Pos.AnchorEnd(9),
            Y = Pos.AnchorEnd(1),
        };

        quitBtn.Accepting += (sender, e) => Application.RequestStop();

        sidebar.Add(ListView);
        detail.Add(TextView);
        Window.Add(titleLabel, sidebar, detail, quitBtn);

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
