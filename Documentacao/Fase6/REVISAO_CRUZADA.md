# Fase 6 — Rodada de revisão cruzada (11/10 a 13/10/2026)

Cada integrante revisa **a seção de um colega** no `.docx` consolidado, conferindo o conteúdo contra o arquivo de origem daquela seção no repositório. O objetivo é pegar o que o autor não vê no próprio texto e o que a consolidação pode ter distorcido (numeração, remissão, nome de serviço, número de parâmetro).

## Como funciona

| Item | Regra |
|---|---|
| Onde revisar | No `.docx` consolidado (para ver numeração, legendas e remissões como o professor vai ver) e no `.md` de origem da seção (para conferir o conteúdo) |
| Rotação | Ciclo fechado de 5: cada seção tem exatamente um revisor e cada integrante revisa uma seção |
| Como registrar | Uma linha por achado na tabela "Achados" no fim deste arquivo — e comentário no card do item, para o autor ver |
| Formato do achado | `seção` + frase/trecho afetado + severidade + correção proposta |
| Severidade | **B — bloqueia** (contradiz o enunciado ou outra seção) · **C — corrige** (erro factual, nome ou número) · **S — sugestão** (redação) |
| Quem corrige | O **autor da seção**, no `.md` de origem. A Pessoa 5 regenera o `.docx` — ninguém edita o `.docx` direto, para não existirem cinco versões paralelas |
| Prazo | Achados até **13/10/2026**; bloqueantes resolvidos no mesmo dia; buffer 14/10 a 16/10 para conferência final e upload |

### Rotação

| Revisor | Revisa a seção | Autor da seção |
|---|---|---|
| Yasmin Kimura (Pessoa 5) | 1 — Identificação dos microsserviços | Roger |
| Roger Viana (Pessoa 1) | 2 — Diagrama de arquitetura | Kevin |
| Kevin Benevides (Pessoa 2) | 3 — Padrões e justificativas | André |
| André Luiz (Pessoa 3) | 4 — Integração entre serviços | Arthur |
| Arthur Corrêa (Pessoa 4) | 5 — Segurança, escalabilidade e governança | Yasmin |

---

## Seção 1 — revisa: Yasmin (autor: Roger)

1. O glossário (seção 1.6) tem **exatamente os 12 nomes** `ms-*`? Todos aparecem com a mesma grafia no diagrama (Figura 2) e na matriz da seção 4.2?
2. A decisão em aberto do item 1.1.2 — 9 serviços de núcleo + 3 satélites contra o piso literal de 10 núcleos: o grupo fecha pela contagem atual ou registra a divergência? (está registrado também na seção 1.7)
3. `MovimentacaoEstoque` (seção 1.1.1, regra 2) é o mesmo nome usado no Database per Service (seção 3.1.3)? E `TransacaoEstoque` aparece só como nome do código atual?
4. As quatro regras de posse (1.1.1) cobrem **uma entidade, um dono** de ponta a ponta, inclusive `Safra` (seção 1.1.4) e as cópias derivadas de `ms-clientes` e `ms-analytics`?
5. Os caminhos de arquivo citados na seção 1.4 existem de fato no repositório? (regra 4 do `README.md` da fase)
6. A tabela 1 responde "responsabilidade única" **e** "dono do dado" para os 12 serviços, sem lacuna?

## Seção 2 — revisa: Roger (autor: Kevin)

1. Estão no repositório os arquivos do desenho nas **quatro páginas**: `.drawio`, `.png` e `.svg` (visão geral + 3 detalhes)? Os `.png`/`.svg` foram reexportados **depois** da revisão de 01/10 (setas síncronas internas na página 1)?
2. A tabela da página 3 (Figura 3) é **idêntica** à matriz e ao catálogo da seção 4.3 — tópico, produtor e consumidores, linha a linha, nos 10 tópicos?
3. A legenda (tabela 6) explica **todos** os padrões de traço usados no desenho? Nenhum traço sem legenda em nenhuma das 4 páginas?
4. A página 1 desenha as **quatro** chamadas síncronas internas que a seção 4.2 lista (`ms-pedidos → ms-estoque`, `→ ms-catalogo`, `→ ms-clientes` e `ms-pagamentos → ms-pedidos`)?
5. Legibilidade: exportar as páginas 2 a 4 para PDF e conferir se a menor fonte impressa em A4 paisagem continua ≥ 12 pt (medições na tabela 8). A visão geral (Figura 1) é para tela/zoom — isso está escrito?
6. Nenhum serviço inventado e nenhum cliente (Web, app, painel) ligado direto a um serviço, só ao gateway?

## Seção 3 — revisa: Kevin (autor: André)

1. Os **4 padrões obrigatórios** (API Gateway, Service Discovery, Database per Service, Saga) têm as três partes — o que resolve, como aparece no sistema, trade-off/limitação? Algum ficou sem trade-off?
2. Cada padrão cita pelo menos um serviço pelo nome do glossário da seção 1.6, com a grafia exata?
3. Saga: compara coreografia x orquestração e **escolhe** coreografia com argumento tirado do desenho (não do livro)?
4. Service Discovery: o argumento de plataforma orquestrada (descoberta nativa por `Service` + DNS interno) é coerente com HPA e mínimo de 2 réplicas da seção 5.5?
5. Os padrões **adicionais** (Strangler Fig, Redis/API Composition, Service Mesh com mTLS) aparecem mesmo no diagrama? Se algum saiu do desenho, sai do texto.
6. A seção 3.3 afirma que CQRS e Transactional Outbox estão fora porque **não existem no desenho** — isso continua verdade depois das mudanças de 01/10 na página 1?

