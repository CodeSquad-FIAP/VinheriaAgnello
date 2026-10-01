# Fase 6 — Arquitetura de APIs e Microsserviços
## Item 1 — Identificação dos Microsserviços e Fronteiras de Domínio

| Campo | Valor |
|---|---|
| Arquivo | `Documentacao/Fase6/P1_servicos.md` (fonte da consolidação) + `P1_servicos.docx` |
| Responsável | Roger Viana Gonçalves de Alencar — RM 97540 |
| Período | 16/09/2026 → 21/09/2026 (congelamento) |
| Versão | 1.1 — revisão de consolidação de 22/09/2026 |

Integrantes do grupo:

- Kevin Benevides da Silva Romariz — RM 557898
- Arthur Cei Corrêa — RM 97781
- Yasmin Kimura — RM 557413
- Roger Viana Gonçalves de Alencar — RM 97540
- André Luiz dos Santos Flores — RM 554952

---

## 1. Serviços candidatos a microsserviço

O ponto de partida foi o que já existe no repositório do grupo: a API .NET de estoque, com as entidades `Vinho`, `Lote`, `Fornecedor`, `Categoria` e `TransacaoEstoque`; o monolito em JSP/Servlet; o app Android com Room; a cadeia Arduino, MQTT e Node-RED; e o par CSV com dashboard no Tableau. A partir disso chegamos a 12 serviços: **9 de núcleo**, que cobrem a cadeia de valor da vinícola do início ao fim, e **3 satélites** — identidade, notificações e analytics — que dão suporte a todos os domínios sem pertencer a nenhum deles em particular.

| Serviço | Classificação | Responsabilidade única | Dados dos quais é dono | Quem consome |
|---|---|---|---|---|
| `ms-catalogo` | Núcleo | Expor e manter o catálogo de vinhos, categorias e fichas de harmonização para os canais de venda. | Vinho (ficha comercial, teor, mídia) · Categoria · Regra de Harmonização | `ms-pedidos` · Web / App (canais) · `ms-analytics` |
| `ms-producao` | Núcleo | Registrar o ciclo produtivo do vinho, da colheita à fermentação e armazenagem, por safra. | Safra · Colheita · Fermentação · Armazenagem | `ms-lotes` · `ms-qualidade` · `ms-analytics` |
| `ms-lotes` | Núcleo | Garantir a rastreabilidade individual de cada lote, do engarrafamento ao consumidor. | Lote · Genealogia do Lote · Código de Rastreio / QR · Histórico do lote | `ms-estoque` · `ms-catalogo` · `ms-qualidade` · App do consumidor |
| `ms-estoque` | Núcleo | Manter a posição de estoque em tempo real e registrar movimentações e reservas. | Posição de Estoque · MovimentacaoEstoque · Reserva | `ms-pedidos` · `ms-fornecedores` · App mobile · `ms-analytics` |
| `ms-pedidos` | Núcleo | Orquestrar o ciclo de vida do pedido, do carrinho à confirmação de entrega. | Carrinho · Pedido · Item do Pedido · Status de Separação / Entrega | `ms-pagamentos` · `ms-estoque` · `ms-notificacoes` · `ms-clientes` |
| `ms-fornecedores` | Núcleo | Gerir o cadastro de fornecedores e o fluxo de compras e recebimento. | Fornecedor · PedidoCompra · Recebimento | `ms-estoque` · `ms-producao` · `ms-analytics` |
| `ms-qualidade` | Núcleo | Monitorar condições ambientais da adega e registrar análises e alertas. | Leitura de Temperatura / Umidade · Análise Laboratorial · Alerta | `ms-estoque` · `ms-producao` · `ms-analytics` · Painel operacional |
| `ms-clientes` | Núcleo | Manter cadastro, perfil e consentimento LGPD de cada cliente. | Cliente · Perfil · Consentimento LGPD | `ms-pedidos` · `ms-identidade` · `ms-notificacoes` · `ms-analytics` |
| `ms-pagamentos` | Núcleo | Processar cobrança, conciliação financeira e estornos das vendas. | Cobrança · Transação · Conciliação · Estorno | `ms-pedidos` · `ms-analytics` · `ms-notificacoes` |
| `ms-identidade` | Satélite | Autenticar e autorizar usuários e serviços via OAuth2/OIDC e RBAC. | Usuário / Credencial · Papel / Permissão (RBAC) · Token / Sessão | `ms-pedidos` · `ms-clientes` · `ms-fornecedores` (painel) · App mobile |
| `ms-notificacoes` | Satélite | Disparar e rastrear comunicações por e-mail, WhatsApp e push. | Template de Mensagem · Envio (status) · Preferência de canal | `ms-pedidos` · `ms-qualidade` · `ms-pagamentos` · `ms-clientes` |
| `ms-analytics` | Satélite | Consolidar indicadores de venda e produção para previsão e dashboards. | Indicador Consolidado · Modelo de Previsão | Dashboard (BI) · Gestão / Direção |

