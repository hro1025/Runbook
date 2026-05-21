using System.Runtime.InteropServices;
using Runbook.Interfaces;
using Runbook.Models;
using Terminal.Gui;

namespace Runbook.UI;

// Handles all keyboard input and wires up the application's keybinds
public class KeybindHandler(
    Dashboard dashboard,
    List<Script> scripts,
    IExecutor executor,
    ConfirmationDialog confirmDialog,
    NameDialog nameDialog,
    string scriptsPath,
    IScanner scanner,
    MessageDialog messageDialog
)
{
    private readonly Dashboard dashboard = dashboard;
    private readonly List<Script> scripts = scripts;
    private readonly IExecutor executor = executor;
    private readonly ConfirmationDialog confirmDialog = confirmDialog;
    private readonly NameDialog nameDialog = nameDialog;
    private readonly string scriptsPath = scriptsPath;
    private readonly IScanner scanner = scanner;
    private readonly MessageDialog messageDialog = messageDialog;

    private bool isEditing; // True when the user is editing a script
    private bool isDialogOpen; // True when a dialog is open, prevents keybind conflicts

    public void Register()
    {
        Application.KeyDown += async (sender, e) =>
        {
            // Esc: cancel editing or quit the application
            if (e.KeyCode == KeyCode.Esc && !isDialogOpen)
            {
                if (isEditing)
                {
                    var selected = scripts[dashboard.ListView.SelectedItem];
                    var diskContent = File.ReadAllText(selected.Path!);

                    // Only prompt if content has actually changed since last save
                    if (
                        dashboard.ScriptEdits.TryGetValue(selected.Path!, out var staged)
                        && staged != diskContent
                    )
                    {
                        isDialogOpen = true;
                        var confirmed = confirmDialog.Show(
                            "Unsaved Changes",
                            "Save before exiting?"
                        );
                        isDialogOpen = false;

                        if (confirmed)
                        {
                            File.WriteAllText(selected.Path!, staged);
                            dashboard.ScriptEdits.Remove(selected.Path!);
                        }
                        else
                        {
                            dashboard.ScriptEdits.Remove(selected.Path!);
                            dashboard.TextView.Text = diskContent;
                        }
                    }

                    isEditing = false;
                    dashboard.EditBarEditing.Visible = false;
                    dashboard.TextView.ReadOnly = true;
                    dashboard.ListView.SetFocus();
                    dashboard.RefreshDisplayNames(scripts, null);
                    e.Handled = true;
                    return;
                }

                isDialogOpen = true;
                var runningCount = scripts.Count(s => executor.IsRunning(s));
                var quitMessage =
                    runningCount > 0
                        ? $"{runningCount} script(s) will keep running."
                        : "Exit Runbook?";
                var confirmed2 = confirmDialog.Show("Quit Runbook", quitMessage);
                isDialogOpen = false;
                if (confirmed2)
                    Application.RequestStop();
                e.Handled = true;
            }

            // E: enter edit mode for the selected script
            if (e.KeyCode == KeyCode.E && !isEditing && !isDialogOpen)
            {
                var selected = scripts[dashboard.ListView.SelectedItem];
                isEditing = true;
                dashboard.EditBarEditing.Visible = true;
                dashboard.TextView.ReadOnly = false;
                dashboard.TextView.SetFocus();
                dashboard.RefreshDisplayNames(scripts, selected.Path);
                e.Handled = true;
            }

            // C: create a new script, set permissions, and open it in edit mode
            if (e.KeyCode == KeyCode.C && !isEditing && !isDialogOpen)
            {
                isDialogOpen = true;
                var name = nameDialog.Show("New Script");
                isDialogOpen = false;

                if (name != null)
                {
                    var ext = Path.GetExtension(name);
                    string? template = ext switch
                    {
                        ".sh" => "#!/bin/bash\n",
                        ".py" => "#!/usr/bin/env python3\n",
                        ".csx" => "#!/usr/bin/env dotnet-script\n",
                        _ => null,
                    };

                    if (template == null)
                    {
                        messageDialog.Show(
                            "Unsupported type",
                            "Please use one of the supported script types\n.sh, .py, .csx"
                        );
                        e.Handled = true;
                        return;
                    }

                    var path = Path.Combine(scriptsPath, name);
                    File.WriteAllText(path, template);

                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        File.SetUnixFileMode(
                            path,
                            UnixFileMode.UserRead
                                | UnixFileMode.UserWrite
                                | UnixFileMode.UserExecute
                                | UnixFileMode.GroupRead
                                | UnixFileMode.GroupExecute
                                | UnixFileMode.OtherRead
                                | UnixFileMode.OtherExecute
                        );
                    }

                    ReloadScripts();

                    var newIndex = scripts.FindIndex(s => s.Path == path);
                    if (newIndex >= 0)
                    {
                        dashboard.ListView.SelectedItem = newIndex;
                        isEditing = true;
                        dashboard.EditBarEditing.Visible = true;
                        dashboard.TextView.ReadOnly = false;
                        dashboard.TextView.SetFocus();
                        dashboard.RefreshDisplayNames(scripts, path);
                    }
                }

                e.Handled = true;
            }

            // D: delete the selected script after confirmation
            if (e.KeyCode == KeyCode.D && !isEditing && dashboard.ListView.HasFocus)
            {
                var selected = scripts[dashboard.ListView.SelectedItem];
                isDialogOpen = true;
                var confirmed = confirmDialog.Show("Delete", $"Delete {selected.Name}?");
                isDialogOpen = false;

                if (confirmed)
                {
                    File.Delete(selected.Path!);
                    ReloadScripts();
                    dashboard.ListView.SelectedItem = 0;
                }

                e.Handled = true;
            }

            // Ctrl+S: save the current script and briefly show the saved indicator
            if (e.KeyCode == (KeyCode.S | KeyCode.CtrlMask) && isEditing)
            {
                var selected = scripts[dashboard.ListView.SelectedItem];
                File.WriteAllText(selected.Path!, dashboard.TextView.Text);
                dashboard.ScriptEdits.Remove(selected.Path!);
                dashboard.EditBarSaved.Visible = true;

                Application.AddTimeout(
                    TimeSpan.FromSeconds(1),
                    () =>
                    {
                        dashboard.EditBarSaved.Visible = false;
                        return false;
                    }
                );
                e.Handled = true;
            }
        };
    }

    // Rescans the scripts folder and refreshes the sidebar list
    private void ReloadScripts()
    {
        var newScripts = scanner.Scan(scriptsPath);
        scripts.Clear();
        scripts.AddRange(newScripts);
        dashboard.RefreshDisplayNames(scripts, null);
    }
}