## Seção 4 — revisa: André (autor: Arthur)

1. Todos os pares da matriz de comunicação (tabela 11) têm **Tipo** e **Motivo** preenchidos? Nenhum endpoint fora da convenção `/v1/...` da seção 5.9.2?
2. As quatro mudanças da revisão de 01/10 (a seção 4.6 lista): `lote.criado` com produtor `ms-lotes`; `cliente.anonimizado` com `ms-pedidos`; linha `API Gateway → monolito atual`; linha `ms-pedidos → ms-estoque`. O autor concorda com as quatro? Se não, é o momento de discutir — depois da revisão cruzada não mudamos mais nomes de tópico.
3. O catálogo de eventos (tabela 12) traz produtor e consumidores **iguais** aos da Figura 3 do diagrama?
4. Idempotência (`idEvento`, chave natural) e fila morta (`<topico>.DLQ`, 7 dias) estão explícitos na seção 4.1, no item 4.3 e na coluna "Idempotente obrigatório?"?
5. A compensação da compra (seção 4.4.1, passo 4) descreve o mesmo fluxo da seção 5.6.2 (Fluxo de compensação)? Divergência aqui é achado **B**.
6. Os quatro fluxos exigidos pelo card estão narrados: compra de vinho, alerta de temperatura da adega, rastreabilidade do lote e consulta de estoque do dia a dia?

## Seção 5 — revisa: Arthur (autor: Yasmin)

1. Autenticação (5.3), autorização (5.4) e comunicação entre serviços (5.3.4) deixam o **papel do API Gateway** explícito — validação do token na borda, `X-Identidade` assinado e `traceId`, mTLS interno?
2. Escalabilidade (5.5) e resiliência (5.6) trazem **mecanismos concretos**, não adjetivos: HPA com percentual, cache Redis com TTL, retry com backoff e número de tentativas, circuit breaker com limiar e janela, DLQ com retenção, idempotência no consumidor?
3. LGPD (5.8.2): base legal, minimização, direito do titular e prazos de retenção estão nomeados? A trilha de auditoria cobre as ações de `admin`, `gestor-adega` e `enologo`?
4. Os **números** desta seção batem com os das outras: rate limit 60/600 req/min, timeouts de 2/3/5 s (seção 4.2 cita o de 3 s do estoque), DLQ com 7 dias (seção 4.3), partição por `loteId`/`pedidoId`/`deviceId`?
5. A matriz de autorização (tabela 16) cobre os **12 serviços** do glossário da seção 1.6 com os 5 papéis (`cliente`, `atendente`, `enologo`, `gestor-adega`, `admin`)? Algum serviço ficou sem linha?
6. Os ADRs citados em 5.9.4 existem em `Documentacao/Fase6/adr/` com o mesmo título usado no texto?
7. Os SLOs (tabela 19) não prometem algo incompatível com o desenho — por exemplo, disponibilidade de três noves em um serviço sem réplica (seção 5.5)?

---

## Achados

Preencher uma linha por achado. A Pessoa 5 consolida, o autor corrige no `.md` da sua seção e o `.docx` é regenerado.

| # | Revisor | Seção | Severidade | Achado (frase / trecho) | Correção proposta | Situação |
|---|---|---|---|---|---|---|
| 1 | | | | | | |
| 2 | | | | | | |
| 3 | | | | | | |

## O que já está conferido (não recomeçar)

- **Nomes e tópicos:** os 10 tópicos do Kafka e os 12 serviços foram conferidos linha a linha entre o diagrama, a matriz da seção 4 e o glossário da seção 1 — os nomes batem.
- **Numeração:** 71 títulos numerados, 27 tabelas com legenda sequencial (1 a 27) e 4 figuras; nenhuma remissão aponta para seção inexistente.
- **Cobertura:** o `.docx` reproduz 100% dos blocos de texto das cinco seções, exceto as duas frases da seção 2 que foram reescritas de propósito (a nota sobre inserir o `.svg` no Word e a orientação de consolidação, que agora apontam para as Figuras 1 a 4).
- **Decisões já tomadas na consolidação (01/10):** `lote.criado` com produtor `ms-lotes` (posse de Lote/Genealogia/QR no item 1.4) e a concordância de `A parte 4 foi alinhada…`, ambas registradas no `P2_arquitetura.md` e no `P4_integracao.md`.

## Dúvidas que a rodada precisa fechar

1. **Contagem de serviços:** 9 de núcleo + 3 satélites (item 1.2) contra o piso literal de 10 núcleos do enunciado. Fecha como está ou extrai o carrinho do `ms-pedidos`?
2. **`recebimento.confirmado`:** mantido como tópico proposto (seções 1.7, 2.5 e 4.2). Confirma ou remove dos três lugares?
3. **Revisão cruzada e depois?** Quem faz a conferência final do `.docx` (nomes de arquivo, capa, sumário) antes do upload de 16/10.
