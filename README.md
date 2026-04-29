# Runbook

Runbook is a terminal-based script management tool. You can execute, edit, create, and delete scripts from a simple TUI interface.

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
2. Open a terminal (PowerShell or CMD) and run:
```powershell
.\Runbook.exe
```

## Scripts Location
Scripts are stored in `~/.scripts/runbook/`

## Keybinds

### Normal Mode
| Key     | Action |
|---------|--------|
| `Enter` | Run script |
| `E`     | Edit script |
| `C`     | Create script |
| `D`     | Delete script |
| `Esc`   | Quit |

### Edit Mode
| Key      | Action |
|----------|--------|
| `Ctrl+S` | Save |
| `Esc`    | Cancel |
