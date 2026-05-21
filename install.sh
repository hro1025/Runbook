#!/bin/bash

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m'

msg_info()    { echo -e "${YELLOW}  ⟳ $1...${NC}"; }
msg_ok()      { echo -e "${GREEN}  ✓ $1${NC}"; }
msg_err()     { echo -e "${RED}  ✗ $1${NC}"; exit 1; }
msg_skip()    { echo -e "${CYAN}  ↷ $1 (already installed)${NC}"; }
msg_section() { echo -e "\n${BLUE}  ── $1 ──${NC}"; }

# ── Detect distro ──────────────────────────────────────────────
if [ -f /etc/os-release ]; then
    . /etc/os-release
    DISTRO=$ID
fi

# ── Detect root vs user ────────────────────────────────────────
if [ "$EUID" -eq 0 ]; then
    BIN_DIR="/usr/local/bin"
    IS_ROOT=true
else
    BIN_DIR="$HOME/.local/bin"
    mkdir -p "$BIN_DIR"
    IS_ROOT=false
fi

# ── Banner ─────────────────────────────────────────────────────
echo -e "${BLUE}"
echo "  ██████╗ ██╗   ██╗███╗   ██╗██████╗  ██████╗  ██████╗ ██╗  ██╗"
echo "  ██╔══██╗██║   ██║████╗  ██║██╔══██╗██╔═══██╗██╔═══██╗██║ ██╔╝"
echo "  ██████╔╝██║   ██║██╔██╗ ██║██████╔╝██║   ██║██║   ██║█████╔╝ "
echo "  ██╔══██╗██║   ██║██║╚██╗██║██╔══██╗██║   ██║██║   ██║██╔═██╗ "
echo "  ██║  ██║╚██████╔╝██║ ╚████║██████╔╝╚██████╔╝╚██████╔╝██║  ██╗"
echo "  ╚═╝  ╚═╝ ╚═════╝ ╚═╝  ╚═══╝╚═════╝  ╚═════╝  ╚═════╝ ╚═╝  ╚═╝"
echo -e "${NC}"
echo -e "  Terminal-based script manager | https://github.com/hro1025/Runbook"
echo ""

# ── Config ─────────────────────────────────────────────────────
REPO="hro1025/Runbook"
LATEST=$(curl -fsSL "https://api.github.com/repos/$REPO/releases/latest" | grep '"tag_name"' | head -1 | cut -d'"' -f4)
LATEST=${LATEST:-v1.0.0}

# ── Helpers ────────────────────────────────────────────────────
has_systemd() {
    command -v systemctl &>/dev/null && pidof systemd &>/dev/null
}

pkg_install() {
    if $IS_ROOT; then
        "$@"
    else
        sudo "$@"
    fi
}

# Check and install a single package (apt)
apt_pkg() {
    local pkg=$1
    if dpkg -s "$pkg" &>/dev/null 2>&1; then
        msg_skip "$pkg"
    else
        msg_info "Installing $pkg"
        pkg_install apt-get install -y "$pkg" &>/dev/null
        msg_ok "Installed $pkg"
    fi
}

# Check and install a single package (pacman)
pacman_pkg() {
    local pkg=$1
    if pacman -Q "$pkg" &>/dev/null 2>&1; then
        msg_skip "$pkg"
    else
        msg_info "Installing $pkg"
        pkg_install pacman -S --noconfirm --needed "$pkg" &>/dev/null
        msg_ok "Installed $pkg"
    fi
}

# Check and install a single package (dnf)
dnf_pkg() {
    local pkg=$1
    if rpm -q "$pkg" &>/dev/null 2>&1; then
        msg_skip "$pkg"
    else
        msg_info "Installing $pkg"
        pkg_install dnf install -y "$pkg" &>/dev/null
        msg_ok "Installed $pkg"
    fi
}

