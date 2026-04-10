# Vinheria Agnello

Aplicação web Java (Jakarta EE) com Servlets e JSP, construída com Maven e deployada no Google Cloud Compute Engine.

**Stack:** Java 17 · Jakarta Servlet 6 · JSP · JSTL · Tomcat 10 · Maven

---

## Acesso

**URL pública:** `http://34.74.234.182:8080`

Para testar, cadastre um usuário na tela de cadastro ou use qualquer e-mail/senha já registrado.

---

## Infraestrutura — Google Cloud Compute Engine

| Item | Valor |
|---|---|
| Projeto | `project-afd32909-8926-457c-aa2` |
| Máquina | `e2-micro` (free tier) |
| Zona | `us-east1-b` |
| SO | Debian 12 |
| Disco | 30 GB Standard (`pd-standard`) |
| IP público | `34.74.234.182` |

> A instância `e2-micro` em `us-east1`, `us-east4` ou `us-west1` é elegível ao **Always Free** do GCP — sem custo mensal.

---

## Como foi feito o deploy

### 1. Pré-requisitos locais

- Google Cloud CLI instalado via tarball (Fedora 43 não tem suporte oficial ao repositório DNF do GCP)
- Autenticação via `gcloud init`

### 2. Criar a VM

```bash
gcloud compute instances create vinheria-vm \
  --project=project-afd32909-8926-457c-aa2 \
  --zone=us-east1-b \
  --machine-type=e2-micro \
  --image-family=debian-12 \
  --image-project=debian-cloud \
  --boot-disk-size=30GB \
  --boot-disk-type=pd-standard \
  --tags=http-server
```

### 3. Abrir a porta 8080

```bash
gcloud compute firewall-rules create allow-8080 \
  --project=project-afd32909-8926-457c-aa2 \
  --allow tcp:8080 \
  --target-tags http-server
```

### 4. Acessar a VM

```bash
gcloud compute ssh vinheria-vm \
  --zone=us-east1-b \
  --project=project-afd32909-8926-457c-aa2
```

### 5. Instalar dependências na VM

```bash
# Java 17 + Maven (Java 21 não está disponível no Debian 12 sem configuração extra)
sudo apt update && sudo apt install -y openjdk-17-jdk maven git

# Tomcat 10.1.54
wget https://dlcdn.apache.org/tomcat/tomcat-10/v10.1.54/bin/apache-tomcat-10.1.54.tar.gz
tar -xzf apache-tomcat-10.1.54.tar.gz
sudo mv apache-tomcat-10.1.54 /opt/tomcat
sudo chmod +x /opt/tomcat/bin/*.sh
```

### 6. Adicionar swap (necessário no e2-micro com 1 GB RAM)

```bash
sudo fallocate -l 1G /swapfile
sudo chmod 600 /swapfile
sudo mkswap /swapfile
sudo swapon /swapfile
```

### 7. Clonar, buildar e deployar

```bash
git clone https://github.com/CodeSquad-FIAP/VinheriaAgnello.git
cd VinheriaAgnello
mvn package -DskipTests

/opt/tomcat/bin/shutdown.sh
rm -rf /opt/tomcat/webapps/ROOT /opt/tomcat/webapps/ROOT.war
cp target/vinheriaagnello.war /opt/tomcat/webapps/ROOT.war
/opt/tomcat/bin/startup.sh
```

---

## Observações

- O `pom.xml` foi ajustado de Java 21 para Java 17, pois o Debian 12 não oferece `openjdk-21-jdk` no repositório padrão.
- O armazenamento de dados é **in-memory** (`InMemoryDatabase.java`) — os dados são perdidos ao reiniciar o Tomcat. Isso é esperado para o escopo do projeto.
- O Tomcat redireciona `/` para `/auth/login` via `RootRedirectServlet` — o HTTP 302 ao acessar a raiz é comportamento correto.
