# Runbook
Runbook is a terminal-based script manager. You can execute, edit, create, and delete scripts from a simple TUI interface. Multiple scripts can run simultaneously in the background.

## Installation

### Quick Install
```bash
curl -fsSL https://raw.githubusercontent.com/hro1025/Runbook/main/install.sh | bash
```

The install script will automatically install:
- **dotnet 10** — required to run Runbook
- **python3** — for running `.py` scripts
- **ttyd** — lets you access Runbook from a browser
- **git**, **curl**, **wget** — general dependencies
- **Runbook binary** — downloaded from the latest release
- **systemd service** — Runbook starts automatically on boot or login

Supports: Debian/Ubuntu, Arch, Fedora, RHEL/CentOS, openSUSE, Alpine, Void, Gentoo.

> **Note:** The script detects whether you are root or a regular user and installs accordingly. Root installs create a system-wide service (starts on boot). Non-root installs create a user-level service (starts on login). You may be prompted for your sudo password during installation.

### Manual Install
1. Download `Runbook` from the [releases page](https://github.com/hro1025/Runbook/releases)
2. Make it executable and run it:
```bash
chmod +x Runbook
./Runbook
```

## Browser Access
The install script sets up Runbook as a systemd service via ttyd, so it is accessible from any browser on your network.

After installation, Runbook is available at:
```
http://<your-server-ip>:7681
```

**Root install** — check and manage the system service:
```bash
systemctl status runbook
systemctl restart runbook
```

**Non-root install** — check and manage the user service:
```bash
systemctl --user status runbook
systemctl --user restart runbook
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
