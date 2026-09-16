# Fase 6 — Segurança, Escalabilidade, Resiliência e Governança

**Autora:** Yasmin Kimura · **RM:** 557413 · **Turma:** 3ESOA · **Grupo:** CodeSquad
**Projeto:** Vinheria Agnello — arquitetura baseada em APIs e microsserviços
**Versão:** 1.0 · **Data:** 16/09/2026
**Posição no documento final:** Seção 5 (Segurança e Governança) — item 5 da atividade da Fase 6

---

## 1. Escopo e ligação com as demais partes

Esta seção responde à segunda metade da atividade da Fase 6: **segurança e governança da arquitetura de microsserviços** — autenticação, autorização, comunicação segura entre serviços, boas práticas de API, gestão de segredos, LGPD, escalabilidade, resiliência, observabilidade e governança de contratos.

Ela não redefine o que já é definido em outras partes. Para não haver divergência de texto (fonte clássica de perda de nota em trabalho em grupo), fica fixada a divisão:

| Assunto | Onde é tratado | Papel desta seção |
|---|---|---|
| Quais serviços existem, responsabilidade e dono do dado | Seção 1 (P1) | Consome os nomes `ms-*` sem alterá-los |
| Desenho da arquitetura, gateway, bancos, mensageria | Seção 2 (P2) | Consome o desenho; não redesenha |
| Padrões justificados (Saga, Database per Service etc.) | Seção 3 (P3) | Cita o padrão; o "porquê" fica no P3 |
| Quem fala com quem, síncrono x assíncrono, tópicos | Seção 4 (P4) | Usa exatamente os mesmos nomes de tópicos |
| **Segurança, governança, escala e resiliência** | **Seção 5 (esta)** | Define o "sob quais condições" da arquitetura |

Nenhuma decisão aqui altera a lista de serviços nem a matriz de comunicação: ela adiciona as políticas aplicadas sobre eles.

---

## 2. Retrato do sistema atual (evidências do repositório)

A seção parte de uma auditoria do código que o grupo já entregou nas fases anteriores. Isso importa por dois motivos: (a) mostra que a arquitetura proposta **resolve problemas reais e identificáveis**, e não problemas genéricos de livro; (b) evidencia que o grupo sabe onde está o monolito, o que é pré-requisito da migração via *Strangler Fig*.

