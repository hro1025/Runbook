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
    public Dictionary<string, string> ScriptOutputs { get; } = [];
    public ProgressBar ProgressBar { get; }

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
            Text = " Esc: Quit | Enter: Run | E: Edit | S: Script - Settings",
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
        ProgressBar = new ProgressBar()
        {
            X = Pos.Center(),
            Y = 0,
            Width = Dim.Fill(),
            Height = 5,
            Visible = false,
            ProgressBarStyle = ProgressBarStyle.MarqueeContinuous,
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

        var outputFrame = new FrameView()
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

                ScriptOutputs.TryGetValue(selected.Path!, out var savedOutput);
                Output.Text = savedOutput ?? "";

                LineNumbers.Text = string.Join("\n", numbers);
                TextView.Text = string.Join("\n", lines);
            }
        };
        ListView.SelectedItem = 1;
        ListView.SelectedItem = 0;

        ListView.OpenSelectedItem += async (sender, e) =>
        {
            var selected = scripts[ListView.SelectedItem];
            var run = confirmationDialog.Show("Run Script", $"Run {selected.Name}?");
            if (run)
            {
                ProgressBar.Visible = true;
                var timer = Application.AddTimeout(
                    TimeSpan.FromMilliseconds(100),
                    () =>
                    {
                        ProgressBar.Pulse();
                        return true;
                    }
                );

                var output = await executor.Execute(selected);

                Application.RemoveTimeout(timer!);
                ProgressBar.Visible = false;
                ScriptOutputs[selected.Path!] = output;
                Output.Text = output;
            }
        };

        sidebar.Add(ListView);
        preview.Add(LineNumbers, TextView);
        outputFrame.Add(Output, ProgressBar);
        Window.Add(sidebar, preview, outputFrame, StatusBar, EditBarEditing, EditBarSaved);
    }
}
