#!/bin/bash
set -e

REPO="Real-Lamafps/Runbook"
LATEST=$(curl -fsSL "https://api.github.com/repos/$REPO/releases/latest" | grep '"tag_name"' | cut -d'"' -f4)
BINARY_URL="https://github.com/$REPO/releases/download/$LATEST/Runbook"

if [ -f /etc/os-release ]; then
	. /etc/os-release
	DISTRO=$ID
fi

echo "Detected: $DISTRO"

install_dotnet() {
	curl -fsSL https://dot.net/v1/dotnet-install.sh | bash -s -- --channel 10.0
	export DOTNET_ROOT="$HOME/.dotnet"
	export PATH="$PATH:$HOME/.dotnet:$HOME/.dotnet/tools"
	echo 'export DOTNET_ROOT=$HOME/.dotnet' >>~/.bashrc
	echo 'export PATH=$PATH:$HOME/.dotnet:$HOME/.dotnet/tools' >>~/.bashrc
}

install_ttyd() {
	curl -fsSL https://github.com/tsl0922/ttyd/releases/latest/download/ttyd.x86_64 -o /usr/local/bin/ttyd
	chmod +x /usr/local/bin/ttyd
}

install_common_deps() {
	install_dotnet
	install_ttyd
	curl -fsSL "$BINARY_URL" -o /usr/local/bin/Runbook
	chmod +x /usr/local/bin/Runbook
	echo "Runbook installed at /usr/local/bin/Runbook"
}

case $DISTRO in
ubuntu | debian | linuxmint | pop | elementary | zorin | kali | raspbian)
	apt update -y
	apt install -y curl wget git python3 python3-pip
	install_common_deps
	;;
arch | manjaro | endeavouros | garuda | artix)
	pacman -Sy --noconfirm curl wget git python python-pip
	install_common_deps
	;;
fedora)
	dnf install -y curl wget git python3 python3-pip
	install_common_deps
	;;
rhel | centos | almalinux | rocky)
	dnf install -y curl wget git python3 python3-pip || yum install -y curl wget git python3 python3-pip
	install_common_deps
	;;
opensuse* | sles)
	zypper install -y curl wget git python3 python3-pip
	install_common_deps
	;;
alpine)
	apk add --no-cache curl wget git python3 py3-pip bash
	install_common_deps
	;;
void)
	xbps-install -Sy curl wget git python3 python3-pip
	install_common_deps
	;;
gentoo)
	emerge --ask=n net-misc/curl net-misc/wget dev-vcs/git dev-lang/python
	install_common_deps
	;;
nixos)
	echo "NixOS detected — please install manually via nix-env or home-manager"
	exit 1
	;;
*)
	echo "Unknown distro: $DISTRO — attempting with available package manager..."
	if command -v apt &>/dev/null; then
		apt install -y curl wget git python3 python3-pip
	elif command -v pacman &>/dev/null; then
		pacman -Sy --noconfirm curl wget git python python-pip
	elif command -v dnf &>/dev/null; then
		dnf install -y curl wget git python3 python3-pip
	elif command -v zypper &>/dev/null; then
		zypper install -y curl wget git python3 python3-pip
	elif command -v apk &>/dev/null; then
		apk add --no-cache curl wget git python3 py3-pip
	else
		echo "No supported package manager found. Install dependencies manually."
		exit 1
	fi
	install_common_deps
	;;
esac

# Create systemd service
cat >/etc/systemd/system/runbook.service <<EOF
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
systemctl enable runbook
systemctl start runbook

IP=$(hostname -I | awk '{print $1}')
echo ""
echo "Done! Runbook is running at http://$IP:7681"
