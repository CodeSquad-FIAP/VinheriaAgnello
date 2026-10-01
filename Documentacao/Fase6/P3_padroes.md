# Fase 6 — Arquitetura de APIs e Microsserviços

## Item 3 — Padrões de Arquitetura e Justificativas

| Campo | Valor |
|---|---|
| Arquivo | `Documentacao/Fase6/P3_padroes.md` |
| Responsável | André Luiz dos Santos Flores — RM 554952 |
| Período | 22/09/2026 → 30/09/2026 |
| Dependências | `P1_servicos.md` e `P2_arquitetura.md` |
| Fonte dos nomes | Glossário congelado do P1 e componentes representados no P2 |
| Versão | 1.1 — revisão de consolidação de 30/09/2026 (Service Discovery justificado pelo ambiente-alvo e fronteira explícita com a seção 6 do P5) |

---

## 1. Padrões obrigatórios

### 1.1 API Gateway Pattern

**O que resolve →** O API Gateway cria um ponto único de entrada para os clientes e evita que Web, aplicativo mobile e painel interno precisem conhecer diretamente o endereço e a topologia de cada microsserviço. Além do roteamento, ele concentra responsabilidades transversais da borda da aplicação, como validação de autenticação, limitação de requisições e composição de respostas para diferentes canais.

**Como aparece no nosso sistema →** Na arquitetura da Vinheria Agnello, todos os canais entram obrigatoriamente pelo **API Gateway**. Ele roteia requisições no formato `/api/<dominio>` para o respectivo `ms-<dominio>`, valida o JWT emitido pelo `ms-identidade`, aplica *rate limit* com contadores no Redis e realiza agregação/BFF para o aplicativo mobile. Assim, uma operação do Web ou do app pode chegar, por exemplo, ao `ms-catalogo`, `ms-pedidos` ou `ms-clientes` sem que o cliente acesse diretamente esses serviços. Durante a migração, as rotas ainda não extraídas continuam sendo encaminhadas pelo gateway ao sistema legado.

**Trade-off/limitação →** O gateway passa a ser um componente crítico da arquitetura. Uma falha ou configuração incorreta pode afetar o acesso a vários serviços ao mesmo tempo. Ele também precisa ser escalável e monitorado para não se tornar gargalo nem concentrar regras de negócio que deveriam permanecer dentro dos microsserviços.

---

### 1.2 Service Discovery Pattern

**O que resolve →** O Service Discovery permite que os componentes da arquitetura localizem instâncias dos serviços sem depender de endereços IP fixos. Isso é importante em um ambiente distribuído, porque instâncias podem ser reiniciadas, substituídas ou replicadas sem exigir reconfiguração manual dos consumidores.

**Como aparece no nosso sistema →** O P2 representa explicitamente um componente de **Service registry / discovery**, responsável por registrar e localizar réplicas e por disponibilizar informações de *health check*. Dessa forma, serviços como `ms-pedidos`, `ms-estoque` e `ms-pagamentos` podem ser localizados pela plataforma sem acoplamento a um endereço fixo. A escolha aqui é pelo **ambiente**, não por moda: o alvo do grupo é uma plataforma orquestrada, como o próprio P5 assume ao dimensionar por *pod*, com HPA e mínimo de 2 réplicas nos serviços de núcleo (P5, seção 5). Nesse ambiente a descoberta é nativa — cada serviço é exposto por um `Service` com DNS interno, e as réplicas entram e saem do balanceamento conforme o *readiness probe*, sem catálogo externo de endpoints. Consul ou Eureka se justificariam **fora** de um orquestrador, onde esse catálogo não existe: mantê-los aqui acrescentaria um segundo plano de controle para resolver o que o DNS interno já resolve. O padrão permanece o mesmo — muda só quem executa o papel de registry/discovery.

**Trade-off/limitação →** A descoberta de serviços adiciona infraestrutura e operação ao ambiente. Se o registry estiver indisponível ou com informações desatualizadas, novas chamadas podem não localizar uma instância saudável. Por isso, esse componente também precisa de alta disponibilidade e monitoramento. Delegar a descoberta à plataforma transfere essa responsabilidade para ela: `Service`, DNS interno e *probes* passam a ser parte do caminho crítico, e um *readiness probe* mal configurado tira do balanceamento uma instância que está no ar.