### 1.1 Regras de posse dos dados (uma entidade, um dono)

Para que a parte 3 (Database per Service) e o diagrama da parte 2 não encontrem posse duplicada, valem estas quatro regras. Nenhum serviço compartilha tabela com outro: cada entidade tem um único dono.

1. **Safra pertence ao `ms-producao`.** O `ms-catalogo` guarda a ficha comercial do vinho (teor, harmonização, mídia) e apenas *referencia* a safra de exibição pelo `safraId`; não é dono do registro de safra. O `ms-lotes` também referencia `safraId` ao classificar o lote.
2. **Nome canônico da movimentação.** O que hoje existe como `TransacaoEstoque` no .NET passa a se chamar `MovimentacaoEstoque` no alvo — é a mesma entidade, renomeada na migração. Ela pertence ao `ms-estoque` e não se duplica em outro serviço.
3. **Só o `ms-estoque` escreve no agregado de estoque.** Posição, movimentação e reserva são escritas exclusivamente por ele. A entrada gerada pelo recebimento de uma compra nasce no `ms-fornecedores` (dono do `Recebimento`) e chega ao `ms-estoque` como evento; **quem registra o movimento de entrada é o `ms-estoque`**. Nenhum outro serviço escreve posição de estoque.
4. **Cópias derivadas são declaradas como tal.** O histórico de compras do `ms-clientes` e o dataset histórico do `ms-analytics` são *read-models* (cópias somente-leitura) alimentados por evento a partir do `ms-pedidos`; não são posse de dado. O dono do pedido continua sendo o `ms-pedidos`.

### 1.2 Sobre a contagem

O enunciado pede **os principais serviços** do sistema e exemplifica quatro domínios (produção, rastreamento de lotes, pedidos/compras/fornecedores/estoque e qualidade). A decomposição do grupo chegou a **12 serviços: 9 de núcleo + 3 satélites**, pelos critérios de fronteira do item 2:

- **Núcleo (9)** — os que carregam a cadeia de valor da vinícola: `ms-catalogo`, `ms-producao`, `ms-lotes`, `ms-estoque`, `ms-pedidos`, `ms-fornecedores`, `ms-qualidade`, `ms-clientes` e `ms-pagamentos`;
- **Satélites (3)** — capacidades de plataforma que servem a todos os domínios sem pertencer a nenhum: `ms-identidade`, `ms-notificacoes` e `ms-analytics`.

Optamos por não inflar a lista para arredondar um número: entre criar um serviço a mais só para fechar uma contagem e manter apenas serviços com responsabilidade genuinamente coesa, ficamos com a segunda opção — o mesmo critério usado para descartar os seis itens do item 3. Se o grupo quiser um serviço de núcleo a mais, o único candidato natural é extrair o carrinho do `ms-pedidos` (avaliado e descartado no item 3 por ser estado efêmero do mesmo agregado); a decisão está registrada no item 7.

### 1.3 Critérios de aceite — conferência

| Critério do enunciado | Onde é atendido |
|---|---|
| Cada serviço tem responsabilidade única e não compartilha tabela com outro | Coluna "Responsabilidade única" da tabela do item 1 e regras de posse do item 1.1 |
| Cada serviço diz quem é o dono do dado (uma entidade por linha) | Coluna "Dados dos quais é dono" da tabela do item 1, com as cópias derivadas declaradas no item 1.1 |
| Nomes padronizados (`ms-...`) | Item 6 — glossário congelado, 12 nomes, sem exceção |
| Seção curta do que ficou de fora e por quê | Item 3, com seis descartes justificados |