| Camada | O que existe hoje | Evidência no repositório | Risco / limitação |
|---|---|---|---|
| Aplicação web (monolito JSP/Servlet) | Login por sessão HTTP; usuários e senhas em `ConcurrentHashMap` em memória, sem *hash*; usuário `admin` semeado com senha padrão | `Web/src/main/java/br/com/fiap/vinheriaagnello/repository/InMemoryDatabase.java` (`CREDENTIALS`, `seed()`) | Credencial padrão e senha em texto claro; estado em memória não sobrevive a *restart* nem escala horizontalmente |
| Filtro de autenticação (web) | `@WebFilter(urlPatterns = "/app/*")` — verifica apenas se existe usuário na sessão | `.../web/AuthFilter.java` | Rotas administrativas `ADMIN_WINES = /admin/vinhos` e `ADMIN_CUSTOMERS = /admin/clientes` (`WebRoutes.java`) **ficam fora do filtro**: há autenticação, mas **não há autorização por papel** |
| API de estoque (.NET) | CORS `AllowAll` (`AllowAnyOrigin` + `AllowAnyMethod` + `AllowAnyHeader`), `UseAuthorization()` sem esquema de autenticação registrado, `AllowedHosts: "*"` | `Web/src/VinheriaAgnello.Server/src/VinheriaAgnello.API/Program.cs`; `appsettings.json` | Qualquer origem consome a API; autorização declarada no *pipeline* mas sem autenticação efetiva |
| Banco de dados | SQLite em arquivo único (`Data Source=vinheria_agnello.db`) com `EnsureCreated()` no *startup* | `appsettings.json`; `Program.cs` | Banco único compartilhado por todos os módulos — exatamente o que o padrão *Database per Service* elimina |
| Contrato da API | Swagger gerado em tempo de execução, exposto apenas em `Development`, sem arquivo de contrato versionado | `Program.cs` (`UseSwagger`/`UseSwaggerUI`) | Sem contrato como fonte da verdade: nada impede o *mobile* e o *web* divergirem |
| Mobile (Android) | Banco local Room com entidade `Produto` (nome, tipo, safra, quantidade, preço) e DAO | `Mobile/app/src/main/java/com/example/myapplication/data/local/entity/Produto.kt`; `dao/ProdutoDao.kt` | Dado em repouso sem criptografia no dispositivo; sincronização com o servidor não é autenticada por identidade de usuário |
| IoT / adega | Arduino Uno (Wokwi) com **DHT22** (temperatura/umidade, pino D2) e **LDR** (luminosidade bruta, A0), JSON a cada **4000 ms** a 9600 baud → ponte Python (`rfc2217://localhost:4000`) → Node-RED (`:1880`) → **HiveMQ Cloud, 8883 com TLS** | `Arduino/VinheriaSensores/VinheriaSensores.ino`; `MQTT/README_MQTT.md` | Credencial MQTT **compartilhada com permissão de Publish *e* Subscribe** e tópico `Fiap/iot/3ESOA/CodeSquad/sensor`: qualquer portador da credencial pode publicar leitura falsa na adega |
| Dados analíticos | CSV com `ID_Venda, Data, Produto, Canal_Venda, Regiao, Valor_Total, Idade_Cliente, Lead_Time_Entrega, Sucesso_Venda` + dashboard Tableau (`.twbx`) | `Web/historico_vendas-vinheria_agnello.csv`; `Web/vinheria dashboard.twbx` | Idade de cliente em base analítica sem pseudonimização, base legal documentada ou política de retenção |

**Conclusão da auditoria:** o sistema atual tem autenticação frágil (sessão + senha padrão em memória), autorização inexistente por papel, API aberta a qualquer origem, banco único compartilhado, ausência de contrato versionado e credenciais de broker compartilhadas. Todos esses pontos são tratados nas seções 3 a 9 abaixo, **como política da arquitetura de microsserviços** — não como correção pontual do monolito atual (que segue no ar durante a migração).

---

## 3. Autenticação

### 3.1 Padrão escolhido: OAuth 2.0 / OpenID Connect com Keycloak

- O `ms-identidade` é implementado com **Keycloak** (OIDC), responsável por emissão, renovação e revogação de credenciais. Nenhum serviço de negócio implementa tela de login, armazena senha ou valida senha.
- **Access token JWT de vida curta (5 min)** + **refresh token rotativo** (7 dias para web, 30 dias para o app mobile com renovação por uso). Vida curta reduz o dano de um token vazado e torna a revogação eficaz na prática.
- Claims do token: `sub` (identificador do cliente/usuário), `perfis` (papéis — seção 4), `escopo`, `cliente_loja` (para operador lotado em uma loja), `exp`, `iat`, `jti`. Os claims **não carregam dado pessoal** (nome, CPF, e-mail) — só identificadores: menos dado circulando em log e em *trace* (LGPD, seção 8).

### 3.2 Onde o token é validado: no API Gateway

Decisão explícita (**ADR-002**, seção 9.4): o **API Gateway** valida assinatura (JWKS), expiração, emissor, público-alvo e escopo do token. Os microsserviços de negócio **confiam na identidade propagada** e revalidam apenas o mínimo que lhes é próprio (por exemplo, o `ms-pagamentos` revalida a assinatura mesmo tendo passado pelo gateway).

Justificativa (trade-off explícito): validar em todos os serviços é mais seguro e mais custoso — cada serviço passa a depender do Keycloak e a duplicar lógica de token. Centralizar no gateway elimina duplicação e ponto de falha nos serviços, mas exige que a comunicação interna seja fechada (o que é garantido por mTLS, seção 3.4). As duas coisas juntas: **validação central na borda + canal interno autenticado**.

### 3.3 Fluxos por tipo de cliente

