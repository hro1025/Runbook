using Runbook.Interfaces;
using Runbook.Models;
using Terminal.Gui;

namespace Runbook.UI;

// Builds and owns all UI components that make up the main application window
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
    public Dictionary<string, string> ScriptEdits { get; } = [];
    public ProgressBar ProgressBar { get; }
    private readonly HashSet<string> runningScripts = [];

    public Dashboard(
        List<Script> scripts,
        List<string> displayNames,
        ITheme theme,
        ConfirmationDialog confirmationDialog,
        IExecutor executor,
        MessageDialog messageDialog
    )
    {
        // Root window that fills the entire terminal
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

        // Sidebar list of script names
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

        // Output panel showing stdout/stderr from script execution
        Output = new TextView
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(1),
            ReadOnly = true,
            CanFocus = true,
            ColorScheme = theme.Main(),
            WordWrap = true,
        };

        // Script preview panel showing file contents
        TextView = new TextView()
        {
            X = 1,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            ReadOnly = true,
            CanFocus = true,
        };

        // Update line numbers whenever the preview content changes
        TextView.ContentsChanged += (sender, e) =>
        {
            var lines = TextView.Text?.Split('\n') ?? [];
            var numbers = new string[lines.Length];
            for (int i = 0; i < lines.Length; i++)
                numbers[i] = (i + 1).ToString().PadLeft(4);
        };

        // Bottom status bar showing available keybinds
        StatusBar = new Label()
        {
            Text = " Esc: Quit | Enter: Run | E: Edit | C: Create | D: Delete ",
            X = 0,
            Y = Pos.AnchorEnd(1),
            Width = Dim.Fill(),
            ColorScheme = theme.StatusBar(),
        };

        // Edit bar shown in red while the user is actively editing
        EditBarEditing = new Label
        {
            Text = " EDITING | Ctrl+S: Save | Esc: Cancel ",
            X = 0,
            Y = Pos.AnchorEnd(1),
            Width = Dim.Fill(),
            ColorScheme = theme.EditBarEditing(),
            Visible = false,
        };

        // Edit bar shown in green after the file has been saved
        EditBarSaved = new Label
        {
            Text = " EDITING | Ctrl+S: Save | Esc: Cancel ",
            X = 0,
            Y = Pos.AnchorEnd(1),
            Width = Dim.Fill(),
            ColorScheme = theme.EditBarSaved(),
            Visible = false,
        };

        // Marquee progress bar shown while a script is running
        ProgressBar = new ProgressBar()
        {
            X = 2,
            Y = Pos.AnchorEnd(1),
            Width = Dim.Fill(2),
            Height = 1,
            Visible = false,
            ProgressBarStyle = ProgressBarStyle.MarqueeContinuous,
        };

        // Left panel: script list
        var sidebar = new FrameView()
        {
            Title = "Scripts",
            X = 0,
            Y = 0,
            BorderStyle = LineStyle.Rounded,
            Width = Dim.Percent(15),
            Height = Dim.Fill(1),
        };

        // Center panel: script file preview with line numbers
        var preview = new FrameView()
        {
            Title = "Preview",
            X = Pos.Right(sidebar),
            Y = 0,
            BorderStyle = LineStyle.Rounded,
            Width = Dim.Fill(80),
            Height = Dim.Fill(1),
        };

        // Right panel: script execution output
        var outputFrame = new FrameView()
        {
            Title = "Output",
            X = Pos.Right(preview),
            Y = 0,
            BorderStyle = LineStyle.Rounded,
            Width = Dim.Fill(),
            Height = Dim.Fill(1),
        };

        // When a script is selected, load its contents into the preview and restore any saved output
        ListView.SelectedItemChanged += (sender, e) =>
        {
            if (e.Item >= 0 && e.Item < scripts.Count)
            {
                var selected = scripts[e.Item];

                ScriptOutputs.TryGetValue(selected.Path!, out var savedOutput);
                Output.Text = savedOutput ?? "";
                Output.MoveEnd();

                // Restore staged edit if one exists, otherwise load from disk
                if (ScriptEdits.TryGetValue(selected.Path!, out var edited))
                    TextView.Text = edited;
                else
                    TextView.Text = File.ReadAllText(selected.Path!);

                ProgressBar.Visible = runningScripts.Contains(selected.Path!);
            }
        };

        // Track edits per script in memory as the user types
        TextView.ContentsChanged += (sender, e) =>
        {
            if (ListView.SelectedItem >= 0 && ListView.SelectedItem < scripts.Count)
            {
                var selected = scripts[ListView.SelectedItem];
                if (selected.Path != null)
                    ScriptEdits[selected.Path] = TextView.Text ?? "";
            }
        };

        // Trigger selection change to load the first script on startup
        if (scripts.Count > 0)
        {
            ListView.SelectedItem = 0;
        }

        // On Enter, confirm and execute the selected script, streaming output in real time
        ListView.OpenSelectedItem += async (sender, e) =>
        {
            var selected = scripts[ListView.SelectedItem];
            // Check before asking — no point confirming if it's already running
            if (runningScripts.Contains(selected.Path!))
            {
                messageDialog.Show("Already running", $"{selected.Name} is already running.");
                return;
            }
            var run = confirmationDialog.Show("Run Script", $"Run {selected.Name}?");
            if (run)
            {
                ScriptOutputs[selected.Path!] = "";
                Output.Text = "";
                runningScripts.Add(selected.Path!);
                ProgressBar.Visible = true;

                var timer = Application.AddTimeout(
                    TimeSpan.FromMilliseconds(30),
                    () =>
                    {
                        ProgressBar.Pulse();
                        return true;
                    }
                );

                try
                {
                    await executor.Execute(
                        selected,
                        line =>
                        {
                            Application.Invoke(() =>
                            {
                                ScriptOutputs[selected.Path!] += line + "\n";
                                if (scripts[ListView.SelectedItem].Path == selected.Path)
                                {
                                    Output.Text = ScriptOutputs[selected.Path!];
                                    Output.MoveEnd();
                                }
                            });
                        }
                    );
                }
                catch (Exception ex)
                {
                    Application.Invoke(() => messageDialog.Show("Missing runtime", ex.Message));
                }

                Application.RemoveTimeout(timer!);
                runningScripts.Remove(selected.Path!);

                if (scripts[ListView.SelectedItem].Path == selected.Path)
                    ProgressBar.Visible = false;
            }
        };

        sidebar.Add(ListView);
        preview.Add(TextView);
        outputFrame.Add(Output, ProgressBar);
        Window.Add(sidebar, preview, outputFrame, StatusBar, EditBarEditing, EditBarSaved);
    }
}