### 1.4 Dono do dado por entidade

Cada entidade pertence a **um único** serviço, e esta é a lista que a parte 3 (*Database per Service*) usa como entrada. Onde existe leitura cruzada, ela acontece por API ou por evento — nunca por acesso direto ao banco do vizinho.

| Serviço | Entidades das quais é dono |
|---|---|
| `ms-catalogo` | Vinho (ficha comercial), Categoria, Regra de Harmonização, Mídia do Produto |
| `ms-producao` | Safra, Colheita, Fermentação, Armazenagem |
| `ms-lotes` | Lote, Genealogia do Lote, Código de Rastreio / QR, Histórico do Lote |
| `ms-estoque` | Posição de Estoque, MovimentacaoEstoque, Reserva |
| `ms-pedidos` | Carrinho, Pedido, Item do Pedido, Status de Separação / Entrega |
| `ms-fornecedores` | Fornecedor, PedidoCompra, Recebimento |
| `ms-qualidade` | Leitura de Temperatura / Umidade, Análise Laboratorial, Alerta |
| `ms-clientes` | Cliente, Perfil, Consentimento LGPD |
| `ms-pagamentos` | Cobrança, Transação, Conciliação, Estorno |
| `ms-identidade` | Usuário / Credencial, Papel / Permissão (RBAC), Token / Sessão |
| `ms-notificacoes` | Template de Mensagem, Envio (status), Preferência de Canal |
| `ms-analytics` | Indicador Consolidado, Modelo de Previsão |

Fora dessa lista, duas coisas circulam como **projeção derivada**, não como posse: o histórico de compras em `ms-clientes` (projeção do que pertence ao `ms-pedidos`) e o dataset histórico em `ms-analytics` (cópia alimentada por evento). `Safra` aparece uma única vez, no `ms-producao`, conforme a regra 1 do item 1.1.

## 2. Fronteiras de domínio

Cada corte que propomos aqui delimita o que a literatura de DDD chama de *Bounded Context*: um pedaço do sistema com vocabulário e regras próprias, que muda no seu próprio ritmo. Por isso ele não deveria dividir tabela, deploy nem time com o vizinho.

O que existe hoje no repositório é o contraexemplo do recorte que queremos: em `Web/src/main/java/br/com/fiap/vinheriaagnello/`, o catálogo (`web/VitrineServlet.java`, `web/VinhoServlet.java`) e o cadastro de clientes (`web/ClienteCadastroServlet.java`) compartilham o mesmo `repository/InMemoryDatabase.java` e os mesmos modelos de `model/` — mexer na vitrine obriga a testar o cadastro. Cada fronteira abaixo existe para não repetir esse acoplamento em escala de serviço. Os critérios usados em cada corte estão descritos a seguir — os 12 serviços aparecem justificados.

