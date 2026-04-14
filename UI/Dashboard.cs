using Runbook.Core;
using Runbook.Interfaces;
using Runbook.Models;
using Terminal.Gui;

namespace Runbook.UI;

public class Dashboard
{
    public Window Window { get; }
    public ListView ListView { get; }
    public TextView TextView { get; }
    public TextView Output { get; }
    public Label LineNumbers { get; }

    public Dashboard(
        List<Script> scripts,
        List<string> displayNames,
        ITheme theme,
        ConfirmationDialog confirmDialog,
        IExecutor iexecutor
    )
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

        Output = new TextView()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            ReadOnly = true,
            CanFocus = true,
            ColorScheme = theme.Main(),
        };

        TextView = new TextView()
        {
            X = 5,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            ReadOnly = true,
            CanFocus = true,
        };

        var sidebar = new FrameView()
        {
            Title = "Scripts",
            X = 0,
            Y = 0,
            BorderStyle = LineStyle.Rounded,
            Width = Dim.Percent(15),
            Height = Dim.Fill(1),
        };

        var preview = new FrameView()
        {
            Title = "Preview",
            X = Pos.Right(sidebar),
            Y = 0,
            BorderStyle = LineStyle.Rounded,

            Width = Dim.Fill(80),
            Height = Dim.Fill(1),
        };

        var output = new FrameView()
        {
            Title = "Output",
            X = Pos.Right(preview),
            Y = 0,
            BorderStyle = LineStyle.Rounded,
            Width = Dim.Fill(),
            Height = Dim.Fill(1),
        };

        var statusBar = new Label
        {
            Text = " Esc: Quit | Enter: Run | /: Search | Tab: Switch",
            X = 0,
            Y = Pos.AnchorEnd(1),
            Width = Dim.Fill(),
            ColorScheme = theme.StatusBar(),
        };

        sidebar.Add(ListView);
        preview.Add(LineNumbers, TextView);
        output.Add(Output);
        Window.Add(sidebar, preview, output, statusBar);

        Window.KeyDown += (sender, e) =>
        {
            if (e.KeyCode == KeyCode.Esc)
            {
                var confirmed = confirmDialog.Show("Quit", "Exit Runbook?");
                if (confirmed)
                {
                    Application.RequestStop();
                }
                e.Handled = true;
            }
            if (e.KeyCode == KeyCode.E)
            {
                {
                    var confirmed = confirmDialog.Show("Edit", "Edit the script?");
                    if (confirmed)
                    {
                        iexecutor.OpenProgram(scripts[ListView.SelectedItem].Path!);
                    }

                    e.Handled = true;
                }
            }

            ListView.SetFocus();
        };
    }
}
