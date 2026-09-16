# ADR-003 — Identidade MQTT por dispositivo com ACL de publicação

- **Data:** 16/09/2026
- **Status:** aceita
- **Contexto:** a integração atual da adega usa **uma credencial compartilhada** no HiveMQ Cloud com permissão de **Publish + Subscribe** em `Fiap/iot/3ESOA/CodeSquad/#` (`MQTT/README_MQTT.md`, porta 8883 com TLS). Qualquer portador da credencial pode publicar leitura falsa de temperatura da adega — e a telemetria é justamente o que dispara alerta de qualidade.
- **Decisão:** cada adega recebe **usuário próprio** no broker (`adega-<id>`), com **ACL restrita a publicar** no seu tópico de telemetria e sem permissão de assinar o *namespace* de comandos. O caminho sensor → ponte (`rfc2217://localhost:4000`) → Node-RED → broker permanece igual: **o firmware não muda** (`Arduino/VinheriaSensores/VinheriaSensores.ino`). Na migração, uma ponte mapeia o tópico físico legado `Fiap/iot/3ESOA/CodeSquad/sensor` para o evento `qualidade.leitura`.
- **Consequências:**
  - Positivas: leitura de adega passa a ser atribuível a um dispositivo, com trilha de auditoria; publicar no tópico de outra adega deixa de ser possível; rotação de credencial passa a ser por dispositivo, sem parar a operação inteira.
  - Negativas: é preciso provisionar e rotacionar credencial por dispositivo (automatizado no cadastro da adega) e manter a ponte de tópico legado enquanto o firmware não for atualizado.
- **Alternativas descartadas:** manter credencial compartilhada com escopo reduzido (qualquer adega comprometida falsifica leitura de qualquer outra); migrar o firmware para HTTPS (o Arduino Uno não tem Wi-Fi; exigiria trocar o hardware, fora do escopo desta fase).