- **Catálogo e lotes parecem parecidos, mas mudam por razões diferentes.** O catálogo (`ms-catalogo`) muda por motivo comercial, como uma safra nova em destaque ou uma ficha de harmonização reescrita. Lotes (`ms-lotes`) muda por exigência regulatória, ligada à rastreabilidade do vinho. Juntar os dois num módulo só faria o time comercial testar regra fiscal toda vez que trocasse uma foto de produto.
- **Produção × Qualidade.** Um decide o que produzir e quando (`ms-producao`). O outro mede se o que já foi produzido está dentro do padrão prometido (`ms-qualidade`). A diferença de frequência é grande: a telemetria dos sensores gera uma leitura a cada poucos segundos, um volume que não faz sentido impor ao modelo transacional, bem mais lento, da produção.
- **Estoque e pedidos guardam verdades diferentes.** O estoque (`ms-estoque`) sabe quanto existe de fato na adega. Pedidos (`ms-pedidos`) sabe quanto já foi prometido a algum cliente. Se fossem o mesmo serviço, um pico de acesso ao catálogo em data comemorativa passaria a competir, na mesma escala, com a escrita do estoque, que não pode errar.
- **Pagamentos isolado.** Cobrança, conciliação e estorno lidam com dado sensível e exigem trilha de auditoria própria, no padrão PCI-DSS. Não há razão para replicar essa exigência em outro contexto. Isolar tudo em `ms-pagamentos` limita a um único serviço o raio de exposição desse dado.
- **Compras e recebimento × estoque.** `ms-fornecedores` é dono do documento de compra (`PedidoCompra`) e do `Recebimento` — a conferência física do que chegou, com preço e divergência. `ms-estoque` é dono da posição e do movimento. São verdades com donos diferentes: uma compra pode estar recebida e ainda não movimentada, e o inverso nunca pode acontecer. Por isso o movimento de entrada é escrito só pelo `ms-estoque`, a partir do evento de recebimento.
- **Fornecedores × pedidos.** `PedidoCompra` (suprimento: fornecedor, custo, prazo de entrega) e `Pedido` (venda: cliente, receita, entrega) têm ciclos, indicadores e interlocutores distintos. Chamar os dois de "pedido" no mesmo serviço é o caminho mais curto para relatório de compra entrando no faturamento — por isso o nome `PedidoCompra` fica no `ms-fornecedores` e o `Pedido` no `ms-pedidos`.
- **Clientes × Identidade.** Dado de cliente é negócio: cadastro, perfil e consentimento LGPD, que muda quando o cliente quer, com obrigação legal de exclusão. Credencial, papel e token são plataforma: mudam quando a política de segurança muda. Se fossem o mesmo serviço, uma alteração de política de senha passaria a mexer no cadastro comercial e o consentimento LGPD ficaria no caminho crítico do login.
- **Identidade e notificações não carregam nenhuma regra de negócio da vinícola:** são capacidades de plataforma, usadas por qualquer domínio. Por isso viraram satélites, e não núcleo — assim evitamos a tentação de grudar autenticação ou envio de e-mail na lógica de um domínio específico.
- **Analytics também é satélite, e lê, não escreve.** `ms-analytics` consolida indicadores a partir de eventos dos domínios e é dono apenas do indicador consolidado e do modelo de previsão. Se fosse núcleo, toda decisão de BI viraria dependência do caminho transacional — e um dashboard pesado poderia derrubar venda.

## 3. O que NÃO vira microsserviço (e por quê)

Um recorte generoso demais tem custo real: dilui responsabilidade e multiplica chamada de rede sem ganhar coesão em troca. Por isso descartamos de propósito os itens a seguir como serviços independentes.

- **Categoria isolada.** Tem poucos valores e quase nunca muda. Transformá-la num serviço à parte criaria uma chamada de rede para responder a algo que uma tabela de referência dentro do `ms-catalogo` resolve sozinha. É o caso do `Categoria.cs` atual.
- **A genealogia do lote não é um contexto de negócio à parte.** É só uma faceta do dado, um relacionamento entre lotes, e cabe perfeitamente dentro do `ms-lotes`.
- **Camada de apresentação.** O monolito `Web/src/main/java/br/com/fiap/vinheriaagnello/` (Servlet/JSP) e o app Android em `Mobile/app/src/main/java/com/example/myapplication/` continuam existindo, só que agora como clientes dos microsserviços (ou, no limite, um BFF, sigla de *Backend-for-Frontend*). Como não são donos de regra de negócio nem de dado, não entram nesta lista. A validação de sessão que hoje está em `AuthFilter.java` migra para o API Gateway.
- **O Node-RED segue fazendo o que já faz hoje:** traduzir e repassar telemetria do broker MQTT para o `ms-qualidade`, conforme `MQTT/node_red_vinheria_flow.json` e `MQTT/README_MQTT.md`. Não tem domínio de negócio nem dado próprio, então continua sendo infraestrutura de integração, não um serviço.
- **Um `ms-relatorios` à parte.** Duplicaria a posse de um dado que já pertence ao `ms-analytics`. Um relatório é só uma forma de exibir a mesma informação analítica, não um contexto de negócio novo.
- **Separar o carrinho do `ms-pedidos` também não se justifica agora.** O carrinho é um estado efêmero, anterior à transação, do mesmo agregado do pedido; isolá-lo cedo criaria consistência distribuída sem ganho real, um YAGNI clássico nesta fase do projeto.