---

### 1.3 Database per Service Pattern

**O que resolve →** O Database per Service define que cada microsserviço é o único dono dos seus dados e que nenhum serviço acessa diretamente as tabelas ou coleções de outro. Isso reduz o acoplamento entre domínios e permite que cada serviço evolua seu modelo de dados de acordo com sua própria responsabilidade.

**Como aparece no nosso sistema →** Os 12 serviços da Vinheria possuem bancos próprios no diagrama. O `ms-catalogo` usa MongoDB, o `ms-qualidade` usa TimescaleDB e os demais serviços usam majoritariamente PostgreSQL, incluindo o read-model do `ms-analytics`. A posse de dados também foi definida no P1: o `ms-estoque`, por exemplo, é o único dono de Posição de Estoque, `MovimentacaoEstoque` e Reserva; o `ms-pedidos` é dono de Carrinho, Pedido, Item do Pedido e Status de Separação/Entrega; e o `ms-pagamentos` é dono de Cobrança, Transação, Conciliação e Estorno. Quando um domínio precisa de informação de outro, a troca deve acontecer por API ou evento, e não por acesso direto ao banco vizinho.

No sistema atual existem formas diferentes de persistência que serão gradualmente substituídas ou reposicionadas durante a migração: o Web Servlet/JSP utiliza `Web/src/main/java/br/com/fiap/vinheriaagnello/repository/InMemoryDatabase.java`, a API .NET de estoque possui seu próprio SQLite configurado em `Web/src/VinheriaAgnello.Server/src/VinheriaAgnello.API/appsettings.json` e mapeado por `Web/src/VinheriaAgnello.Server/src/VinheriaAgnello.Infrastructure/Data/AppDbContext.cs`, e o aplicativo Android utiliza Room em `Mobile/app/src/main/java/com/example/myapplication/data/local/`.

**Trade-off/limitação →** Consultas que antes poderiam ser resolvidas diretamente em um banco deixam de poder fazer `JOIN` entre domínios. Dados cruzados passam a ser obtidos por APIs, eventos ou projeções de leitura, como ocorre no `ms-analytics`. Isso aumenta a complexidade da integração e pode introduzir consistência eventual entre diferentes serviços.

---

### 1.4 Saga Pattern

**O que resolve →** O Saga Pattern coordena uma operação de negócio que atravessa vários microsserviços sem depender de uma única transação distribuída. Cada serviço executa sua própria transação local e, quando alguma etapa falha, o fluxo precisa executar ações compensatórias para desfazer ou neutralizar o que já havia sido realizado.

**Como aparece no nosso sistema →** O fluxo de compra da Vinheria utiliza eventos no Apache Kafka. O `ms-pedidos` publica `pedido.criado`; o `ms-estoque` consome esse evento, realiza a reserva e publica `estoque.reservado`; o `ms-pagamentos` processa a cobrança e publica `pagamento.aprovado` ou `pagamento.recusado`. Em caso de aprovação, o pedido segue para confirmação e baixa definitiva do estoque. Em caso de recusa, o pedido é cancelado e a reserva precisa ser liberada, funcionando como compensação da etapa já executada.

#### Coreografia x orquestração

Na **orquestração**, existiria um coordenador central responsável por determinar a próxima etapa da saga. Isso facilita acompanhar o fluxo em um único ponto, mas cria um componente adicional que conhece a sequência completa da operação.

Na **coreografia**, cada serviço reage aos eventos publicados pelos demais. O fluxo fica mais desacoplado, porque os participantes não dependem de um coordenador central, porém o comportamento completo fica distribuído entre vários produtores e consumidores de eventos.

#### Escolha para a Vinheria

Para a arquitetura desenhada, a escolha é **Saga por coreografia**. O P2 representa o fluxo de compra como uma sequência de eventos no Kafka entre `ms-pedidos`, `ms-estoque` e `ms-pagamentos`, e não apresenta um serviço separado atuando como orquestrador. A tabela de tópicos também mostra cada participante publicando o resultado de sua etapa para que o próximo consumidor reaja.

**Trade-off/limitação →** A coreografia reduz o acoplamento direto, mas torna mais difícil enxergar e depurar o fluxo completo, porque o estado da operação fica distribuído entre vários serviços e eventos. Também exige atenção às compensações e às situações em que um consumidor esteja temporariamente indisponível.

