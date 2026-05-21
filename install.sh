#!/bin/bash

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

msg_info() { echo -e "${YELLOW}  ⟳ $1...${NC}"; }
msg_ok()   { echo -e "${GREEN}  ✓ $1${NC}"; }
msg_err()  { echo -e "${RED}  ✗ $1${NC}"; exit 1; }

# Detect distro
if [ -f /etc/os-release ]; then
    . /etc/os-release
    DISTRO=$ID
fi

# Detect root vs user
if [ "$EUID" -eq 0 ]; then
    BIN_DIR="/usr/local/bin"
    IS_ROOT=true
else
    BIN_DIR="$HOME/.local/bin"
    mkdir -p "$BIN_DIR"
    IS_ROOT=false
fi

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

REPO="hro1025/Runbook"
LATEST=$(curl -fsSL "https://api.github.com/repos/$REPO/releases/latest" | grep '"tag_name"' | head -1 | cut -d'"' -f4)
LATEST=${LATEST:-v1.0.0}

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

install_dotnet() {
    msg_info "Installing dotnet 10"
    curl -fsSL https://dot.net/v1/dotnet-install.sh | bash -s -- --channel 10.0 &>/dev/null
    export DOTNET_ROOT="$HOME/.dotnet"
    export PATH="$PATH:$HOME/.dotnet:$HOME/.dotnet/tools"
    echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc
    echo 'export PATH=$PATH:$HOME/.dotnet:$HOME/.dotnet/tools' >> ~/.bashrc
    msg_ok "Installed dotnet 10"
    msg_info "Installing dotnet-script"
    $HOME/.dotnet/dotnet tool install -g dotnet-script &>/dev/null || true
    msg_ok "Installed dotnet-script"
}

install_ttyd() {
    msg_info "Installing ttyd"
    if has_systemd; then
        pkg_install systemctl stop runbook 2>/dev/null || true
        sleep 1
    fi
    curl -fsSL https://github.com/tsl0922/ttyd/releases/latest/download/ttyd.x86_64 -o "$BIN_DIR/ttyd"
    chmod +x "$BIN_DIR/ttyd"
    msg_ok "Installed ttyd"
}

install_runbook() {
    msg_info "Installing Runbook $LATEST"
    curl -L -o "$BIN_DIR/Runbook" "https://github.com/$REPO/releases/download/$LATEST/Runbook" || msg_err "Failed to download Runbook"
    chmod +x "$BIN_DIR/Runbook"
    msg_ok "Installed Runbook $LATEST"
}

install_service() {
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
        msg_ok "Created service"
    else
        # Add to PATH if not root
        if ! echo "$PATH" | grep -q "$BIN_DIR"; then
            echo "export PATH=\"\$PATH:$BIN_DIR\"" >> ~/.bashrc
        fi
        msg_ok "No systemd or not root — run manually with: ttyd --writable Runbook"
    fi
}

install_deps() {
    install_dotnet
    install_ttyd
    install_runbook
    install_service
}

install_arch_deps() {
    msg_info "Installing dependencies"
    pkg_install pacman -Sy --noconfirm --needed curl wget git python
    msg_ok "Installed dependencies"
    install_deps
}

msg_info "Detecting system"
case $DISTRO in
    ubuntu|debian|linuxmint|pop|elementary|zorin|kali|raspbian)
        msg_ok "Detected $DISTRO"
        msg_info "Installing dependencies"
        pkg_install apt-get update -y &>/dev/null
        pkg_install apt-get install -y curl wget git python3 python3-pip &>/dev/null
        msg_ok "Installed dependencies"
        install_deps
        ;;
    arch|manjaro|endeavouros|garuda|artix)
        msg_ok "Detected $DISTRO"
        install_arch_deps
        ;;
    fedora)
        msg_ok "Detected $DISTRO"
        msg_info "Installing dependencies"
        pkg_install dnf install -y curl wget git python3 python3-pip &>/dev/null
        msg_ok "Installed dependencies"
        install_deps
        ;;
    rhel|centos|almalinux|rocky)
        msg_ok "Detected $DISTRO"
        msg_info "Installing dependencies"
        pkg_install dnf install -y curl wget git python3 python3-pip &>/dev/null || \
        pkg_install yum install -y curl wget git python3 python3-pip &>/dev/null
        msg_ok "Installed dependencies"
        install_deps
        ;;
    opensuse*|sles)
        msg_ok "Detected $DISTRO"
        msg_info "Installing dependencies"
        pkg_install zypper install -y curl wget git python3 python3-pip &>/dev/null
        msg_ok "Installed dependencies"
        install_deps
        ;;
    alpine)
        msg_ok "Detected $DISTRO"
        msg_info "Installing dependencies"
        pkg_install apk add --no-cache curl wget git python3 py3-pip bash &>/dev/null
        msg_ok "Installed dependencies"
        install_deps
        ;;
    void)
        msg_ok "Detected $DISTRO"
        msg_info "Installing dependencies"
        pkg_install xbps-install -Sy curl wget git python3 python3-pip &>/dev/null
        msg_ok "Installed dependencies"
        install_deps
        ;;
    gentoo)
        msg_ok "Detected $DISTRO"
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
        if command -v apt-get &>/dev/null; then
            pkg_install apt-get update -y &>/dev/null
            pkg_install apt-get install -y curl wget git python3 python3-pip &>/dev/null
        elif command -v pacman &>/dev/null; then
            pkg_install pacman -Sy --noconfirm --needed curl wget git python
        elif command -v dnf &>/dev/null; then
            pkg_install dnf install -y curl wget git python3 python3-pip &>/dev/null
        elif command -v zypper &>/dev/null; then
            pkg_install zypper install -y curl wget git python3 python3-pip &>/dev/null
        elif command -v apk &>/dev/null; then
            pkg_install apk add --no-cache curl wget git python3 py3-pip &>/dev/null
        else
            msg_err "No supported package manager found"
        fi
        install_deps
        ;;
esac

IP=$(hostname -I 2>/dev/null | awk '{print $1}')
echo ""
if $IS_ROOT && has_systemd && systemctl is-active runbook &>/dev/null 2>&1; then
    echo -e "${GREEN}  Runbook is running at http://$IP:7681${NC}"
else
    echo -e "${GREEN}  Runbook installed! Run with: ttyd --writable Runbook${NC}"
    echo -e "${YELLOW}  Note: restart your terminal or run: source ~/.bashrc${NC}"
fi
echo ""
