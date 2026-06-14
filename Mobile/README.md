# Vinheria Agnello - Persistência Local (Room)

Este projeto contém a fundação da camada de dados local para o aplicativo da **Vinheria Agnello**, desenvolvida seguindo os princípios de Clean Architecture e SOLID. O objetivo principal foi substituir o armazenamento em memória por uma persistência robusta utilizando o banco de dados SQLite através da biblioteca Room.

## 🛠️ O que foi construído (Sprint 1)

A configuração atual foca na infraestrutura de dados, garantindo consistência total com os contratos do servidor C#.

### 1. Arquitetura de Dados
- **Entity (`Produto.kt`):** Mapeamento exato da entidade Vinho, com campos para ID, Nome, Tipo, Safra, Quantidade e Preço.
- **DAO (`ProdutoDao.kt`):** Interface que define as operações de CRUD. Todas as funções são `suspend` (Coroutines) e a listagem utiliza `Flow` para atualizações reativas.
- **Database (`AppDatabase.kt`):** Classe central do Room que gerencia a conexão e a versão do banco de dados.

### 2. Configuração Técnica
- **Kotlin Symbol Processing (KSP):** Implementado para geração de código mais performática.
- **Room 2.8.4:** Versão estável com suporte a Coroutines.
- **Mapeamento C#:** Tipos de dados e nomes de colunas escolhidos para manter paridade com o backend original.

---

## 🚀 Próximos Passos (Handover para Kevin)

A estrutura de persistência está pronta para ser consumida pela interface do usuário. Abaixo está o guia modular para a próxima fase do desenvolvimento.

### 📋 Prompt para o Desenvolvedor Mobile B (Kevin)

> **Contexto:** Kevin, a fundação de dados (Room) da Vinheria Agnello já está implementada e validada com o banco de dados local. Sua missão é conectar a lógica de negócio às telas do aplicativo.
>
> **Tarefa:** Implementar a lógica de UI e o Repository Pattern para gerenciar o estoque de vinhos.
>
> **Atividades Sugeridas:**
> 1. **Repository Pattern:** Crie uma classe `ProdutoRepository` que intermedeia o acesso entre o `ProdutoDao` e a UI. Isso garantirá a inversão de dependência.
> 2. **ViewModel:** Desenvolva uma `ProdutoViewModel` para gerenciar o estado da tela, coletando o `Flow` de produtos do DAO.
> 3. **Interface de Usuário (Compose/XML):**
>    - **Lista de Estoque:** Implementar a visualização que se atualiza automaticamente ao detectar mudanças no banco.
>    - **Formulário de Cadastro/Edição:** Telas para entrada de dados dos vinhos.
>    - **Operações de Exclusão:** Gestão de remoção de itens com feedback visual.
> 4. **Integração:** Conectar os eventos de clique dos botões às funções `insert`, `update` e `delete` do repositório.
>
> **Dica Técnica:** Como o `ProdutoDao` já retorna um `Flow<List<Produto>>`, utilize `collectAsStateWithLifecycle()` (se estiver usando Compose) para garantir que a lista na tela reflita o banco de dados em tempo real sem vazamentos de memória.

---

## 📂 Estrutura de Pastas Criada
```text
app/src/main/java/com/example/myapplication/data/local/
├── entity/          # Produto.kt (Definição da tabela)
├── dao/             # ProdutoDao.kt (Comandos SQL/CRUD)
└── AppDatabase.kt   # Configuração central do banco
```

## ✅ Requisitos Atendidos
- [x] Configuração de dependências (Room, KSP, Coroutines).
- [x] Entidade com mapeamento C# equivalente.
- [x] DAO com Princípio de Responsabilidade Única.
- [x] Banco de Dados exportando o esquema corretamente.