# Check and install a single package (zypper)
zypper_pkg() {
    local pkg=$1
    if rpm -q "$pkg" &>/dev/null 2>&1; then
        msg_skip "$pkg"
    else
        msg_info "Installing $pkg"
        pkg_install zypper install -y "$pkg" &>/dev/null
        msg_ok "Installed $pkg"
    fi
}

# Check and install a single package (apk)
apk_pkg() {
    local pkg=$1
    if apk info -e "$pkg" &>/dev/null 2>&1; then
        msg_skip "$pkg"
    else
        msg_info "Installing $pkg"
        pkg_install apk add --no-cache "$pkg" &>/dev/null
        msg_ok "Installed $pkg"
    fi
}

# ── Installers ─────────────────────────────────────────────────
install_dotnet() {
    msg_section "dotnet"
    if command -v dotnet &>/dev/null; then
        msg_skip "dotnet ($(dotnet --version))"
    else
        msg_info "Installing dotnet 10"
        curl -fsSL https://dot.net/v1/dotnet-install.sh | bash -s -- --channel 10.0 &>/dev/null
        export DOTNET_ROOT="$HOME/.dotnet"
        export PATH="$PATH:$HOME/.dotnet:$HOME/.dotnet/tools"
        echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc
        echo 'export PATH=$PATH:$HOME/.dotnet:$HOME/.dotnet/tools' >> ~/.bashrc
        msg_ok "Installed dotnet 10"
    fi

    if $HOME/.dotnet/dotnet tool list -g 2>/dev/null | grep -q dotnet-script; then
        msg_skip "dotnet-script"
    else
        msg_info "Installing dotnet-script"
        $HOME/.dotnet/dotnet tool install -g dotnet-script &>/dev/null || true
        msg_ok "Installed dotnet-script"
    fi
}

install_ttyd() {
    msg_section "ttyd"
    if command -v ttyd &>/dev/null; then
        msg_skip "ttyd ($(ttyd --version 2>&1 | head -1))"
    else
        msg_info "Installing ttyd"
        if has_systemd; then
            pkg_install systemctl stop runbook 2>/dev/null || true
            sleep 1
        fi
        curl -fsSL https://github.com/tsl0922/ttyd/releases/latest/download/ttyd.x86_64 -o "$BIN_DIR/ttyd"
        chmod +x "$BIN_DIR/ttyd"
        msg_ok "Installed ttyd"
    fi
}

install_runbook() {
    msg_section "Runbook"
    if [ -f "$BIN_DIR/Runbook" ]; then
        msg_info "Updating Runbook to $LATEST"
    else
        msg_info "Installing Runbook $LATEST"
    fi
    curl -L -o "$BIN_DIR/Runbook" "https://github.com/$REPO/releases/download/$LATEST/Runbook" || msg_err "Failed to download Runbook"
    chmod +x "$BIN_DIR/Runbook"
    msg_ok "Installed Runbook $LATEST"
}

install_service() {
    msg_section "Service"
    if $IS_ROOT && has_systemd; then
        msg_info "Creating systemd service"
        cat > /etc/systemd/system/runbook.service << EOF
[Unit]
Description=Runbook TUI Script Manager
After=network.target

[Service]
Environment=DOTNET_ROOT=/root/.dotnet
Environment=PATH=/root/.dotnet/tools:/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin
ExecStart=$BIN_DIR/ttyd --writable $BIN_DIR/Runbook
Restart=always
RestartSec=3

[Install]
WantedBy=multi-user.target
EOF
        systemctl daemon-reload
        systemctl enable -q --now runbook
        msg_ok "Created systemd service"
    else
        if ! echo "$PATH" | grep -q "$BIN_DIR"; then
            echo "export PATH=\"\$PATH:$BIN_DIR\"" >> ~/.bashrc
        fi
        msg_ok "Skipping service (no systemd or not root)"
        msg_ok "Run manually: ttyd --writable Runbook"
    fi
}

install_deps() {
    install_dotnet
    install_ttyd
    install_runbook
    install_service
}