| Cliente | Fluxo OIDC | Observação |
|---|---|---|
| App mobile (Android) | *Authorization Code* + **PKCE** | O app não guarda segredo de cliente; o refresh token vai para o **Android Keystore**, nunca em `SharedPreferences` em texto claro |
| Web (JSP atual e novo front) | *Authorization Code* + cookie `HttpOnly`, `Secure`, `SameSite=Lax` | Estado anti-CSRF obrigatório no *callback*; sessão deixa de ser o mecanismo de identidade |
| Painel interno da adega / back-office | *Authorization Code* + segundo fator (TOTP) obrigatório para papéis `admin`, `gestor-adega` e `enologo` | Ação irreversível (baixa de estoque, laudo) exige 2FA |
| Serviço → serviço | *Client Credentials* (token de serviço de vida curta) **+ mTLS** | Cada serviço tem sua própria identidade; não existe "rede interna confiável" |
| Rastreio público do lote (QR) | *endpoint* público, **sem token**, com resposta mínima (lote, safra, origem) | Informação pública por definição; nenhum dado pessoal |

### 3.4 Comunicação entre serviços

- **mTLS** entre serviços e para o gateway (certificados curtos, rotação automática via *service mesh*): garante canal cifrado **e** identidade mútua — não basta cifrar e assumir que o emissor é quem diz ser.
- Autorização entre serviços por **escopo do token de serviço** (ex.: só o `ms-pedidos` tem o escopo `estoque:reservar`; só o `ms-qualidade` tem `notificacao:disparar`).
- **Broker MQTT/IoT com identidade por dispositivo** (corrige o modelo atual de credencial compartilhada): cada adega tem usuário próprio no broker (`adega-<id>`), com **ACL restrita a publicar** somente no seu tópico de telemetria — o dispositivo não pode assinar o próprio *namespace* de comandos nem publicar no tópico de outro dispositivo. A ponte Node-RED legada publica no tópico físico atual (`Fiap/iot/3ESOA/CodeSquad/sensor`); na migração (Strangler Fig), um *bridge* mapeia esse tópico para `qualidade.leitura` **sem exigir recompilar o firmware** — o dispositivo não muda, a política muda na borda.

### 3.5 Boas práticas de API aplicadas

| Prática | Como é aplicada | Por quê |
|---|---|---|
| Rate limiting e quotas | No gateway, por `sub` + por serviço: 60 req/min por usuário, 600 req/min por cliente de serviço; *burst* com *token bucket* | Protege contra abuso, varredura de catálogo e negação de serviço com credencial válida |
| Validação de entrada contra o contrato | Todo payload validado contra o **OpenAPI** de cada serviço; rejeitar campo desconhecido em escrita | Impede *mass assignment* (o clássico "cliente manda `status` e `preco` no corpo do pedido") |
| Idempotência | `Idempotency-Key` obrigatório em `POST` de pedido, pagamento e movimentação de estoque; chave guardada 24 h com o resultado | Retry de rede e clique duplo não podem virar dois pedidos nem dois débitos |
| Limite de tamanho e paginação | Corpo ≤ 1 MB; `limit` obrigatório com teto de 100 itens | Evita consulta que derruba o serviço (`?limit=999999`) |
| Nada de dado sensível na URL | Identificador, nunca CPF/e-mail em *path* ou *query* | URLs aparecem em log de proxy, histórico e métrica |
| Erros sem vazamento | Resposta de erro padronizada (`codigo`, `mensagem`, `traceId`); sem *stack trace* nem SQL para o cliente | Erro detalhado é mapa para o atacante |

**OWASP API Security Top 10 (2023) — cobertura explícita:**

