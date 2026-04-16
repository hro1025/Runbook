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
    public Label StatusBar { get; }
    public Label EditBarEditing { get; }
    public Label EditBarSaved { get; }

    public Dashboard(
        List<Script> scripts,
        List<string> displayNames,
        ITheme theme,
        ConfirmationDialog confirmationDialog,
        IExecutor executor
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
            ColorScheme = theme.SideBar(),
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
        StatusBar = new Label()
        {
            Text = " Esc: Quit | Enter: Run | E: Edit",
            X = 0,
            Y = Pos.AnchorEnd(1),
            Width = Dim.Fill(),
            ColorScheme = theme.StatusBar(),
        };

        EditBarEditing = new Label
        {
            Text = " EDITING | Ctrl+S: Save | Esc: Cancel ",
            X = 0,
            Y = Pos.AnchorEnd(1),
            Width = Dim.Fill(),
            ColorScheme = theme.EditBarEditing(),
            Visible = false,
        };
        EditBarSaved = new Label
        {
            Text = " EDITING | Ctrl+S: Save | Esc: Cancel ",
            X = 0,
            Y = Pos.AnchorEnd(1),
            Width = Dim.Fill(),
            ColorScheme = theme.EditBarSaved(),
            Visible = false,
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

        ListView.SelectedItemChanged += (sender, e) =>
        {
            if (e.Item >= 0 && e.Item < scripts.Count)
            {
                var selected = scripts[e.Item];
                var lines = File.ReadAllLines(selected.Path!);
                var numbers = new string[lines.Length];
                for (int i = 0; i < lines.Length; i++)
                {
                    numbers[i] = (i + 1).ToString().PadLeft(4);
                }
                LineNumbers.Text = string.Join("\n", numbers);
                TextView.Text = string.Join("\n", lines);
            }
        };
        ListView.SelectedItem = 1;
        ListView.SelectedItem = 0;

        ListView.OpenSelectedItem += (sender, e) =>
        {
            var selected = scripts[ListView.SelectedItem];
            var run = confirmationDialog.Show("Run Script", $"Run {selected.Name}?");
            if (run)
            {
                Output.Text = executor.Execute(selected);
            }
        };

        sidebar.Add(ListView);
        preview.Add(LineNumbers, TextView);
        output.Add(Output);
        Window.Add(sidebar, preview, output, StatusBar, EditBarEditing, EditBarSaved);
    }
}
