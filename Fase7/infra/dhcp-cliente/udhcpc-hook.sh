#!/bin/sh
# hook do udhcpc: aplica a concessão recebida e imprime os parâmetros do DHCP
case "$1" in
  deconfig)
    ip addr flush dev "$interface"
    ;;
  bound|renew)
    ip addr add "$ip/$mask" dev "$interface"
    [ -n "$router" ] && ip route add default via "$router" 2>/dev/null
    [ -n "$dns" ] && printf 'nameserver %s\n' $dns > /etc/resolv.conf
    echo "LEASE CONCEDIDA: ip=$ip mascara=$mask gateway=$router dns=$dns dominio=$domain lease=${lease}s servidor_dhcp=$serverid"
    ;;
esac
