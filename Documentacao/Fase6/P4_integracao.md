# Fase 6 — Arquitetura de APIs e Microsserviços
## Item 4 — Integração: comunicação síncrona x assíncrona

| Campo | Valor |
|---|---|
| Arquivo | `Documentacao/Fase6/P4_integracao.md` |
| Responsável | Arthur Cei Corrêa — RM 97781 |
| Período | 29/09/2026 → 05/10/2026 |
| Dependências | `P1_servicos.md`, `P2_arquitetura.md`, `P3_padroes.md` e `P5_seguranca_governanca.md` |
| Fonte dos nomes | Glossário congelado do P1 e tópicos representados no P2 |
| Versão | 1.2 — revisão de 01/10/2026 |

Este item define **como** os serviços se integram. O P3 explica **por que** a compra usa Saga por coreografia; aqui estão os canais, contratos e decisões concretas dessa Saga.

## 1. Regra de decisão

Usamos **síncrono (REST/HTTPS com JSON)** quando o usuário está esperando e a resposta é necessária para continuar: consulta de catálogo, consulta de saldo de estoque, validação de cliente, criação do pedido, autorização no gateway e consulta pública de rastreio. O gateway é a única porta dos clientes; entre serviços, o tráfego usa HTTPS com mTLS na malha.

Usamos **assíncrono (Kafka)** quando o efeito pode acontecer depois, precisa absorver picos ou vários interessados precisam saber do fato: reserva e liberação de estoque, resultado do pagamento, criação de lote, telemetria, alerta de qualidade, notificação e anonimização. O produtor executa sua transação local e tenta publicar o evento com confirmação de entrega; os consumidores processam em seu próprio ritmo.

O custo do síncrono é o **acoplamento temporal**: uma cadeia de chamadas pode produzir falha em cascata. Toda chamada tem timeout, retry apenas quando idempotente e Circuit Breaker, conforme o P5; o gateway não transforma uma indisponibilidade de um serviço em espera infinita.

O custo do assíncrono é a **consistência eventual**. Kafka trabalha com entrega pelo menos uma vez: mensagens podem ser duplicadas, chegar atrasadas ou, entre partições, fora de ordem. Por isso cada evento tem `idEvento`, cada consumidor guarda os IDs já aplicados, a chave de partição preserva a ordem por `pedidoId`, `loteId` ou `deviceId`, e mensagens que excedem as tentativas vão para `<topico>.DLQ`. Lag, taxa de erro e tamanho da DLQ são monitorados.

## 2. Matriz de comunicação

Cada linha representa uma relação direta. A chamada passa pelo API Gateway quando a origem é um canal; chamadas internas usam a malha com mTLS. Os endpoints são versionados conforme a convenção do P5.

