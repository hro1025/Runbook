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
    public Dictionary<string, string> ScriptEdits { get; } = [];
    public Dictionary<string, string> ScriptStatus { get; } = [];
    public ProgressBar ProgressBar { get; }
    private readonly HashSet<string> runningScripts = [];
    private readonly Dictionary<string, CancellationTokenSource> cancellations = [];

    private static string GetLogPath(Script script) =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".scripts",
            "runbook",
            "logs",
            $"{Path.GetFileName(script.Path)}.log"
        );

    public void RefreshDisplayNames(List<Script> scripts, string? editingPath)
    {
        var currentIndex = ListView.SelectedItem;
        int maxLen = scripts.Count > 0 ? scripts.Max(s => s.Name.Length) : 0;

        var names = scripts.ConvertAll(s =>
        {
            string status;
            if (editingPath == s.Path)
            {
                status = "Editing";
            }
            else if (runningScripts.Contains(s.Path!))
            {
                status = "Running";
            }
            else if (ScriptStatus.TryGetValue(s.Path!, out var st))
            {
                status =
                    st == "done" ? "Done"
                    : st == "stopped" ? "Stopped"
                    : "Error";
            }
            else
            {
                status = "";
            }

            var paddedName = s.Name.PadRight(maxLen);
            return status?.Length == 0 ? paddedName : $"{paddedName}  >> {status}";
        });

        ListView.SetSource(new System.Collections.ObjectModel.ObservableCollection<string>(names));
        ListView.SelectedItem = currentIndex;
    }

    public Dashboard(
        List<Script> scripts,
        List<string> displayNames,
        ITheme theme,
        ConfirmationDialog confirmationDialog,
        IExecutor executor,
        MessageDialog messageDialog
    )
    {
        // ── Restore running state from pid files on startup ──────────
        foreach (var script in scripts)
        {
            if (executor.IsRunning(script))
                runningScripts.Add(script.Path!);
        }

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

        TextView = new TextView()
        {
            X = 1,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            ReadOnly = true,
            CanFocus = true,
        };

        Output = new TextView
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(1),
            ReadOnly = true,
            CanFocus = true,
            ColorScheme = theme.Main(),
            WordWrap = false,
        };

        TextView.ContentsChanged += (sender, e) =>
        {
            var lines = TextView.Text?.Split('\n') ?? [];
            var numbers = new string[lines.Length];
            for (int i = 0; i < lines.Length; i++)
                numbers[i] = (i + 1).ToString().PadLeft(4);
        };

        StatusBar = new Label()
        {
            Text = " Esc: Quit | Enter: Run | E: Edit | C: Create | D: Delete ",
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
            X = 2,
            Y = Pos.AnchorEnd(1),
            Width = Dim.Fill(2),
            Height = 1,
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

                ScriptOutputs.TryGetValue(selected.Path!, out var savedOutput);
                Output.Text = savedOutput ?? "";
                Output.MoveEnd();

                if (ScriptEdits.TryGetValue(selected.Path!, out var edited))
                    TextView.Text = edited;
                else
                    TextView.Text = File.ReadAllText(selected.Path!);

                ProgressBar.Visible = runningScripts.Contains(selected.Path!);
            }
        };

        TextView.ContentsChanged += (sender, e) =>
        {
            if (ListView.SelectedItem >= 0 && ListView.SelectedItem < scripts.Count)
            {
                var selected = scripts[ListView.SelectedItem];
                if (selected.Path != null)
                    ScriptEdits[selected.Path] = TextView.Text ?? "";
            }
        };

        if (scripts.Count > 0)
            ListView.SelectedItem = 0;

        // ── Refresh running status on startup ────────────────────────
        Application.Invoke(() => RefreshDisplayNames(scripts, null));

        // ── Reattach tails for scripts already running ───────────────
        foreach (var script in scripts)
        {
            if (!executor.IsRunning(script))
                continue;

            // Load existing log output instead of clearing it
            var logPath = GetLogPath(script);
            ScriptOutputs[script.Path!] = File.Exists(logPath) ? File.ReadAllText(logPath) : "";

            var cts = new CancellationTokenSource();
            cancellations[script.Path!] = cts;
            var capturedScript = script;

            // Show progress bar if this script is currently selected
            if (
                ListView.SelectedItem >= 0
                && ListView.SelectedItem < scripts.Count
                && scripts[ListView.SelectedItem].Path == capturedScript.Path
            )
            {
                Application.Invoke(() =>
                {
                    Output.Text = ScriptOutputs[capturedScript.Path!];
                    Output.MoveEnd();
                    ProgressBar.Visible = true;
                });
            }

            var reattachTimer = Application.AddTimeout(
                TimeSpan.FromMilliseconds(30),
                () =>
                {
                    ProgressBar.Pulse();
                    return true;
                }
            );

            _ = Task.Run(async () =>
            {
                try
                {
                    await executor.Execute(
                        capturedScript,
                        line =>
                        {
                            Application.Invoke(() =>
                            {
                                ScriptOutputs[capturedScript.Path!] += line + "\n";
                                if (
                                    ListView.SelectedItem >= 0
                                    && ListView.SelectedItem < scripts.Count
                                    && scripts[ListView.SelectedItem].Path == capturedScript.Path
                                )
                                {
                                    Output.Text = ScriptOutputs[capturedScript.Path!];
                                    Output.MoveEnd();
                                }
                            });
                        },
                        cts.Token,
                        reattach: true
                    );
                }
                catch { }
                finally
                {
                    Application.RemoveTimeout(reattachTimer!);
                    ScriptStatus[capturedScript.Path!] = "stopped";
                    runningScripts.Remove(capturedScript.Path!);
                    cancellations.Remove(capturedScript.Path!);
                    Application.Invoke(() =>
                    {
                        RefreshDisplayNames(scripts, null);
                        if (
                            ListView.SelectedItem >= 0
                            && ListView.SelectedItem < scripts.Count
                            && scripts[ListView.SelectedItem].Path == capturedScript.Path
                        )
                        {
                            ProgressBar.Visible = false;
                            Output.Text = ScriptOutputs.TryGetValue(capturedScript.Path!, out var o)
                                ? o
                                : "";
                            Output.MoveEnd();
                        }
                    });
                }
            });
        }

        ListView.OpenSelectedItem += async (sender, e) =>
        {
            var selected = scripts[ListView.SelectedItem];

            if (runningScripts.Contains(selected.Path!))
            {
                var stop = confirmationDialog.Show(
                    "Already running",
                    "Do you want to stop the Script?"
                );
                if (stop)
                {
                    executor.Kill(selected);
                    if (cancellations.TryGetValue(selected.Path!, out var existingCts))
                        existingCts.Cancel();
                    ScriptStatus[selected.Path!] = "stopped";
                    runningScripts.Remove(selected.Path!);
                    cancellations.Remove(selected.Path!);
                    Application.Invoke(() =>
                    {
                        RefreshDisplayNames(scripts, null);
                        if (
                            ListView.SelectedItem >= 0
                            && ListView.SelectedItem < scripts.Count
                            && scripts[ListView.SelectedItem].Path == selected.Path
                            && ScriptOutputs.TryGetValue(selected.Path!, out var lastOutput)
                        )
                        {
                            Output.Text = lastOutput;
                            Output.MoveEnd();
                            ProgressBar.Visible = false;
                        }
                    });
                }
                return;
            }

            var run = confirmationDialog.Show("Run Script", $"Run {selected.Name}?");
            Application.Invoke(() => ListView.SetFocus());
            if (run)
            {
                ScriptOutputs[selected.Path!] = "";
                Output.Text = "";
                ScriptStatus.Remove(selected.Path!);

                var cts = new CancellationTokenSource();
                cancellations[selected.Path!] = cts;
                runningScripts.Add(selected.Path!);
                Application.Invoke(() => RefreshDisplayNames(scripts, null));
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
                                if (
                                    ListView.SelectedItem >= 0
                                    && ListView.SelectedItem < scripts.Count
                                    && scripts[ListView.SelectedItem].Path == selected.Path
                                )
                                {
                                    Output.Text = ScriptOutputs[selected.Path!];
                                    Output.MoveEnd();
                                }
                            });
                        },
                        cts.Token
                    );
                    if (
                        ListView.SelectedItem >= 0
                        && ListView.SelectedItem < scripts.Count
                        && scripts[ListView.SelectedItem].Path == selected.Path
                        && ScriptOutputs.TryGetValue(selected.Path!, out var lastOutput)
                    )
                    {
                        Output.Text = lastOutput;
                        Output.MoveEnd();
                    }
                }
                catch (OperationCanceledException)
                {
                    ScriptStatus[selected.Path!] = "stopped";
                }
                catch (Exception ex)
                {
                    Application.Invoke(() => messageDialog.Show("Error", ex.Message));
                    ScriptStatus[selected.Path!] = "error";
                }

                if (!ScriptStatus.ContainsKey(selected.Path!))
                    ScriptStatus[selected.Path!] = "done";

                Application.RemoveTimeout(timer!);
                cancellations.Remove(selected.Path!);
                runningScripts.Remove(selected.Path!);
                Application.Invoke(() => RefreshDisplayNames(scripts, null));

                if (
                    ListView.SelectedItem >= 0
                    && ListView.SelectedItem < scripts.Count
                    && scripts[ListView.SelectedItem].Path == selected.Path
                )
                {
                    ProgressBar.Visible = false;
                }
            }
        };

        sidebar.Add(ListView);
        preview.Add(TextView);
        outputFrame.Add(Output, ProgressBar);
        Window.Add(sidebar, preview, outputFrame, StatusBar, EditBarEditing, EditBarSaved);
    }
}