| Risco | Contramedida nesta arquitetura |
|---|---|
| API1 BOLA (acesso a objeto de outro usuário) | Autorização sempre no serviço dono do dado, com filtro por `sub`/proprietário no repositório; teste automatizado com dois usuários distintos |
| API2 Autenticação quebrada | OIDC + JWT curto + 2FA no back-office; senha nunca trafega para serviço de negócio |
| API3 Consumo irrestrito de recurso | Rate limit/quota no gateway, paginação obrigatória, timeout em toda chamada |
| API4 Consumo irrestrito de recurso (fluxo) | Limite de itens por pedido e por lote; fila com teto e *backpressure* |
| API5 Autorização em nível de função | Matriz de papéis (seção 4) aplicada por endpoint, não por tela |
| API6 Acesso irrestrito a fluxos sensíveis | Fluxo de pagamento só via `ms-pagamentos`, com `Idempotency-Key` e verificação antifraude |
| API7 SSRF | Serviços não buscam URL fornecida pelo cliente; *webhook* futuro só com lista de destinos permitidos |
| API8 Configuração incorreta | CORS com origem explícita (fim do `AllowAll`), `AllowedHosts` restrito, TLS obrigatório, Swagger desligado em produção |
| API9 Inventário desatualizado | Catálogo de serviços com dono + OpenAPI versionado no repositório (seção 9) |
| API10 Consumo de API de terceiros sem validação | Validação da resposta do adquirente de pagamento e falha segura em resposta inesperada |

---

## 4. Autorização (RBAC + ABAC)

**Modelo:** RBAC como base (papel → permissão) com **regras ABAC** para os casos em que o papel sozinho não decide (loja, região, propriedade do dado).

**Papéis reais da vinheria:** `cliente`, `atendente`, `enologo`, `gestor-adega`, `admin`.

| Serviço | cliente | atendente | enologo | gestor-adega | admin |
|---|---|---|---|---|---|
| `ms-catalogo` | ler | ler | ler | ler | ler, escrever |
| `ms-clientes` | ler/editar **próprio** perfil | ler/editar clientes da sua loja | — | — | ler, escrever |
| `ms-pedidos` | criar/ler **próprios** pedidos | ler/criar pedido para cliente, cancelar | — | — | total |
| `ms-estoque` | ler saldo público | ler saldo, reservar | — | ler, movimentar, ajustar | total |
| `ms-lotes` | ler rastreio público do lote | ler | ler, **criar/validar lote** | ler, movimentar | total |
| `ms-producao` | — | — | ler, registrar | registrar colheita | total |
| `ms-qualidade` | — | — | ler análises, **emitir laudo** | ler telemetria, **configurar faixas/alertas** | total |
| `ms-fornecedores` | — | consultar | — | consultar, receber | total |
| `ms-pagamentos` | pagar próprio pedido, estornar sob regra | registrar pagamento | — | — | estornar |
| `ms-analytics` | — | indicadores da sua loja | — | indicadores de adega | total |

Exemplos de regra **ABAC** (o papel não basta):
- `enologo` **só altera** análise laboratorial enquanto o lote estiver no estado `em_analise` — depois, apenas leitura e a alteração exige justificativa registrada.
- `atendente` só vê clientes da **sua** loja (`cliente_loja == atendente.loja`) — é o mesmo problema de API1 (BOLA), resolvido por atributo e não por tela.
- `gestor-adega` só ajusta faixa de temperatura da **sua** adega (`adegaId` presente no token/atributo).
- Alteração de preço de catálogo acima de um percentual exige `admin` (dupla aprovação: `admin` + `gestor-adega`).

**Como é aplicado (e onde não é):** a autorização é decidida **no serviço dono do dado**, com o contexto vindo do token (papel + atributos). O gateway faz a **autorização grossa** (token válido? escopo da rota?) e o serviço faz a **autorização fina** (esse usuário pode esse objeto?). Nunca se decide autorização em tela ou em JavaScript de front-end.

**Propagação:** o gateway encaminha a identidade em cabeçalho assinado (`X-Identidade` + assinatura do gateway) junto do `traceId`; o serviço valida a assinatura do gateway, o que impede um cliente de forjar `X-Identidade` direto contra um serviço.

---

## 5. Escalabilidade