| Origem → destino | Motivo | Tipo | Tópico, se assíncrono | Endpoint, se síncrono |
|---|---|---|---|---|
| Web/App/Painel → API Gateway | Entrada única, JWT, rate limit e roteamento | Síncrono REST/HTTPS | — | `/api/*` |
| API Gateway → `ms-identidade` | Login, emissão e renovação de tokens; o gateway valida o JWT localmente via JWKS, sem introspecção em cada requisição | Síncrono REST/HTTPS | — | `POST /v1/oauth/token` |
| API Gateway → `ms-catalogo` | Consulta de vinhos e categorias para exibir a oferta | Síncrono REST/HTTPS | — | `GET /v1/vinhos` |
| API Gateway → `ms-estoque` | Consulta de posição disponível no dia a dia | Síncrono REST/HTTPS | — | `GET /v1/estoque/posicoes?produtoId=...` |
| API Gateway → `ms-pedidos` | Criar e consultar pedido | Síncrono REST/HTTPS | — | `POST /v1/pedidos` e `GET /v1/pedidos/{pedidoId}` |
| API Gateway → `ms-clientes` | Consultar ou alterar perfil e consentimento | Síncrono REST/HTTPS | — | `GET /v1/clientes/{clienteId}` |
| API Gateway → `ms-lotes` | Consulta pública do QR Code e genealogia do lote | Síncrono REST/HTTPS | — | `GET /v1/lotes/{loteId}/rastreio` |
| `ms-pedidos` → `ms-catalogo` | Validar produto, preço vigente e dados do item antes de gravar o pedido | Síncrono REST/HTTPS | — | `GET /v1/vinhos/{vinhoId}` |
| `ms-pedidos` → `ms-clientes` | Validar cliente, endereço e consentimento necessário à venda | Síncrono REST/HTTPS | — | `GET /v1/clientes/{clienteId}/validacao-pedido` |
| `ms-pedidos` → Kafka → `ms-estoque`, `ms-clientes` e `ms-analytics` | Solicitar reserva sem bloquear o cliente, atualizar o histórico derivado do cliente e alimentar o read-model analítico | Assíncrono Kafka | `pedido.criado` | — |
| `ms-estoque` → Kafka → `ms-pagamentos`, `ms-pedidos` e `ms-analytics` | Liberar a cobrança depois da reserva, atualizar o pedido e alimentar o read-model analítico | Assíncrono Kafka | `estoque.reservado` | — |
| `ms-pagamentos` → Kafka → `ms-pedidos`, `ms-estoque` e `ms-analytics` | Informar resultado da cobrança; confirmar, compensar e alimentar o read-model analítico | Assíncrono Kafka | `pagamento.aprovado` ou `pagamento.recusado` | — |
| `ms-pedidos` → Kafka → `ms-notificacoes` e `ms-analytics` | Avisar criação, confirmação ou cancelamento e registrar o fato no read-model analítico sem atrasar a resposta do pedido | Assíncrono Kafka | `notificacao.enviar` | — |
| `ms-pagamentos` → `ms-pedidos` | Consulta administrativa de status e conciliação, quando uma leitura imediata for necessária | Síncrono REST/HTTPS | — | `GET /v1/pagamentos/{pedidoId}` |
| `ms-producao` → Kafka → `ms-lotes` e `ms-analytics` | Criar o registro de rastreabilidade e alimentar o read-model analítico | Assíncrono Kafka | `lote.criado` | — |
| `ms-fornecedores` → Kafka → `ms-estoque` e `ms-analytics` | Informar recebimento conferido; o `ms-estoque` registra a entrada e o analytics atualiza seus indicadores | Assíncrono Kafka | `recebimento.confirmado` | — |
| Bridge MQTT → Kafka → `ms-qualidade` | Transformar telemetria em contrato interno de eventos | Assíncrono Kafka | `qualidade.leitura` | — |
| `ms-qualidade` → Kafka → `ms-estoque`, `ms-producao` e `ms-analytics` | Distribuir alerta fora da faixa para ação, operação e histórico | Assíncrono Kafka | `qualidade.alerta` | — |
| `ms-qualidade` → Kafka → `ms-notificacoes` | Solicitar aviso operacional sobre uma condição crítica | Assíncrono Kafka | `notificacao.enviar` | — |
| `ms-clientes` → Kafka → `ms-analytics` e `ms-notificacoes` | Propagar anonimização sem varredura manual em cópias derivadas | Assíncrono Kafka | `cliente.anonimizado` | — |

`recebimento.confirmado` é mantido porque aparece no P2 como tópico proposto para ligar `ms-fornecedores` a `ms-estoque`. Caso o grupo remova essa proposta, a linha deve ser removida simultaneamente do P2 e deste catálogo.

## 3. Catálogo de eventos e tópicos

Propomos um envelope comum para os eventos: `idEvento`, `tipo`, `versao`, `ocorridoEm`, `traceId`, `chaveParticao` e `payload`. O resumo abaixo descreve o payload de negócio, não o envelope completo.

