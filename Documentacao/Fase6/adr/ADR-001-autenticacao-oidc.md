# ADR-001 — OAuth 2.0 / OpenID Connect com Keycloak no lugar de sessão de servidor

- **Data:** 16/09/2026
- **Status:** aceita
- **Contexto:** o monolito web autentica por sessão HTTP e guarda credenciais em memória, sem *hash* e com usuário `admin` de senha padrão (`Web/src/main/java/br/com/fiap/vinheriaagnello/repository/InMemoryDatabase.java`). Sessão em processo impede escala horizontal e não sobrevive a *restart*; nenhum serviço novo deve repetir esse modelo.
- **Decisão:** a identidade fica no `ms-identidade` (Keycloak, OIDC). O acesso é por **JWT de vida curta (5 min)** com *refresh token* rotativo; **nenhum serviço de negócio guarda senha ou tela de login**; papéis viajam em *claim* (sem dado pessoal no token).
- **Consequências:**
  - Positivas: serviços ficam *stateless* e escaláveis horizontalmente; revogação e 2FA passam a ser possíveis; um só lugar guarda credencial.
  - Negativas: o `ms-identidade` vira dependência crítica — mitigado com cache de JWKS nos serviços e tolerância a indisponibilidade curta na validação; e exige ajuste de fluxo no app mobile (*Authorization Code* + PKCE, token no Android Keystore).
- **Alternativas descartadas:** manter sessão de servidor com Redis (resolve o estado, mas não resolve credencial em poder do serviço nem 2FA/revogação); autenticação própria por token estático (sem expiração nem revogação).