| Frente | Decisão | Por que é necessário no nosso caso |
|---|---|---|
| Serviços *stateless* | Nenhum estado de sessão no processo; identidade em JWT, não em sessão de servidor | É a causa raiz de hoje: `InMemoryDatabase` guarda sessão e credencial no processo do Servlet — reiniciar derruba a sessão e duas instâncias divergem |
| Autoscaling horizontal | HPA por CPU (70%) e por RPS/requisição por pod; mínimo 2 réplicas nos serviços de núcleo | Pico sazonal: Dia dos Namorados e Natal concentram venda em poucos dias |
| Cache de leitura | Redis para catálogo (TTL 60 s) e para o rate limit distribuído | Catálogo é ~90% leitura; o *rate limit* precisa de contador compartilhado entre réplicas |
| Réplica de leitura | Uma réplica de leitura por serviço de núcleo; escrita no primário | Relatório e catálogo não competem com a escrita do pedido |
| Particionamento de eventos | Kafka particionado por chave: `loteId` (ordem preservada **por lote**) e `pedidoId` | Ordem por lote é regra de negócio (genealogia e rastreio); ordem global não é necessária |
| Telemetria da adega | Tópico particionado por `deviceId`, com escrita em banco de série temporal e *batching* | Com a leitura a cada **4 s** (`INTERVALO_LEITURA_MS = 4000` no firmware), 50 adegas já geram ~12,5 mensagens/s contínuas, 24 h por dia |
| Banco por serviço | Um banco por serviço (PostgreSQL), MongoDB para catálogo, série temporal para telemetria | Substitui o SQLite de arquivo único compartilhado, que é o gargalo e o acoplamento atual |
| Migração gradual | *Strangler Fig*: o gateway passa a rotear domínio por domínio; o monolito continua respondendo o resto | Permite escalar por domínio sem reescrever tudo de uma vez |

---

## 6. Resiliência

### 6.1 Padrões de falha aplicados

| Padrão | Aplicação concreta | Parâmetro |
|---|---|---|
| Timeout | Toda chamada síncrona tem prazo: catálogo 2 s, estoque 3 s, pagamento 5 s | Nenhuma chamada sem prazo; prazo do gateway sempre maior que o do serviço |
| Retry com backoff + jitter | Só em operação idempotente (`GET` e `POST` com `Idempotency-Key`) | 3 tentativas, 200 ms → 400 ms → 800 ms + jitter aleatório |
| Circuit Breaker | Por destino: `ms-pagamentos`, `ms-estoque`, adquirente externo | Abre com 50% de erro em ≥ 20 chamadas na janela de 30 s; *half-open* em 30 s |
| Bulkhead | *Pool* e limite de concorrência separados para `ms-pagamentos` e para a ingestão de telemetria | Pagamento lento não pode consumir as conexões do catálogo |
| Dead Letter Queue | Tópico `<topico>.DLQ` com retenção de 7 dias e alerta | Mensagem envenenada sai da fila principal sem travar o consumo |
| Idempotência no consumidor | Toda mensagem tem `idEvento`; consumidor guarda os últimos `idEvento` processados | At-least-once é o padrão real de mensageria: sem isso, evento repetido cobra duas vezes |
| Degradação elegante | Catálogo responde do cache Redis se o serviço cair; consulta de estoque devolve último saldo conhecido com aviso "saldo pode estar desatualizado" | Preferir resposta parcial explícita a erro total |
| Falha segura | Se o `ms-pagamentos` não responde, o pedido fica `aguardando_pagamento` e a reserva de estoque é **liberada** após o prazo | Nunca "assumir aprovado" |

### 6.2 Fluxo de compensação (ligação com o Saga do P3)

Compra de vinho: gateway → `ms-pedidos` (`pedido.criado`) → `ms-estoque` reserva (`estoque.reservado`) → `ms-pagamentos` (`pagamento.aprovado` | `pagamento.recusado`) → confirma pedido → `ms-notificacoes` avisa → baixa definitiva.

Falha no pagamento: `pagamento.recusado` → `ms-estoque` **libera a reserva** → `ms-pedidos` marca `cancelado` → `ms-notificacoes` informa. A compensação é evento, não comando direto: o serviço que reservou é o único que pode liberar.