# ── Distro detection & package install ────────────────────────
msg_section "System"
msg_info "Detecting distro"

case $DISTRO in
    ubuntu|debian|linuxmint|pop|elementary|zorin|kali|raspbian)
        msg_ok "Detected $DISTRO"
        msg_section "Dependencies"
        pkg_install apt-get update -y &>/dev/null
        for pkg in curl wget git python3 python3-pip; do apt_pkg "$pkg"; done
        install_deps
        ;;
    arch|manjaro|endeavouros|garuda|artix)
        msg_ok "Detected $DISTRO"
        msg_section "Dependencies"
        for pkg in curl wget git python; do pacman_pkg "$pkg"; done
        install_deps
        ;;
    fedora)
        msg_ok "Detected $DISTRO"
        msg_section "Dependencies"
        for pkg in curl wget git python3 python3-pip; do dnf_pkg "$pkg"; done
        install_deps
        ;;
    rhel|centos|almalinux|rocky)
        msg_ok "Detected $DISTRO"
        msg_section "Dependencies"
        for pkg in curl wget git python3 python3-pip; do dnf_pkg "$pkg"; done
        install_deps
        ;;
    opensuse*|sles)
        msg_ok "Detected $DISTRO"
        msg_section "Dependencies"
        for pkg in curl wget git python3 python3-pip; do zypper_pkg "$pkg"; done
        install_deps
        ;;
    alpine)
        msg_ok "Detected $DISTRO"
        msg_section "Dependencies"
        for pkg in curl wget git python3 py3-pip bash; do apk_pkg "$pkg"; done
        install_deps
        ;;
    void)
        msg_ok "Detected $DISTRO"
        msg_section "Dependencies"
        msg_info "Installing dependencies"
        pkg_install xbps-install -Sy curl wget git python3 python3-pip &>/dev/null
        msg_ok "Installed dependencies"
        install_deps
        ;;
    gentoo)
        msg_ok "Detected $DISTRO"
        msg_section "Dependencies"
        msg_info "Installing dependencies"
        pkg_install emerge --ask=n net-misc/curl net-misc/wget dev-vcs/git dev-lang/python &>/dev/null
        msg_ok "Installed dependencies"
        install_deps
        ;;
    nixos)
        msg_err "NixOS is not supported. Install manually via nix-env or home-manager."
        ;;
    *)
        msg_ok "Unknown distro — detecting package manager"
        msg_section "Dependencies"
        if command -v apt-get &>/dev/null; then
            pkg_install apt-get update -y &>/dev/null
            for pkg in curl wget git python3 python3-pip; do apt_pkg "$pkg"; done
        elif command -v pacman &>/dev/null; then
            for pkg in curl wget git python; do pacman_pkg "$pkg"; done
        elif command -v dnf &>/dev/null; then
            for pkg in curl wget git python3 python3-pip; do dnf_pkg "$pkg"; done
        elif command -v zypper &>/dev/null; then
            for pkg in curl wget git python3 python3-pip; do zypper_pkg "$pkg"; done
        elif command -v apk &>/dev/null; then
            for pkg in curl wget git python3 py3-pip; do apk_pkg "$pkg"; done
        else
            msg_err "No supported package manager found"
        fi
        install_deps
        ;;
esac

# ── Done ───────────────────────────────────────────────────────
IP=$(hostname -I 2>/dev/null | awk '{print $1}')
echo ""
echo -e "${BLUE}  ── Done ──${NC}"
if $IS_ROOT && has_systemd && systemctl is-active runbook &>/dev/null 2>&1; then
    echo -e "${GREEN}  ✓ Runbook is running at http://$IP:7681${NC}"
else
    echo -e "${GREEN}  ✓ Runbook installed at $BIN_DIR/Runbook${NC}"
    echo -e "${YELLOW}  ⟳ Run with: ttyd --writable Runbook${NC}"
    echo -e "${YELLOW}  ⟳ Restart terminal or: source ~/.bashrc${NC}"
fi
echo ""
