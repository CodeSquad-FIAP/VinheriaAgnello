# Jenkins LTS + .NET SDK 10 (o stage Build compila os microsserviços dentro do próprio container)
FROM jenkins/jenkins:lts-jdk21

USER root
# libicu76 é obrigatório: o SDK .NET aborta (SIGABRT) sem ICU na imagem trixie do Jenkins
RUN apt-get update \
 && apt-get install -y --no-install-recommends curl git ca-certificates libicu76 \
 && rm -rf /var/lib/apt/lists/*

# .NET SDK 10 (mesma versão usada no desenvolvimento dos serviços)
RUN curl -sSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh \
 && bash /tmp/dotnet-install.sh --channel 10.0 --install-dir /usr/share/dotnet \
 && ln -sf /usr/share/dotnet/dotnet /usr/local/bin/dotnet \
 && rm /tmp/dotnet-install.sh \
 && dotnet --version

USER jenkins
# plugins mínimos: pipeline, git e timestamps no console
RUN jenkins-plugin-cli --plugins workflow-aggregator git timestamper

# configuração inicial automatizada: usuário admin e job do pipeline (sem cliques no wizard)
COPY init.groovy.d/ /usr/share/jenkins/ref/init.groovy.d/
ENV JAVA_OPTS="-Djenkins.install.runSetupWizard=false"

EXPOSE 8080 50000