### 6.3 Continuidade e deploy

- **Blue-green/canário** por serviço, com *rollback* automático por SLO (aumento de erro 5xx ou de latência p95 na nova versão).
- **Migração de banco compatível** com a versão anterior (expandir → migrar → contrair), para que o *rollback* não exija restaurar banco.
- **Backup** diário por serviço + **teste de restauração** trimestral registrado (backup não testado não é backup).
- Metas de continuidade: **RPO 15 min / RTO 1 h** para `ms-pedidos`, `ms-estoque` e `ms-pagamentos`; RPO 1 h / RTO 4 h para os demais.

---

## 7. Observabilidade

- **OpenTelemetry** com `traceId` único gerado no gateway e propagado em HTTP **e** nos cabeçalhos das mensagens Kafka — é o que permite ligar "o cliente reclamou que o pedido sumiu" a uma cadeia de 5 serviços.
- **Métricas** (Prometheus) por serviço: latência p50/p95/p99, taxa de erro, saturação; por tópico: *lag* do consumidor e tamanho da DLQ; de negócio: pedidos criados, pagamentos aprovados, **alertas de adega**.
- **Dashboards e alertas** (Grafana): alerta de negócio (temperatura fora da faixa — `qualidade.alerta`), alerta de consumo (lag de tópico > 5 min), alerta de borda (taxa de 401/429 anômala, possível tentativa de varredura).
- **Logs estruturados** em JSON, com `traceId`, sem dado pessoal (seção 8).

**SLOs propostos (mensais, por serviço):**

| Serviço | Disponibilidade | Latência | Outro |
|---|---|---|---|
| API Gateway | 99,9% | p95 < 250 ms | — |
| `ms-catalogo` | 99,9% | p95 < 150 ms (cache) | — |
| `ms-estoque` (consulta) | 99,9% | p99 < 400 ms | saldo correto é regra de negócio |
| `ms-pedidos` | 99,9% | p95 < 500 ms | erro 5xx < 0,5% |
| `ms-pagamentos` | 99,9% | p95 < 1,5 s | nenhuma cobrança duplicada (conferência diária) |
| `ms-qualidade` | 99,5% | ingestão < 5 s da leitura | alerta crítico de adega < 30 s |
| `ms-analytics` | 99,0% | atualização < 15 min | atraso não afeta venda |

---

## 8. Segurança da informação, segredos e LGPD

### 8.1 Gestão de segredos

| Hoje | Como fica |
|---|---|
| Credenciais MQTT e cadeia de conexão em arquivo de configuração (`appsettings.json`) e no README (`<USUARIO>`/`<SENHA>`) | Segredos em cofre (**Vault** / *sealed secrets*), injetados em tempo de execução; nada de segredo em repositório, em imagem ou em log |
| Senha de usuário em texto claro na memória (`InMemoryDatabase.CREDENTIALS`) | Senha nunca é armazenada por serviço de negócio: quem autentica é o Keycloak (hash com Argon2id/bcrypt) |
| Token estático de API | Token de serviço de vida curta, emitido por *Client Credentials*, com rotação automática |
| Sem política de rotação | Rotação programada de segredos de broker, banco e adquirente (90 dias, no máximo) e imediata em caso de vazamento |

### 8.2 LGPD