---

## 2. Padrões adicionais

Os padrões abaixo foram escolhidos entre os extras sugeridos na atividade porque aparecem de forma explícita na arquitetura do P2. Outros padrões possíveis não foram incluídos apenas por quantidade, para manter a entrega consistente com o diagrama.

### 2.1 Strangler Fig Pattern

**O que resolve →** O Strangler Fig permite migrar gradualmente um sistema legado para uma nova arquitetura, sem exigir que todo o monolito seja substituído de uma única vez. Novos domínios passam a ser atendidos pelos microsserviços enquanto o restante continua temporariamente no sistema atual.

**Como aparece no nosso sistema →** O diagrama mantém o sistema atual atrás do API Gateway. As rotas ainda não extraídas continuam indo para o Web Servlet/JSP em `Web/src/main/java/br/com/fiap/vinheriaagnello/` e para a API .NET em `Web/src/VinheriaAgnello.Server/`. A extração planejada começa pela API .NET de estoque, que serve como embrião do `ms-estoque`, e depois segue para catálogo e clientes, atendidos futuramente por `ms-catalogo` e `ms-clientes`.

**Trade-off/limitação →** Durante a transição, a equipe precisa manter temporariamente o legado e os novos microsserviços ao mesmo tempo. Isso aumenta a complexidade de roteamento, testes, observabilidade e manutenção até que todos os domínios previstos sejam extraídos.

---

### 2.2 Cache (Redis) + API Composition

**O que resolve →** O uso de cache reduz acessos repetidos a dados que podem ser reutilizados por um período curto, enquanto API Composition permite combinar respostas de mais de um serviço para atender um cliente sem obrigá-lo a realizar várias chamadas diretamente.

**Como aparece no nosso sistema →** O Redis aparece como componente transversal da arquitetura. No P2, ele é usado como cache do `ms-catalogo`, com TTL de 60 segundos, e também pelos contadores de *rate limit* do API Gateway. O gateway realiza agregação/BFF para o aplicativo mobile, compondo respostas sem expor diretamente a malha interna de serviços ao cliente.

**Trade-off/limitação →** O cache pode entregar dados temporariamente desatualizados até o vencimento ou a invalidação da entrada. A composição de respostas no gateway também aumenta a responsabilidade desse componente e pode elevar o tempo de resposta quando depende de vários serviços simultaneamente.

---

### 2.3 Service Mesh com mTLS

**O que resolve →** O Service Mesh centraliza preocupações da comunicação entre serviços, principalmente segurança do tráfego interno. O uso de mTLS permite que as duas pontas da comunicação se autentiquem mutuamente e que os dados trafeguem cifrados dentro da malha.

**Como aparece no nosso sistema →** O P2 representa um **Service mesh (mTLS)** como componente transversal e informa que toda chamada interna da malha utiliza mTLS, com canal cifrado e rotação automática de certificados. Assim, comunicações entre serviços como `ms-pedidos`, `ms-estoque`, `ms-pagamentos`, `ms-clientes` e os demais participantes não dependem apenas da proteção aplicada na borda pelo API Gateway. A política de certificados e de identidade entre serviços é detalhada na seção 3.4 do P5; neste item o *service mesh* é justificado como decisão de arquitetura, sem repetir o texto daquela seção.

**Trade-off/limitação →** O service mesh adiciona complexidade operacional à plataforma, pois exige gerenciamento dos componentes da malha, certificados, políticas e observabilidade. Também adiciona uma camada extra ao caminho de comunicação, que precisa ser corretamente configurada e monitorada para não dificultar o diagnóstico de falhas.

---

## 3. Padrões avaliados e não incluídos nesta versão

A atividade sugere outros padrões que poderiam fortalecer a arquitetura, como Circuit Breaker, Retry com backoff, Bulkhead, Transactional Outbox, Idempotent Consumer e CQRS. Eles não entram nesta lista por dois motivos distintos, que não devem ser confundidos:

