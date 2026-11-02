# Evidências — Fase 7, item 1: rede, DNS e DHCP

Ambiente: Docker Compose em `Fase7/infra/` (`docker compose up -d --build`), rede `vinheria-net`
com subnet fixa `172.28.0.0/24` e IP fixo por serviço. Host: Windows 11 + Docker Desktop 29.9.2.
Coleta em 02/11/2026.

## 1. Rede criada (subnet fixa + IP por serviço)

```
$ docker network inspect vinheria-net
subnet 172.28.0.0/24 gateway 172.28.0.1
vinheria-catalogo  172.28.0.10/24
vinheria-pedidos  172.28.0.11/24
vinheria-dhcp-cliente  172.28.0.99/24
vinheria-cliente  172.28.0.100/24
vinheria-dns  172.28.0.53/24
```

## 2. DNS interno: pesquisa direta, reversa e alias (CNAME)

```
$ docker compose exec cliente nslookup catalogo.vinheria.local 172.28.0.53
Server:		172.28.0.53
Address:	172.28.0.53#53

Name:	catalogo.vinheria.local
Address: 172.28.0.10

$ docker compose exec cliente nslookup 172.28.0.11 172.28.0.53   # pesquisa reversa (PTR)
11.0.28.172.in-addr.arpa	name = pedidos.vinheria.local.

$ docker compose exec cliente nslookup www.vinheria.local 172.28.0.53  # CNAME
Server:		172.28.0.53
Address:	172.28.0.53#53

www.vinheria.local	canonical name = catalogo.vinheria.local.
Name:	catalogo.vinheria.local
Address: 172.28.0.10

$ docker compose exec cliente getent hosts catalogo.vinheria.local pedidos.vinheria.local
172.28.0.10       catalogo.vinheria.local  catalogo.vinheria.local
172.28.0.11       pedidos.vinheria.local  pedidos.vinheria.local
```

## 3. Comunicação por nome: Pedidos → Catálogo (sem IP no código)

```
$ docker compose exec cliente curl -s http://pedidos.vinheria.local:8080/pedidos
{"servico":"pedidos","versao":"1.0.0","host":"pedidos","catalogo":{"url":"http://catalogo.vinheria.local:8080","resolvido_por_dns":"172.28.0.10","vinhos_disponiveis":4},"total":2,"itens":[{"pedido":1001,"cliente":"Giulio Agnello","vinhoId":1,"quantidade":2,"status":"confirmado"},{"pedido":1002,"cliente":"Bianca Agnello","vinhoId":3,"quantidade":1,"status":"separando"}]}
```

O campo `resolvido_por_dns` é o IP que o serviço de Pedidos obteve resolvendo
`catalogo.vinheria.local` — a chamada acontece pelo nome.

## 4. DHCP: concessão real com reserva por MAC

```
$ docker compose exec dhcp-cliente /scripts/lease.sh
== 1. MAC do container (tem reserva no DHCP) ==
link/ether 02:42:ac:1c:00:96

== 2. Removendo o IP estático do Docker para ficar como cliente DHCP ==

== 3. Solicitando concessão ao servidor DHCP (DORA) ==
udhcpc: started, v1.36.1
udhcpc: broadcasting discover
udhcpc: broadcasting select for 172.28.0.150, server 172.28.0.53
udhcpc: lease of 172.28.0.150 obtained from 172.28.0.53, lease time 43200
LEASE CONCEDIDA: ip=172.28.0.150 mascara=24 gateway=172.28.0.1 dns=172.28.0.53 dominio=vinheria.local lease=43200s servidor_dhcp=172.28.0.53

== 4. Configuração de rede recebida ==
2: eth0@if52: <BROADCAST,MULTICAST,UP,LOWER_UP> mtu 1500 qdisc noqueue state UP group default
    inet 172.28.0.150/24 scope global eth0
       valid_lft forever preferred_lft forever
default via 172.28.0.1 dev eth0
172.28.0.0/24 dev eth0 proto kernel scope link src 172.28.0.150
nameserver 172.28.0.53
```

Lease registrado do lado do servidor:

```
$ docker compose exec dns cat /var/lib/misc/dnsmasq.leases
1793673555 02:42:ac:1c:00:96 172.28.0.150 dhcp-cliente 01:02:42:ac:1c:00:96
```

Troca DORA no log do servidor (172.28.0.150 é o IP da reserva em `infra/dhcp/dhcp-hosts.conf`):

```
$ docker logs vinheria-dns | grep -E 'DHCP(OFFER|REQUEST|ACK)|broadcast response'
dnsmasq-dhcp[1]: 1666301516 DHCPOFFER(eth0) 172.28.0.150 02:42:ac:1c:00:96
dnsmasq-dhcp[1]: 1666301516 broadcast response
dnsmasq-dhcp[1]: 1666301516 DHCPREQUEST(eth0) 172.28.0.150 02:42:ac:1c:00:96
dnsmasq-dhcp[1]: 1666301516 DHCPACK(eth0) 172.28.0.150 02:42:ac:1c:00:96 dhcp-cliente
dnsmasq-dhcp[1]: 1666301516 broadcast response
```

## Configuração usada

- Escopo: `172.28.0.100`–`172.28.0.200`, máscara `255.255.255.0`, concessão de 12h
  (`infra/dns/dnsmasq.conf`).
- Opções entregues ao cliente: gateway `172.28.0.1`, DNS `172.28.0.53`, domínio `vinheria.local`.
- Reserva: MAC `02:42:ac:1c:00:96` → `172.28.0.150` / `dhcp-cliente.vinheria.local`
  (`infra/dhcp/dhcp-hosts.conf`).
- `dhcp-broadcast` no dnsmasq: sem isso o ACK sai em unicast para um IP que o cliente ainda não tem e a
  concessão nunca fecha.