| Tópico | Payload resumido | Produtor | Consumidores | Idempotente obrigatório? |
|---|---|---|---|---|
| `pedido.criado` | `pedidoId`, `clienteId`, itens, totais, endereço resumido, `correlationId` | `ms-pedidos` | `ms-estoque`, `ms-clientes`, `ms-analytics` | Sim; uma reserva e uma atualização de histórico por `pedidoId` |
| `estoque.reservado` | `pedidoId`, `reservaId`, itens, quantidades e expiração | `ms-estoque` | `ms-pagamentos`, `ms-pedidos`, `ms-analytics` | Sim; não duplicar cobrança nem transição |
| `pagamento.aprovado` | `pedidoId`, `pagamentoId`, valor, moeda, data e autorização | `ms-pagamentos` | `ms-pedidos`, `ms-estoque`, `ms-analytics` | Sim; não confirmar ou baixar duas vezes |
| `pagamento.recusado` | `pedidoId`, `pagamentoId`, motivo codificado e data | `ms-pagamentos` | `ms-pedidos`, `ms-estoque`, `ms-analytics` | Sim; liberar a reserva uma única vez |
| `lote.criado` | `loteId`, `safraId`, origem, data, quantidade e dados de genealogia | `ms-producao` | `ms-lotes`, `ms-analytics` | Sim; um QR e uma genealogia por `loteId` |
| `recebimento.confirmado` | `recebimentoId`, fornecedor, itens, lote(s), quantidades e divergências | `ms-fornecedores` | `ms-estoque`, `ms-analytics` | Sim; não lançar a mesma entrada duas vezes |
| `qualidade.leitura` | `deviceId`, `adegaId`, `loteId` opcional, temperatura, umidade, luminosidade e medição | Bridge MQTT | `ms-qualidade` | Sim por `idEvento`; tolerar retry da bridge |
| `qualidade.alerta` | `alertaId`, `deviceId`, `adegaId`, tipo, valor, limite, severidade e intervalo | `ms-qualidade` | `ms-estoque`, `ms-producao`, `ms-analytics` | Sim por `alertaId` e janela de alerta |
| `notificacao.enviar` | `notificacaoId`, destinatário, canais, template, parâmetros e referência | `ms-pedidos` ou `ms-qualidade` | `ms-notificacoes`, `ms-analytics` | Sim por `notificacaoId`; retry não pode reenviar indevidamente |
| `cliente.anonimizado` | `clienteId`, data, motivo, versão da política e escopos a limpar | `ms-clientes` | `ms-analytics`, `ms-notificacoes` | Sim; anonimização é operação monotônica |

O consumidor confirma o offset somente depois de concluir a transação local. Um evento duplicado é reconhecido pelo `idEvento` ou pela chave natural do domínio e não reaplica o efeito. Depois do limite de retry, o evento vai para, por exemplo, `pagamento.aprovado.DLQ`; a DLQ tem retenção de sete dias, alerta operacional e procedimento de reprocessamento após correção. Como Transactional Outbox não faz parte do P2/P3, a publicação de cada evento deve usar retry do produtor, confirmação de entrega e alerta de reconciliação para o caso de a transação local ser confirmada e a publicação falhar; a adoção futura de Outbox exigiria atualização do diagrama e do P3.

## 4. Fluxos ponta a ponta

### 4.1 Compra de vinho

1. O app envia `POST /v1/pedidos` ao gateway. O gateway valida o JWT e encaminha ao `ms-pedidos`, que consulta o `ms-catalogo` para preço/produto e o `ms-clientes` para a validação do comprador. A resposta síncrona confirma apenas que o pedido foi aceito, não que o pagamento terminou.
2. O `ms-pedidos` grava o pedido como `aguardando_reserva` e publica `pedido.criado`. O `ms-estoque` consome, verifica a posição, cria a Reserva e publica `estoque.reservado`.
3. O `ms-pagamentos` consome `estoque.reservado`, cobra o meio tokenizado e publica `pagamento.aprovado` ou `pagamento.recusado`. Em aprovação, `ms-pedidos` confirma o pedido e publica `notificacao.enviar`; `ms-notificacoes` avisa o cliente e `ms-analytics` registra o fato. Em paralelo, `ms-estoque` consome `pagamento.aprovado` e faz a baixa definitiva. Essa ordem é uma sequência de negócio, não uma cadeia síncrona: consumidores Kafka podem processar em paralelo e não há garantia temporal entre o aviso e a baixa. O `ms-lotes` não participa da baixa da venda, pois sua rastreabilidade é criada pelo evento `lote.criado`.
4. Em recusa, timeout definitivo ou antifraude negativo, `pagamento.recusado` faz o `ms-estoque` liberar a Reserva e o `ms-pedidos` marcar o pedido como `cancelado`; `ms-notificacoes` informa a falha. Essa é a compensação da Saga: não há transação distribuída e o serviço que criou a reserva é o único que a desfaz.
5. Se uma mensagem for repetida, `pedidoId`, `reservaId` e `pagamentoId` impedem duplicar pedido, reserva, cobrança, baixa ou aviso. Se houver mensagem inválida, o consumidor não bloqueia a partição indefinidamente: envia para a DLQ e gera alerta para reprocessamento.

