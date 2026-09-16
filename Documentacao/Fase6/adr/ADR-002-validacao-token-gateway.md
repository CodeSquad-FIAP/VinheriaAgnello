# ADR-002 — Validação de token centralizada no API Gateway

- **Data:** 16/09/2026
- **Status:** aceita
- **Contexto:** com 10 a 12 serviços (lista do P1), validar JWT em cada serviço duplicaria lógica de verificação, acoplaria todos ao Keycloak e multiplicaria a chance de implementação inconsistente (um serviço esquecer de validar assinatura, por exemplo).
- **Decisão:** o **API Gateway** valida assinatura (JWKS), emissor, público, expiração e escopo, e propaga a identidade em cabeçalho assinado (`X-Identidade` + assinatura do gateway) junto do `traceId`. Os serviços **revalidam a assinatura do gateway** e fazem a **autorização fina** (papel + atributo + objeto). Serviços de risco (`ms-pagamentos`) revalidam também o JWT original.
- **Consequências:**
  - Positivas: uma política de borda, menos código repetido, um lugar para rate limit e quotas, e impossibilidade de o cliente forjar identidade direto contra um serviço (o cabeçalho é assinado).
  - Negativas: o gateway vira ponto único de política — mitigado com múltiplas réplicas, *health check*, limites de recurso e *circuit breaker* nos serviços; e qualquer troca de política exige *deploy* do gateway.
- **Alternativas descartadas:** validar em todos os serviços (mais seguro, porém acoplante e repetitivo); rede interna "confiável" sem autenticação mútua (fere o princípio de confiança zero e não resiste a movimentação lateral).