- **Circuit Breaker, Retry com backoff, Bulkhead, dead letter queue e idempotência no consumidor estão na arquitetura** — são tratados como **políticas de resiliência** na seção 6 do P5, com parâmetros definidos (janela de erro, número de tentativas, retenção da DLQ). Aqui não são repetidos para não duplicar texto entre as seções do mesmo documento: o P3 justifica o padrão, o P5 detalha a política.
- **CQRS e Transactional Outbox continuam fora** porque não existem no desenho do P2: não há read model de catálogo nem tabela de *outbox* representados. Se entrarem no P2 antes da consolidação, esta seção é revista; caso contrário, citá-los aqui seria justificar um padrão que o diagrama não sustenta.

O critério que vale para os **padrões de arquitetura** desta entrega continua sendo o do diagrama: só entra o que tem representação explícita ou suporte direto no P2. O que muda na redação é não tratar os itens acima como se estivessem fora da arquitetura — os de resiliência estão nela, descritos na parte do P5 que é dona desse assunto.

---

## 4. Conferência dos critérios de aceite

| Critério do Item 3 | Situação |
|---|---|
| Os 4 padrões obrigatórios têm justificativa com exemplo do sistema | Atendido: API Gateway, Service Discovery, Database per Service e Saga |
| Cada padrão cita ao menos um serviço pelo nome do diagrama | Atendido nos blocos correspondentes |
| Todo padrão apresenta trade-off ou limitação | Atendido em todos os 7 padrões |
| Saga compara coreografia e orquestração | Atendido no item 1.4 |
| Saga escolhe uma abordagem com argumento | Atendido: coreografia, por ser a abordagem representada no P2 |
| Padrões extras aparecem no diagrama | Atendido: Strangler Fig, Redis/API Composition e Service Mesh com mTLS |
| Padrões de resiliência não duplicados entre P3 e P5 | Atendido: Circuit Breaker, Retry, Bulkhead, DLQ e idempotência ficam como política na seção 6 do P5 (item 3) |
| Nomes dos serviços seguem o glossário congelado do P1 | Atendido |
| Nenhum serviço novo foi inventado | Atendido |

---

## 5. Dependências e fronteiras com as outras partes

- **P1 — Roger:** fornece a lista congelada de serviços, responsabilidades e posse de dados usada principalmente no Database per Service.
- **P2 — Kevin:** fornece o desenho que valida quais padrões realmente aparecem na arquitetura.
- **P4 — Arthur:** detalha a comunicação entre serviços e o fluxo de mensagens. Neste P3, a Saga é apresentada apenas como padrão arquitetural e justificada pelo seu propósito.
- **P5 — Yasmin:** reutiliza estas decisões na seção de governança, segurança, tolerância a falhas e consolidação. Fronteira acordada: o P3 justifica o padrão (o "porquê"); as políticas de resiliência, com parâmetros, ficam na seção 6 do P5 (o "como"), para não haver texto repetido entre as duas seções.

---

## 6. Pontos a confirmar com o grupo

- **Saga por coreografia:** esta entrega assume coreografia porque o P2 mostra os serviços reagindo a eventos no Kafka e não apresenta um orquestrador separado.
- **Service Discovery:** a escolha feita no item 1.2 assume plataforma orquestrada (descoberta nativa por `Service` + DNS interno), pelo ambiente que o P5 já adota. O P2 desenha o registry/discovery sem nomear a tecnologia; se o grupo preferir declarar Consul ou Eureka, ou nomear a plataforma explicitamente no P5, o parágrafo do item 1.2 muda em uma linha.
- **Padrões adicionais futuros:** Outbox e CQRS só devem entrar no P3 se forem também representados no P2, para preservar a consistência entre texto e diagrama. Os padrões de resiliência (Circuit Breaker, Retry, Bulkhead, DLQ, idempotência) permanecem no P5, para o documento não repetir a mesma explicação em duas seções.

---

## Registro de revisão

- **v1.0 (30/09/2026)** — versão inicial do Item 3, com os quatro padrões obrigatórios e três padrões adicionais consistentes com o P1 e o P2 (André Luiz dos Santos Flores).
- **v1.1 (30/09/2026)** — revisão de consolidação (Yasmin Kimura, P5): Service Discovery justificado pelo ambiente-alvo (plataforma orquestrada, `Service` + DNS interno) em vez de manter a tecnologia indefinida; fronteira explícita com a seção 6 do P5 na lista de padrões não incluídos, para o documento não afirmar que Circuit Breaker, Bulkhead e idempotência estão fora da arquitetura.