| Item | Como tratamos |
|---|---|
| Base legal e finalidade | Cadastro e pedido = execução de contrato; campanha/marketing = **consentimento explícito e granular**, registrado com data e versão do texto |
| Minimização | A base analítica não precisa de nome, e-mail nem CPF: o `ms-analytics` recebe evento com **identificador pseudonimizado** (`clienteId` → *hash* por safra de chave), nunca o cadastro |
| O caso do CSV atual | `historico_vendas-vinheria_agnello.csv` traz `Idade_Cliente`: dado pessoal em base analítica. Na arquitetura proposta, idade entra **agregada por faixa** (18-24, 25-34, …) e o bruto fica no `ms-clientes`, com acesso auditado |
| Direito do titular | Exclusão/anonimização no `ms-clientes` dispara evento `cliente.anonimizado`; `ms-analytics` e `ms-notificacoes` reagem — sem depender de varredura manual |
| Retenção | Pedido fiscal: 5 anos. Telemetria de adega: 12 meses em série temporal, depois agregada. Log com dado pessoal: 30 dias |
| Logs e trace | CPF/e-mail/telefone mascarados (`***.***.***-**`, `a***@dominio`) na origem; `traceId` ≠ dado pessoal |
| Trilha de auditoria | Toda ação de `admin`, `gestor-adega` e `enologo` (alterar preço, ajustar estoque, emitir laudo, estornar) grava quem/quando/o quê/antes-depois, em *append-only* |
| Pagamentos (PCI-DSS) | Número de cartão **nunca** entra no nosso domínio: tokenização no adquirente; o `ms-pagamentos` guarda apenas *token*, bandeira, últimos 4 dígitos e status |
| Mobile | Banco Room cifrado (SQLCipher), token no Android Keystore, *cache* de estoque limpo no *logout* |

### 8.3 Privacidade desde a concepção

Nenhum serviço recebe dado pessoal de que não precise: catálogo, estoque, qualidade e analytics trabalham por identificador. Quem conhece cadastro é o `ms-clientes` (e o Keycloak, para credencial). Isso reduz o alcance de qualquer incidente — vazamento em `ms-estoque` não expõe cadastro.

---

## 9. Governança

### 9.1 Contrato como fonte da verdade

- **OpenAPI 3.1** por serviço (`ms-*/contracts/openapi.yaml`) e **AsyncAPI** para os eventos (`contracts/asyncapi.yaml`), versionados **no repositório** — não gerados só em tempo de execução.
- **CI valida contrato**: *lint* (Spectral), quebra de compatibilidade detectada por *diff* e **build que falha** quando um campo obrigatório é removido, um tipo muda ou um *enum* perde valor. Contrato quebrado não chega ao `main`.
- Serviços mockados a partir do contrato desde o primeiro dia (permite Roger/Arthur escreverem contra contrato antes do serviço existir).
- Swagger desligado em produção (hoje só existe em `Development`, o que já é o comportamento correto — a diferença é o contrato deixar de ser só de desenvolvimento e passar a ser artefato versionado).

### 9.2 Versionamento e depreciação

- Versão no caminho (`/v1/...`), nunca no corpo ou em cabeçalho.
- Compatibilidade: adição de campo é compatível; remoção/renome/estreitamento de tipo **não é** — exige nova versão.
- Depreciação formal: anúncio, cabeçalho `Deprecation`/`Sunset`, **90 dias** de convivência, telemetria de uso da versão antiga antes de desligar. Nenhuma versão é desligada sem uso residual igual a zero.
- Eventos: evolução de esquema (*schema registry*) permite adicionar campo opcional; mudança incompatível cria novo tópico com sufixo de versão.

### 9.3 Catálogo de serviços e papéis de nome

| Convenção | Regra | Exemplo |
|---|---|---|
| Serviço | `ms-` + domínio, minúsculo, *kebab-case* | `ms-qualidade`, `ms-pagamentos` |
| Tópico de evento | `dominio.evento`, minúsculo, sem prefixo de turma | `pedido.criado`, `estoque.reservado`, `qualidade.alerta` |
| Chamada REST | `/v1/recurso`, substantivo no plural | `/v1/v1/vinhos` → `/v1/vinhos` |
| Banco/esquema | `db_<serviço>` | `db_estoque` |
| Fila morta | `<topico>.DLQ` | `pedido.criado.DLQ` |

Catálogo com **dono** por serviço (evita "serviço órfão" que ninguém mantém nem conhece):

