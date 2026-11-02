#!/bin/sh
# Prova do DHCP: o container descarta o IP fixo do Docker e pede endereço ao servidor
# (dnsmasq em 172.28.0.53). O MAC tem reserva -> deve receber 172.28.0.150.
set -e

echo "== 1. MAC do container (tem reserva no DHCP) =="
ip link show eth0 | grep -o 'link/ether [0-9a-f:]*'

echo
echo "== 2. Removendo o IP estático do Docker para ficar como cliente DHCP =="
ip addr flush dev eth0
ip link set eth0 up

echo
echo "== 3. Solicitando concessão ao servidor DHCP (DORA) =="
udhcpc -i eth0 -q -n -t 5 -T 3 -f -s /scripts/udhcpc-hook.sh

echo
echo "== 4. Configuração de rede recebida =="
ip -4 addr show eth0
ip route
cat /etc/resolv.conf

echo
echo "== 5. Registro do lease no servidor =="
echo "ver: docker compose exec dns cat /var/lib/misc/dnsmasq.leases"
