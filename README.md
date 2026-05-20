# Runbook
Runbook is a terminal-based script manager. You can execute, edit, create, and delete scripts from a simple TUI interface. Multiple scripts can run simultaneously in the background.

## Installation

### Quick Install (Linux)
```bash
curl -fsSL https://raw.githubusercontent.com/Real-Lamafps/Runbook/main/install.sh | bash
```

The install script will automatically install:
- **dotnet 10** — required to run Runbook
- **python3** — for running `.py` scripts
- **ttyd** — lets you access Runbook from a browser
- **git**, **curl**, **wget** — general dependencies
- **Runbook binary** — downloaded from the latest release

Supports: Debian/Ubuntu, Arch, Fedora, RHEL/CentOS, openSUSE, Alpine, Void, Gentoo.

### Manual Install
1. Download `Runbook` from the [releases page](https://github.com/Real-Lamafps/Runbook/releases)
2. Make it executable and run it:
```bash
chmod +x Runbook
./Runbook
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
Scripts are stored in `~/.scripts/runbook/`.

## Keybinds

### Normal Mode
| Key | Action |
|-----|--------|
| `Enter` | Run script |
| `E` | Edit script |
| `C` | Create script |
| `D` | Delete script |
| `Esc` | Quit |

## Browser Access
If you installed via the install script, ttyd is included. You can run Runbook in the browser:
```bash
ttyd --writable Runbook
```
Then visit `http://localhost:7681`.
