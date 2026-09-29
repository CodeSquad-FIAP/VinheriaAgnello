# Fase 6 — Arquitetura de APIs e Microsserviços
## Item 2 — Diagrama de Arquitetura

| Campo | Valor |
|---|---|
| Arquivos | `P2_arquitetura.drawio` (editável) · `P2_arquitetura.png` · `P2_arquitetura.svg` |
| Responsável | Kevin Benevides da Silva Romariz — RM 557898 |
| Período | 22/09/2026 → 28/09/2026 |
| Fonte dos nomes | Glossário do P1 (item 6) e lista de tópicos do P5 |

![Arquitetura de microsserviços da Vinheria Agnello](P2_arquitetura.png)

> No `.docx`, inserir o `P2_arquitetura.svg`: é vetorial (permite zoom sem perder nitidez) e tem o texto convertido em curvas, então o Word não depende de fonte instalada. O `.png` (3992 × 3126) serve para chat e slides.

## 2.1 Como ler o diagrama

O desenho é lido da esquerda para a direita. Os **clientes** — web (páginas JSP atuais), app mobile (Android/Room) e painel interno da adega — só entram no sistema pelo **API Gateway**. Ele roteia por prefixo (`/api/<dominio>` → `ms-<dominio>`), valida o JWT emitido pelo `ms-identidade` (Keycloak, ADR-002), aplica o *rate limit* com contadores no Redis e faz a agregação (BFF) do app mobile. Dentro da **malha de serviços**, toda chamada trafega com mTLS.

Os **12 serviços** da lista congelada do P1 aparecem com os mesmos nomes: 9 de núcleo e 3 satélites de plataforma. Cada um tem **seu próprio banco** (Database per Service): PostgreSQL na maioria, MongoDB no `ms-catalogo`, TimescaleDB (série temporal) no `ms-qualidade` e um read-model PostgreSQL no `ms-analytics`. Nenhuma linha liga um serviço ao banco de outro.

A camada assíncrona é o **Apache Kafka**. A tabela dentro do bloco lista cada tópico com seu produtor e seus consumidores. O fluxo de compra segue a saga do P5: `pedido.criado` → `estoque.reservado` → `pagamento.aprovado` ou `pagamento.recusado` → confirmação ou cancelamento → `notificacao.enviar`.

O **fluxo IoT** aparece à esquerda, embaixo: sensor Arduino → ponte serial → Node-RED → broker MQTT (TLS, usuário por adega, ADR-003) → *bridge* MQTT → Kafka, que publica `qualidade.leitura`. O `ms-qualidade` consome a leitura e publica `qualidade.alerta` e `notificacao.enviar`.

A **área de migração** mostra o *Strangler Fig*: o monolito atual (Servlet/JSP + API .NET de estoque, com SQLite compartilhado) fica atrás do gateway, que continua mandando para ele as rotas ainda não extraídas. O primeiro domínio extraído é o estoque, porque a API .NET já é o embrião do `ms-estoque` (P1, item 4). Catálogo e clientes vêm em seguida.

## 2.2 Legenda

| Traço | Significado |
|---|---|
| Linha contínua grossa, azul | Síncrono — REST/HTTPS (dentro da malha com mTLS) |
| Linha tracejada longa, vermelho-tijolo | Assíncrono — evento publicado/consumido no Kafka |
| Linha pontilhada, verde | Telemetria IoT — serial e MQTT sobre TLS |
| Traço-ponto, cinza, seta aberta | Migração Strangler Fig (monolito → serviço) |
| Linha fina sem seta | Serviço ↔ banco próprio (acesso exclusivo) |

Os tipos de comunicação se distinguem pelo **padrão do traço**, não só pela cor, então a figura continua legível em impressão preto e branco.

## 2.3 Conferência dos critérios de aceite

| Critério | Situação |
|---|---|
| Todo microsserviço do P1 aparece; nenhum serviço inventado | 12 de 12, com os nomes do glossário. Redis, registry, mesh, observabilidade, Vault e a *bridge* MQTT → Kafka são infraestrutura, não serviços de domínio (mesmo critério do P1, item 3, para o Node-RED) |
| Síncrono × assíncrono distinguíveis pela legenda | Sim: traço contínuo × tracejado, além da cor |
| Nenhum acesso direto ao banco de outro serviço; cliente só via gateway | Sim: cada banco liga a um único serviço; os três clientes apontam apenas para o gateway |
| Nomes dos tópicos iguais aos do texto | Os 9 tópicos do P5 (`pedido.criado`, `estoque.reservado`, `pagamento.aprovado`, `pagamento.recusado`, `lote.criado`, `qualidade.leitura`, `qualidade.alerta`, `notificacao.enviar`, `cliente.anonimizado`) + `recebimento.confirmado`, marcado como proposto |
| Legível impresso (fonte ≥ 12 no tamanho final) | Ver item 2.4 |

## 2.4 Pontos a confirmar com o grupo

- **`recebimento.confirmado`** — aparece no diagrama com asterisco, como proposta do P1 (item 7). Se o P4 mudar o nome, ajustar a linha da tabela no `.drawio` e reexportar.
- **Produtores e consumidores por tópico** — a tabela é a proposta desta parte. Principais escolhas: o `ms-notificacoes` consome apenas `notificacao.enviar` (contrato de entrada único), e `qualidade.alerta` vai para estoque, produção e analytics. O Arthur (P4) deve usar a mesma tabela em "quem fala com quem"; se discordar de alguma linha, avisar antes de 30/09.
- **Tamanho de impressão (critério ainda não atendido)** — a figura inteira numa página não chega a 12 pt. Em A4 paisagem (margens de 2 cm), os nomes dos serviços ficam com cerca de 7 pt e as notas com 4–5 pt. Em A3 paisagem, ficam com cerca de 10 pt e 7 pt. Para cumprir o critério, a proposta é manter esta figura como **visão geral** (com o SVG permitindo zoom) e acrescentar duas figuras de detalhe em página própria: (a) clientes, gateway, serviços e bancos; (b) tabela de tópicos e fluxo IoT. A decisão fica com o grupo na consolidação (P5, parte 2).

## Como editar e reexportar

Abrir `P2_arquitetura.drawio` no draw.io (desktop ou app.diagrams.net). Para o `.docx`, **não** usar o "Exportar como SVG" padrão: ele grava o texto em `foreignObject` (HTML), que o Word não renderiza ("Text is not SVG - cannot display"). Gerar o SVG a partir do PDF:

```bash
drawio -x -f png --theme light -b 10 -s 2 -o P2_arquitetura.png P2_arquitetura.drawio
drawio -x -f pdf --theme light -b 10 -o P2_arquitetura.pdf P2_arquitetura.drawio
pdftocairo -svg P2_arquitetura.pdf P2_arquitetura.svg && rm P2_arquitetura.pdf
```
