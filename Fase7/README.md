# Fase 7 — Vinheria Agnello

Entregáveis das atividades da fase 7:

| Item | Atividade | Onde está |
|---|---|---|
| 1 | Serviços I — servidor DHCP e DNS | `infra/` (rede Docker, dnsmasq com DHCP + DNS) |
| 2 | DevOps — repositório + pipeline Jenkins | `services/` (esqueleto dos 2 microsserviços), `jenkins/` (imagem, `Jenkinsfile`, pasta de publicação) |

## Os dois microsserviços simulados

Decisão do grupo (27/10): **Catálogo de Vinhos** e **Pedidos**, escolhidos no estudo de
microsserviços da fase 6. Ambos são APIs mínimas em .NET 10, sem dependência externa.

- `catalogo.vinheria.local:8080` — `GET /vinhos`, `GET /health`
- `pedidos.vinheria.local:8080` — `GET /pedidos` (consulta o Catálogo **pelo nome DNS**), `GET /health`

## Item 1 — rede, DNS e DHCP

Tudo em Docker Compose (`infra/docker-compose.yml`): rede própria `vinheria-net` com
subnet fixa `172.28.0.0/24` e IP fixo por serviço.

| Serviço | IP | Papel |
|---|---|---|
| `dns` | 172.28.0.53 | dnsmasq: zona `vinheria.local` (A, CNAME, PTR) + servidor DHCP |
| `catalogo` | 172.28.0.10 | microsserviço de Catálogo |
| `pedidos` | 172.28.0.11 | microsserviço de Pedidos |
| `cliente` | DHCP | container de testes (nslookup, getent, curl) |

```bash
cd infra
docker compose up -d --build
docker compose ps
```

Provas (rodam dentro da rede, por nome):

```bash
# DNS direto e reverso
docker compose exec cliente nslookup catalogo.vinheria.local
docker compose exec cliente nslookup 172.28.0.11
docker compose exec cliente getent hosts catalogo.vinheria.local

# Pedidos chamando o Catálogo pelo nome (não pelo IP)
docker compose exec cliente curl -s http://pedidos.vinheria.local:8080/pedidos

# DHCP: concessão real (DORA) + leases do servidor
docker compose exec dhcp-cliente /scripts/lease.sh
docker compose exec dns cat /var/lib/misc/dnsmasq.leases
```

Escopo DHCP: `172.28.0.100`–`172.28.0.200`, concessão de 12h, gateway `172.28.0.1`,
DNS `172.28.0.53`; reserva fixa de `172.28.0.150` para o MAC do container `dhcp-cliente`
(`infra/dhcp/dhcp-hosts.conf`). Saída completa das provas: `evidencias/item1-rede-dns-dhcp.md`.

## Item 2 — Jenkins: build e deploy

```bash
cd jenkins
docker compose up -d --build      # Jenkins LTS + .NET SDK 10, porta 8080
```

Pipeline (`jenkins/Jenkinsfile`, versionado): stage **Build** (`echo "Deploy da Vinheria"` +
`dotnet publish` dos dois serviços) e stage **Deploy** (copia o artefato para a pasta de
publicação, com `BUILD_INFO.txt` contendo build, data e hora).

- **Pasta de publicação:** `Fase7/jenkins/publicacao/` no host
  (`/var/jenkins_home/publicacao` dentro do container).
- Estrutura publicada: `publicacao/<serviço>/` com os DLLs prontos (`dotnet <serviço>.dll`).
- Cada build grava `publicacao/<serviço>/BUILD_INFO.txt` com número do build, commit e
  timestamp — é a prova de que o deploy rodou.

Evidências: `evidencias/item2-jenkins.md`.

## Evidências (execução real em 02/11/2026)

- `evidencias/item1-rede-dns-dhcp.md` — rede, DNS (direto/reverso/CNAME), concessão DHCP e **reserva por
  serviço** (catálogo, pedidos e cliente de teste)
- `evidencias/item2-jenkins.md` — dois builds verdes, console e artefato publicado
- `evidencias/jenkins-historico-builds.png` e `evidencias/jenkins-console-build2.png` — prints do Jenkins
- `evidencias/prints/` — prints de cada etapa: `rede.png`, `dns.png`, `dhcp.png`, `chamada.png`, `publicacao.png`
