#!/bin/bash
set -e

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
BINARY_URL="https://github.com/$REPO/releases/download/$LATEST/Runbook"

install_dotnet() {
    msg_info "Installing dotnet 10"
    curl -fsSL https://dot.net/v1/dotnet-install.sh | bash -s -- --channel 10.0 &>/dev/null
    export DOTNET_ROOT="$HOME/.dotnet"
    export PATH="$PATH:$HOME/.dotnet:$HOME/.dotnet/tools"
    echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc
    echo 'export PATH=$PATH:$HOME/.dotnet:$HOME/.dotnet/tools' >> ~/.bashrc
    msg_ok "Installed dotnet 10"
}

install_ttyd() {
    msg_info "Installing ttyd"
    curl -fsSL https://github.com/tsl0922/ttyd/releases/latest/download/ttyd.x86_64 -o /usr/local/bin/ttyd
    chmod +x /usr/local/bin/ttyd
    msg_ok "Installed ttyd"
}

install_runbook() {
    msg_info "Installing Runbook $LATEST"
    curl -fsSL "$BINARY_URL" -o /usr/local/bin/Runbook || msg_err "Failed to download Runbook"
    chmod +x /usr/local/bin/Runbook
    msg_ok "Installed Runbook $LATEST"
}

install_service() {
    msg_info "Creating systemd service"
    cat > /etc/systemd/system/runbook.service << EOF
[Unit]
Description=Runbook TUI Script Manager
After=network.target

[Service]
ExecStart=/usr/local/bin/ttyd --writable /usr/local/bin/Runbook
Restart=always
RestartSec=3

[Install]
WantedBy=multi-user.target
EOF
    systemctl daemon-reload
    systemctl enable -q --now runbook
    msg_ok "Created service"
}

install_deps() {
    install_dotnet
    install_ttyd
    install_runbook
    install_service
}

msg_info "Detecting system"
case $DISTRO in
    ubuntu|debian|linuxmint|pop|elementary|zorin|kali|raspbian)
        msg_ok "Detected $DISTRO"
        msg_info "Installing dependencies"
        apt update -y &>/dev/null
        apt install -y curl wget git python3 python3-pip &>/dev/null
        msg_ok "Installed dependencies"
        install_deps ;;
    arch|manjaro|endeavouros|garuda|artix)
        msg_ok "Detected $DISTRO"
        msg_info "Installing dependencies"
        pacman -Sy --noconfirm curl wget git python python-pip &>/dev/null
        msg_ok "Installed dependencies"
        install_deps ;;
    fedora)
        msg_ok "Detected $DISTRO"
        msg_info "Installing dependencies"
        dnf install -y curl wget git python3 python3-pip &>/dev/null
        msg_ok "Installed dependencies"
        install_deps ;;
    rhel|centos|almalinux|rocky)
        msg_ok "Detected $DISTRO"
        msg_info "Installing dependencies"
        dnf install -y curl wget git python3 python3-pip &>/dev/null || yum install -y curl wget git python3 python3-pip &>/dev/null
        msg_ok "Installed dependencies"
        install_deps ;;
    opensuse*|sles)
        msg_ok "Detected $DISTRO"
        msg_info "Installing dependencies"
        zypper install -y curl wget git python3 python3-pip &>/dev/null
        msg_ok "Installed dependencies"
        install_deps ;;
    alpine)
        msg_ok "Detected $DISTRO"
        msg_info "Installing dependencies"
        apk add --no-cache curl wget git python3 py3-pip bash &>/dev/null
        msg_ok "Installed dependencies"
        install_deps ;;
    void)
        msg_ok "Detected $DISTRO"
        msg_info "Installing dependencies"
        xbps-install -Sy curl wget git python3 python3-pip &>/dev/null
        msg_ok "Installed dependencies"
        install_deps ;;
    gentoo)
        msg_ok "Detected $DISTRO"
        msg_info "Installing dependencies"
        emerge --ask=n net-misc/curl net-misc/wget dev-vcs/git dev-lang/python &>/dev/null
        msg_ok "Installed dependencies"
        install_deps ;;
    nixos)
        msg_err "NixOS is not supported. Install manually via nix-env or home-manager." ;;
    *)
        msg_ok "Unknown distro — detecting package manager"
        if command -v apt &>/dev/null; then apt install -y curl wget git python3 python3-pip &>/dev/null
        elif command -v pacman &>/dev/null; then pacman -Sy --noconfirm curl wget git python python-pip &>/dev/null
        elif command -v dnf &>/dev/null; then dnf install -y curl wget git python3 python3-pip &>/dev/null
        elif command -v zypper &>/dev/null; then zypper install -y curl wget git python3 python3-pip &>/dev/null
        elif command -v apk &>/dev/null; then apk add --no-cache curl wget git python3 py3-pip &>/dev/null
        else msg_err "No supported package manager found"
        fi
        install_deps ;;
esac

IP=$(hostname -I | awk '{print $1}')
echo ""
echo -e "${GREEN}  Runbook is running at http://$IP:7681${NC}"
echo ""
