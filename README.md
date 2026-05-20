To check the service status:
```bash
systemctl status runbook
```

To restart it:
```bash
systemctl restart runbook
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
