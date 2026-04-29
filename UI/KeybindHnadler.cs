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
    IScanner scanner
)
{
    private readonly Dashboard dashboard = dashboard;
    private readonly List<Script> scripts = scripts;
    private readonly IExecutor executor = executor;
    private readonly ConfirmationDialog confirmDialog = confirmDialog;
    private readonly NameDialog nameDialog = nameDialog;
    private readonly string scriptsPath = scriptsPath;
    private string originalText = "";
    private readonly IScanner scanner = scanner;
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
                    // If there are unsaved changes, ask before discarding
                    var currentText = dashboard.TextView.Text ?? "";
                    if (currentText != originalText)
                    {
                        isDialogOpen = true;
                        var confirmed = confirmDialog.Show("Unsaved Changes", "Discard changes?");
                        isDialogOpen = false;
                        if (!confirmed)
                        {
                            e.Handled = true;
                            return;
                        }
                    }

                    // Restore original file contents and exit edit mode
                    var selected = scripts[dashboard.ListView.SelectedItem];
                    dashboard.TextView.Text = File.ReadAllText(selected.Path!);
                    isEditing = false;
                    dashboard.EditBarEditing.Visible = false;
                    dashboard.TextView.ReadOnly = true;
                    dashboard.ListView.SetFocus();
                    e.Handled = true;
                    return;
                }

                // Not editing — confirm quit
                isDialogOpen = true;
                var confirmed2 = confirmDialog.Show("Quit", "Exit Runbook?");
                isDialogOpen = false;
                if (confirmed2)
                    Application.RequestStop();
                e.Handled = true;
            }

            // E: enter edit mode for the selected script
            if (e.KeyCode == KeyCode.E && !isEditing && dashboard.ListView.HasFocus)
            {
                isDialogOpen = true;
                var confirmed = confirmDialog.Show("Edit", "Edit the script?");
                isDialogOpen = false;
                if (confirmed)
                {
                    isEditing = true;
                    dashboard.EditBarEditing.Visible = true;
                    dashboard.TextView.ReadOnly = false;
                    dashboard.TextView.SetFocus();
                }
                e.Handled = true;
            }

            // C: create a new bash script, set permissions, and open it in edit mode
            if (e.KeyCode == KeyCode.C && !isEditing && dashboard.ListView.HasFocus)
            {
                isDialogOpen = true;
                var confirmed = confirmDialog.Show("Create", "Create new script?");
                isDialogOpen = false;

                if (confirmed)
                {
                    var name = nameDialog.Show("New Script");

                    if (name != null)
                    {
                        // Create the file with a bash shebang
                        var path = Path.Combine(scriptsPath, name + ".sh");
                        File.WriteAllText(path, "#!/bin/bash\n");

                        // Make the script executable on Linux/macOS
                        if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
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

                        // Select the new script and open it in edit mode
                        var newIndex = scripts.FindIndex(s => s.Path == path);
                        if (newIndex >= 0)
                        {
                            dashboard.ListView.SelectedItem = newIndex;
                            isEditing = true;
                            dashboard.EditBarEditing.Visible = true;
                            dashboard.TextView.ReadOnly = false;
                            dashboard.TextView.SetFocus();
                        }
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
                originalText = dashboard.TextView.Text ?? "";
                dashboard.EditBarSaved.Visible = true;

                // Hide the saved indicator after 1 second
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

        var newNames = newScripts.ConvertAll(s => s.Name ?? "");
        dashboard.ListView.SetSource(
            new System.Collections.ObjectModel.ObservableCollection<string>(newNames)
        );
    }
}
