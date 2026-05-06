# Runbook
Runbook is a terminal-based script manager. You can execute, edit, create, and delete scripts from a simple TUI interface. Multiple scripts can run simultaneously in the background.

## Installation

### Linux
1. Download `Runbook` from the [releases page](https://github.com/hro1025/Runbook/releases)
2. Make it executable and run it:
```bash
chmod +x Runbook
./Runbook
```

### Windows
1. Download `Runbook.exe` from the [releases page](https://github.com/hro1025/Runbook/releases)
2. Run it directly:
```powershell
./Runbook.exe
```

## Dependencies
Runbook supports three script types. The required runtime must be installed for each type:

| Script Type | Extension | Runtime Required |
|-------------|-----------|-----------------|
| Bash | `.sh` | Built-in on Linux |
| Python | `.py` | [Python](https://python.org) |
| C# Script | `.csx` | [dotnet-script](https://github.com/dotnet-script/dotnet-script) |

If a runtime is missing, Runbook will show an error when you try to run that script type.

## Scripts Location
Scripts are stored in `~/.scripts/runbook/` on Linux and `%USERPROFILE%\.scripts\runbook\` on Windows.

## Keybinds
### Normal Mode
| Key | Action |
|-----|--------|
| `Enter` | Run script |
| `E` | Edit script |
| `C` | Create script |
| `D` | Delete script |
| `Esc` | Quit |

### Edit Mode
| Key | Action |
|-----|--------|
| `Ctrl+S` | Save |
| `Esc` | Cancel |