### 4.2 Alerta de temperatura da adega

O sensor Arduino publica a leitura no caminho MQTT existente; a bridge/Node-RED autentica-se no broker e transforma o dado no evento Kafka `qualidade.leitura`, particionado por `deviceId`. O `ms-qualidade` persiste a série temporal, compara temperatura e umidade com a faixa da adega e, fora do limite, publica `qualidade.alerta` para `ms-estoque`, `ms-producao` e `ms-analytics`. Para avisar pessoas, publica `notificacao.enviar`, consumido pelo `ms-notificacoes`.

O alerta também fica registrado para auditoria com `alertaId`, valor, limite, horário e severidade. Uma leitura duplicada não gera dois alertas porque o consumidor usa `idEvento` e a regra de deduplicação por `alertaId`/janela. Se `ms-notificacoes` estiver indisponível, o Kafka acumula as mensagens; se o processamento falhar repetidamente, `notificacao.enviar.DLQ` recebe a mensagem e o lag dispara um alerta, sem interromper a ingestão do sensor.

### 4.3 Rastreabilidade e consulta do dia a dia

No fluxo produtivo, o `ms-producao` registra o ciclo da safra e publica `lote.criado`. O `ms-lotes` consome o evento, cria o QR e a genealogia do lote de forma idempotente. Quando o consumidor lê o QR, consulta `GET /v1/lotes/{loteId}/rastreio` no gateway; o endpoint é público e retorna apenas os dados mínimos de origem, safra e histórico.

Separadamente, para a operação diária, o app mobile solicita `GET /v1/estoque/posicoes...` ao gateway. O gateway chama o `ms-estoque`, que responde o saldo atual na hora. Isso é síncrono porque o usuário precisa decidir a venda ou reposição com a posição conhecida; o app pode mostrar o último saldo com aviso de desatualização apenas como degradação controlada, nunca como confirmação de reserva.

## 5. Consistência com P1, P2 e P5

- Serviços: foram usados somente os 12 nomes congelados no glossário do P1.
- Tópicos: aparecem os 10 tópicos do P2: `pedido.criado`, `estoque.reservado`, `pagamento.aprovado`, `pagamento.recusado`, `lote.criado`, `recebimento.confirmado`, `qualidade.leitura`, `qualidade.alerta`, `notificacao.enviar` e `cliente.anonimizado`. Os consumidores de `pedido.criado` e `notificacao.enviar` também incluem `ms-clientes`/`ms-analytics`, conforme o P2. `recebimento.confirmado` permanece marcado como proposta; se for retirado pelo grupo, deve ser removido dos dois documentos.
- Resiliência: timeout, Circuit Breaker, retry restrito a operações idempotentes, DLQ, idempotência por `idEvento` e monitoramento de lag seguem os parâmetros da seção 6 do P5.
- Segurança: clientes entram pelo gateway; chamadas internas usam mTLS; nenhum serviço acessa diretamente o banco de outro serviço.

## Registro de revisão

- **v1.2 (01/10/2026)** — revisão editorial para alinhar cabeçalho ao padrão das demais partes e distinguir a proposta de envelope de evento de um contrato já implementado.
