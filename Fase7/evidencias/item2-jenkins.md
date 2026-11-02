# Evidências — Fase 7, item 2: repositório + pipeline Jenkins com build e deploy

Ambiente: Jenkins LTS 2.580.2 em container (`Fase7/jenkins/docker-compose.yml`), imagem própria com
.NET SDK 10 (`Fase7/jenkins/Dockerfile`), job `vinheria-pipeline` criado sem cliques
(`init.groovy.d/02-job.groovy`) apontando para o `Jenkinsfile` versionado no repositório.
Coleta em 02/11/2026.

## 1. Histórico de builds: duas execuções verdes

```
$ curl -s -u admin:*** "http://localhost:8080/job/vinheria-pipeline/api/json?tree=builds[number,result]"
{"builds":[{"number":2,"result":"SUCCESS"},{"number":1,"result":"SUCCESS"}]}
```

Prints da interface do Jenkins (job e console do build #2):

- `Fase7/evidencias/jenkins-historico-builds.png` — builds #1 (14:43) e #2 (14:46), ambos verdes
- `Fase7/evidencias/jenkins-console-build2.png` — console do build #2 (checkout do commit `a7fd682`)

## 2. Build #1 — primeiro deploy (commit e27df36)

```
Deploy da Vinheria
SDK: 10.0.403
  Pedidos.Api -> /var/jenkins_home/workspace/vinheria-pipeline/out/pedidos/
artefatos gerados:
out/catalogo/Catalogo.Api.dll
publicado: /var/jenkins_home/publicacao/catalogo
publicado: /var/jenkins_home/publicacao/pedidos
--- prova do deploy (arquivo publicado com data/hora) ---
-rw-r--r-- 1 jenkins jenkins 89 Nov  2 14:44 /var/jenkins_home/publicacao/catalogo/BUILD_INFO.txt
-rw-r--r-- 1 jenkins jenkins 88 Nov  2 14:44 /var/jenkins_home/publicacao/pedidos/BUILD_INFO.txt
servico=pedidos
build=1
commit=e27df36
branch=origin/main
data_hora=2026-11-02 14:44:57
Finished: SUCCESS
```

## 3. Build #2 — com mudança de código (versão 1.1.0, commit a7fd682)

```
Deploy da Vinheria
SDK: 10.0.403
  Catalogo.Api -> .../out/catalogo/
  Pedidos.Api -> .../out/pedidos/
publicado: /var/jenkins_home/publicacao/catalogo
publicado: /var/jenkins_home/publicacao/pedidos
--- prova do deploy (arquivo publicado com data/hora) ---
-rw-r--r-- 1 jenkins jenkins 89 Nov  2 14:46 /var/jenkins_home/publicacao/pedidos/BUILD_INFO.txt
build=2
commit=a7fd682
data_hora=2026-11-02 14:46:43
Finished: SUCCESS
```

O console mostra o checkout da revisão nova (`feat(fase7): versão 1.1.0 dos serviços…`), ou seja, o
build 2 não é um re-run do primeiro.

## 4. Artefato na pasta de publicação (visto do host)

```
$ ls -l Fase7/jenkins/publicacao/*/BUILD_INFO.txt
-rw-r--r-- 1 yasmi 197609 89 nov  2 11:46 .../publicacao/catalogo/BUILD_INFO.txt
-rw-r--r-- 1 yasmi 197609 88 nov  2 11:46 .../publicacao/pedidos/BUILD_INFO.txt

$ cat Fase7/jenkins/publicacao/pedidos/BUILD_INFO.txt
servico=pedidos
build=2
commit=a7fd682
branch=origin/main
data_hora=2026-11-02 14:46:43

$ ls Fase7/jenkins/publicacao/catalogo
BUILD_INFO.txt  Catalogo.Api  Catalogo.Api.deps.json  Catalogo.Api.dll
Catalogo.Api.pdb  Catalogo.Api.runtimeconfig.json  Catalogo.Api.staticwebassets.endpoints.json  web.config
```

## Prints (imagens)

- `Fase7/evidencias/jenkins-historico-builds.png` — builds #1 e #2 no histórico do job
- `Fase7/evidencias/jenkins-console-build2.png` — console do build #2 (checkout do commit `a7fd682`)
- `Fase7/evidencias/prints/publicacao.png` — pasta de publicação vista do host, com `BUILD_INFO.txt` e
  a data/hora do build

## Como reproduzir

```bash
cd Fase7/jenkins
docker compose up -d --build      # Jenkins em http://localhost:8080 (usuário admin)
# o job vinheria-pipeline já existe, criado pelo init.groovy.d
curl -u admin:<senha> -X POST http://localhost:8080/job/vinheria-pipeline/build
```

Pasta de publicação: `Fase7/jenkins/publicacao/` no host
(`/var/jenkins_home/publicacao` no container). Cada build sobrescreve os dois serviços e grava
`BUILD_INFO.txt` com build, commit e data/hora — é a prova do deploy.
