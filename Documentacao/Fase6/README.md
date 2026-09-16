# Fase 6 — Arquitetura de APIs e Microsserviços (Vinheria Agnello)

Atividade: modelar a arquitetura de um sistema baseado em APIs e microsserviços para a Vinheria Agnello.
Prazo do grupo: **16/09/2026 → 16/10/2026** (entrega única em `.docx`, 5 seções na ordem do enunciado).

## Divisão e arquivos

| Parte | Item da atividade | Responsável | Prazo | Arquivo |
|---|---|---|---|---|
| 1 | Identificação dos serviços | Roger | 16/09 → 21/09 (congelamento) | `P1_servicos.md` |
| 2 | Diagrama de arquitetura | Kevin | 22/09 → 28/09 | `P2_arquitetura.*` |
| 3 | Padrões de arquitetura justificados | André | 22/09 → 30/09 | `P3_padroes.md` |
| 4 | Integração (síncrono x assíncrono) | Arthur | 29/09 → 05/10 | `P4_integracao.md` |
| 5 | **Segurança, governança e consolidação** | **Yasmin** | Parte 1: 16/09 → 24/09 · Parte 2 (`.docx`): 06/10 → 13/10 | [`P5_seguranca_governanca.md`](P5_seguranca_governanca.md) |

Decisões registradas (ADR): [`adr/`](adr/)

## Regras de consistência (valem para todos os arquivos)

1. **Uma única grafia** para nome de serviço (`ms-<dominio>`), tópico (`dominio.evento`) e papel de usuário. A lista congelada do P1 é a fonte; quem precisar mudar, avisa no grupo antes — diagrama e texto precisam bater.
2. Cada parte escreve em **Markdown simples** (`##`, parágrafos, tabelas). A consolidação do `.docx` (Parte 2 do P5) cola sem retrabalho de formatação.
3. Nada de imagem colada só no chat: diagrama do P2 precisa entrar no repositório em `.drawio` + `.png` **e** `.svg`.
4. Toda afirmação técnica sobre o sistema atual deve citar o caminho do arquivo no repositório.

## Ordem do documento final (enunciado)

1. Lista de microsserviços e suas funções (P1)
2. Diagrama de arquitetura (P2)
3. Padrões de arquitetura e justificativas (P3)
4. Comunicação entre serviços: síncrona x assíncrona (P4)
5. Segurança e governança — autenticação/autorização, escalabilidade e resiliência (P5)

## Entrega

- Arquivo único `.docx`: `Nomecompleto_rm_turma_fase6_atividade.docx` (turma **3ESOA**), com capa (nome, RM, turma), sumário, as 5 seções e, no fim, a tabela de divisão de tarefas.
- Se o portal exigir PDF, gerar o PDF com o mesmo prefixo. Se o upload for individual (padrão FIAP), cada integrante sobe a mesma versão com o próprio nome/RM.