## 4. Do repositório ao serviço

Nada disso veio de teoria abstrata. Cada corte foi ancorado em algo que o grupo já tinha construído, e a tabela abaixo mostra de onde veio cada decisão, com o caminho do arquivo no repositório.

| Artefato no repositório | Serviço herdeiro / papel |
|---|---|
| `Web/src/VinheriaAgnello.Server/src/VinheriaAgnello.Domain/Entities/Vinho.cs` e `Categoria.cs` | `ms-catalogo` |
| `Web/src/VinheriaAgnello.Server/src/VinheriaAgnello.Domain/Entities/Lote.cs` | `ms-lotes` |
| `Web/src/VinheriaAgnello.Server/src/VinheriaAgnello.Domain/Entities/Fornecedor.cs` | `ms-fornecedores` |
| `Web/src/VinheriaAgnello.Server/src/VinheriaAgnello.Domain/Entities/TransacaoEstoque.cs` | `ms-estoque` (renomeado para `MovimentacaoEstoque`) |
| `Web/src/VinheriaAgnello.Server/src/VinheriaAgnello.API/Controllers/EstoqueController.cs` e `Program.cs` | `ms-estoque` (a API .NET de hoje é o serviço extraído) |
| `Web/src/main/java/br/com/fiap/vinheriaagnello/` (`web/VitrineServlet.java`, `web/VinhoServlet.java`, `web/ClienteCadastroServlet.java`, `model/`, `repository/InMemoryDatabase.java`) | Cliente / BFF dos microsserviços; não é um serviço. Hoje concentra catálogo e clientes — consumidor futuro de `ms-catalogo` e `ms-clientes` |
| `Mobile/app/src/main/java/com/example/myapplication/data/local/` (`AppDatabase.kt`, `entity/Produto.kt`, `dao/ProdutoDao.kt`) e `ui/estoque/` | Cliente / BFF dos microsserviços; não é um serviço. Hoje é espelho local de estoque — consumidor futuro de `ms-estoque` |
| `Arduino/VinheriaSensores/VinheriaSensores.ino` + `MQTT/node_red_vinheria_flow.json` + `MQTT/README_MQTT.md` | `ms-qualidade` (o Node-RED atua como gateway de integração) |
| `Web/historico_vendas-vinheria_agnello.csv` + `Web/vinheria dashboard.twbx` | `ms-analytics` |

## 5. Reconciliação com os domínios do enunciado

O enunciado pede os principais serviços e exemplifica quatro domínios. A tabela abaixo é a correspondência entre cada exemplo do enunciado e os serviços propostos nesta entrega, com a razão de ter havido separação em mais de um serviço.

| Exemplo do enunciado | Serviços propostos | Por que não é um serviço só |
|---|---|---|
| **Gestão de Produção** — registro de colheitas, fermentação, armazenamento | `ms-producao` (a armazenagem é posição de estoque e fica no `ms-estoque`) | A safra e o ciclo produtivo são um domínio coeso; a posição física do vinho pertence ao estoque, que é escrito só por ele (regra 3 do item 1.1) |
| **Rastreamento de Vinhos** — controle de lotes, histórico de produção | `ms-lotes` (a safra vem do `ms-producao`) | Lote, genealogia, código de rastreio e histórico são um domínio só, que muda por exigência regulatória — não pelo motivo comercial do catálogo (item 2) |
| **Gestão de Pedidos** — controle de compras, fornecedores e estoque | `ms-pedidos`, `ms-fornecedores` e `ms-estoque` | São três verdades com donos diferentes: `Pedido` (venda, cliente, receita), `PedidoCompra` (suprimento, custo, prazo) e Posição de Estoque (o que existe de fato) — separação justificada nos cortes "Fornecedores × pedidos" e "Compras e recebimento × estoque" (item 2) |
| **Monitoramento de Qualidade** — temperatura, umidade e análises laboratoriais | `ms-qualidade` | Um serviço só, com banco de série temporal: recebe a telemetria da adega e publica os alertas (seções 2 e 4) |