| Serviço | Dono (papel funcional) | Responsável técnico no grupo |
|---|---|---|
| API Gateway, `ms-identidade` | Arquitetura | Roger (P1) com revisão da Yasmin (P5) |
| `ms-catalogo`, `ms-producao`, `ms-lotes`, `ms-estoque`, `ms-fornecedores` | Operação da vinheria | A definir no P1 (lista congelada em 21/09) |
| `ms-pedidos`, `ms-pagamentos`, `ms-notificacoes` | Comercial | Arthur (P4) |
| `ms-qualidade` | Adega | Kevin (P2) |
| `ms-clientes`, `ms-analytics` | Dados e LGPD | Yasmin (P5) |

> Os responsáveis técnicos seguem a divisão do cronograma da Fase 6; os domínios definitivos vêm da lista congelada do P1, que é a fonte única dos nomes.

### 9.4 ADRs — Architecture Decision Records

Arquivos curtos em `Documentacao/Fase6/adr/`, um por decisão: contexto → decisão → consequências (boas e ruins).

- **ADR-001 — OAuth 2.0/OIDC com Keycloak no lugar de sessão de servidor.** Contexto: o monolito autentica com sessão e credenciais em memória (`InMemoryDatabase`), o que não escala nem sobrevive a *restart*. Decisão: identidade centralizada, JWT de vida curta, nenhum serviço guarda senha. Consequência: serviços ficam *stateless* e escaláveis; em troca, o `ms-identidade` vira dependência crítica (mitigada com cache de JWKS e tolerância a falha na validação).
- **ADR-002 — Validação de token centralizada no API Gateway.** Contexto: validar em todos os serviços duplicaria lógica e acoplaria cada serviço ao Keycloak. Decisão: validação de borda + canal interno autenticado por mTLS. Consequência: menos duplicação e um ponto de política; em troca, o gateway vira ponto único (mitigado com réplicas e *health check*).
- **ADR-003 — Identidade MQTT por dispositivo com ACL de publicação.** Contexto: credencial compartilhada com Publish+Subscribe permite leitura falsa de adega. Decisão: usuário por dispositivo, publicação restrita ao tópico próprio, ponte de compatibilidade para o tópico legado. Consequência: telemetria confiável e auditável; em troca, há que provisionar credencial por dispositivo e manter a ponte durante a migração.

---

## 10. Riscos residuais e pendências

| Item | Situação | Depende de |
|---|---|---|
| Nomes definitivos dos serviços e donos do dado | Estimados a partir do repositório e da proposta do P1 | Lista congelada do **P1** (21/09) — esta seção usa os mesmos nomes assim que congelados |
| Papéis da matriz de autorização por serviço | Definidos por domínio funcional | Revisão final contra o diagrama do **P2** e a matriz do **P4** |
| Nomes dos tópicos Kafka | Propostos aqui e iguais aos do desenho | Confirmação cruzada com **P4** (fonte única: `pedido.criado`, `estoque.reservado`, `pagamento.aprovado`, `qualidade.leitura`, `qualidade.alerta`, `lote.criado`, `notificacao.enviar`) |
| Cifra do banco Room e fluxo PKCE no mobile | Especificado nesta seção | Execução depende do time mobile (fase de implementação, não desta atividade) |
| Política de retenção formal (parecer jurídico) | Valores propostos com base em prática de mercado e LGPD | Validação do professor/instituição, se exigida |

---

## 11. Fechamento

Esta seção fecha a arquitetura pelo lado que o desenho sozinho não garante: **quem pode fazer o quê, sob quais condições, com qual rastreabilidade e o que acontece quando algo falha**. Ela responde, com decisões explícitas e justificadas, aos pontos que a auditoria do próprio repositório expôs: sessão e senha padrão em memória (seção 3), rotas administrativas sem verificação de papel (seção 4), CORS aberto e API sem autenticação efetiva (seção 3.5), banco único compartilhado (seção 5), contrato não versionado (seção 9) e credencial de broker compartilhada (seção 6, ADR-003).

Para a consolidação do `.docx`, esta parte entra como **Seção 5** na ordem exigida pelo enunciado, com a tabela de divisão de tarefas já prevista. Nomes de serviços, de tópicos e de papéis aqui usados devem permanecer idênticos aos das Seções 1 a 4 — qualquer mudança posterior exige atualização nas cinco seções.
