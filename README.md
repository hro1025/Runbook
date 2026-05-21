# Runbook
Runbook is a terminal-based script manager. You can execute, edit, create, and delete scripts from a simple TUI interface. Multiple scripts can run simultaneously in the background and keep running even after you close Runbook.
 
## Installation
 
### Quick Install
```bash
curl -fsSL https://raw.githubusercontent.com/hro1025/Runbook/main/install.sh -o /tmp/runbook_install.sh && bash /tmp/runbook_install.sh
```
 
During installation you will be asked whether you want browser access via ttyd. Choose yes for server setups, no for local terminal use.
 
The install script will automatically install:
- **dotnet 10** — required to run Runbook
- **dotnet-script** — for running `.csx` scripts
- **python3** — for running `.py` scripts
- **git**, **curl**, **wget** — general dependencies
- **Runbook binary** — downloaded from the latest release
- **ttyd** *(optional)* — lets you access Runbook from a browser
- **systemd service** *(optional)* — Runbook starts automatically on boot or login
Supports: Debian/Ubuntu, Arch, Fedora, openSUSE.
 
> **Note:** The script detects whether you are root or a regular user and installs accordingly. Root installs create a system-wide service (starts on boot). Non-root installs create a user-level service (starts on login). You may be prompted for your sudo password during installation.
 
### Manual Install
1. Download `Runbook` from the [releases page](https://github.com/hro1025/Runbook/releases)
2. Make it executable and run it:
```bash
chmod +x Runbook
./Runbook
```
 
## Usage
 
### Terminal
Run Runbook directly in your terminal:
```bash
Runbook
```
 
### Browser (server setup)
If you chose browser access during install, Runbook is accessible from any browser on your network via ttyd.
 
After installation, Runbook is available at:
```
http://<your-server-ip>:7681
```
 
**Root install** — manage the system service:
```bash
systemctl status runbook
systemctl restart runbook
```
 
**Non-root install** — manage the user service:
```bash
systemctl --user status runbook
systemctl --user restart runbook
```
 
## Background Scripts
Scripts run as detached background processes — they keep running even if you close Runbook or the browser tab. When you reopen Runbook, it automatically reattaches to any running scripts and streams their output.
 
To stop a running script, select it and press `Enter`, then confirm.
 
When quitting Runbook with running scripts, you will be notified how many scripts are still running in the background.
 
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
 
Log files and PID files are stored in:
- `~/.scripts/runbook/logs/`
- `~/.scripts/runbook/pids/`
## Keybinds
 
### Normal Mode
| Key | Action |
|-----|--------|
| `Enter` | Run / Stop script |
| `E` | Edit script |
| `C` | Create script |
| `D` | Delete script |
| `Esc` | Quit |
 