O resultado da análise dos dois lados:

- **Sem correspondência direta no enunciado (6):** `ms-catalogo` (vitrine comercial dos vinhos), `ms-clientes` (cadastro, perfil e consentimento LGPD), `ms-pagamentos` (cobrança e conciliação), `ms-identidade` (credencial e RBAC), `ms-notificacoes` (e-mail, WhatsApp e push) e `ms-analytics` (indicadores e previsão) — todos ancorados em algo que já existe no repositório (item 4).
- **Nomes:** nenhum serviço foi renomeado; o glossário do item 6 é a mesma nomenclatura que o diagrama (seção 2), os padrões (seção 3) e a comunicação (seção 4) usam. Uma entidade muda de nome no alvo: `TransacaoEstoque` → `MovimentacaoEstoque`.
- **Cortados:** nenhum serviço. O carrinho foi avaliado e **não** promovido a serviço; permanece como agregado dentro do `ms-pedidos` (justificativa no item 3).
- **Separação que o enunciado não pede explicitamente:** a **classificação núcleo/satélite** e a divisão entre `ms-clientes` (dado de cliente e LGPD) e `ms-identidade` (credencial e RBAC), que sem ela ficariam no mesmo serviço de plataforma.
- **Contagem:** 12 serviços (9 núcleo + 3 satélites), conforme o item 1.2.

## 6. Glossário de nomes

Esta é a nomenclatura definitiva dos serviços. Os itens 2, 3 e 4 desta atividade, o diagrama de contexto e a comunicação entre serviços devem usar exatamente estes nomes.

| Serviço | Classificação |
|---|---|
| `ms-catalogo` | Núcleo |
| `ms-producao` | Núcleo |
| `ms-lotes` | Núcleo |
| `ms-estoque` | Núcleo |
| `ms-pedidos` | Núcleo |
| `ms-fornecedores` | Núcleo |
| `ms-qualidade` | Núcleo |
| `ms-clientes` | Núcleo |
| `ms-pagamentos` | Núcleo |
| `ms-identidade` | Satélite |
| `ms-notificacoes` | Satélite |
| `ms-analytics` | Satélite |

Tópicos de evento seguem a lista já escrita na parte 5, para não haver duas grafias: `pedido.criado`, `estoque.reservado`, `pagamento.aprovado`, `pagamento.recusado`, `lote.criado`, `qualidade.leitura`, `qualidade.alerta`, `notificacao.enviar` e `cliente.anonimizado`. Formato `<dominio>.<evento>`, minúsculo, sem prefixo de turma; fila morta como `<topico>.DLQ`.

## 7. Pontos a confirmar com o grupo

Registrado em 22/09/2026, antes do início das partes 2 e 3:

- **Evento `recebimento.confirmado`** — proposto neste item para ligar o recebimento de compra (`ms-fornecedores`) ao `ms-estoque`. Ele **não** está na lista de tópicos já escrita na parte 5; a confirmação do nome fica com a parte 4 (comunicação entre serviços).
- **Serviços de núcleo** — 9 de núcleo e 3 satélites, total de 12 (item 1.2). Se o grupo quiser um serviço de núcleo a mais, o candidato é extrair o carrinho do `ms-pedidos`.
- **`MovimentacaoEstoque`** — nome canônico da entidade que hoje existe como `TransacaoEstoque.cs`; a parte 3 usa este nome no Database per Service.

## Registro de revisão

- **v1.0 (21/09/2026)** — conteúdo original do item 1, entregue em PDF (Roger Viana Gonçalves de Alencar).
- **v1.1 (22/09/2026)** — revisão de consolidação (Yasmin Kimura, Pessoa 5): reconciliação com a lista de partida, definição de posse de `Safra` e de `MovimentacaoEstoque`, quem escreve o movimento de entrada do recebimento, fronteiras justificadas para `ms-clientes`, `ms-fornecedores` e `ms-analytics`, caminhos completos dos arquivos do repositório e conversão para `P1_servicos.md`.
