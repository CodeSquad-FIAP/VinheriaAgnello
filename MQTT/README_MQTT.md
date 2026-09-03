Este diretório contém os arquivos responsáveis pela integração entre o Arduino Uno simulado no Wokwi, o Node-RED e o broker MQTT HiveMQ Cloud. A implementação segue a arquitetura definida para a etapa de Comunicação & Broker MQTT:

Componente

Função

Configuração principal

Wokwi / Arduino Uno

Gera as leituras dos sensores em JSON

Serial em 9600 baud

Ponte Python

Encaminha a serial simulada do Wokwi para o Node-RED

rfc2217://localhost:4000

Node-RED

Recebe o JSON e publica via MQTT

Fluxo em node_red_vinheria_flow.json

HiveMQ Cloud

Broker MQTT responsável por receber e distribuir as mensagens

Porta 8883 com TLS

O Arduino Uno utilizado no projeto não possui conectividade Wi-Fi. Por isso, a integração utiliza a serial simulada do Wokwi e uma ponte em Python para encaminhar os dados ao Node-RED.

Arquivos

MQTT/bridge_wokwi_nodered.py: faz a ponte entre a serial simulada do Wokwi e o Node-RED.

MQTT/node_red_vinheria_flow.json: fluxo exportado do Node-RED utilizado para publicar e conferir as mensagens MQTT.

Arduino/VinheriaSensores/wokwi.toml: configuração do Wokwi no VS Code e exposição da serial na porta 4000.

Funcionamento

O firmware do Arduino realiza uma nova leitura dos sensores a cada 4000 ms e envia uma linha JSON pela serial em 9600 baud:

{"temperatura":14.0,"umidade":70.0,"luminosidade":853}

A ponte Python recebe essa mensagem pela serial simulada do Wokwi e encaminha o JSON para o Node-RED.

O fluxo completo é:

DHT22 + LDR
    ↓
Arduino Uno / Wokwi
    ↓
JSON pela serial
    ↓
RFC2217 — localhost:4000
    ↓
bridge_wokwi_nodered.py
    ↓
Node-RED
    ↓
MQTT
    ↓
HiveMQ Cloud

No Node-RED, cada leitura recebida é publicada automaticamente no tópico:

Fiap/iot/3ESOA/CodeSquad/sensor

Assim, a cada nova leitura do Arduino, uma nova mensagem MQTT é enviada ao HiveMQ.

Execução no Wokwi

Abra o projeto no VS Code.

Confirme que a extensão Wokwi Simulator está instalada e ativada.

Abra Arduino/VinheriaSensores/diagram.json.

Pressione F1.

Execute Wokwi: Start Simulator.

Mantenha a aba do Wokwi visível durante a integração.

Confirme que uma nova linha JSON é gerada a cada quatro segundos.

O arquivo wokwi.toml utiliza a seguinte configuração:

[wokwi]
version = 1
firmware = "build/VinheriaSensores.ino.hex"
elf = "build/VinheriaSensores.ino.elf"
rfc2217ServerPort = 4000

A porta 4000 permite que outros programas acessem a serial simulada do Arduino.

Integração com Node-RED e MQTT

O Arduino Uno não se conecta diretamente ao HiveMQ. A integração segue este fluxo:

Arduino Uno → serial simulada → ponte Python → Node-RED → broker MQTT (HiveMQ)

Primeiro, inicie o Node-RED:

node-red

Depois, abra no navegador:

http://127.0.0.1:1880

Importe o arquivo:

MQTT/node_red_vinheria_flow.json

No broker MQTT do fluxo, configure:

Server: <HOST_DO_HIVEMQ>
Port: 8883
TLS: ativado
Protocol: MQTT V3.1.1
Username: <USUARIO>
Password: <SENHA>

As credenciais do HiveMQ devem possuir permissão de Publish + Subscribe.

Depois da configuração, os nós MQTT do Node-RED devem aparecer com o status:

connected

Ponte entre Wokwi e Node-RED

Instale as dependências Python:

pip install pyserial requests

Na raiz do projeto, execute:

python MQTT/bridge_wokwi_nodered.py

Quando a conexão estiver funcionando, o terminal exibirá mensagens como:

Serial conectada.
Enviado ao Node-RED: {"temperatura": 14.0, "umidade": 70.0, "luminosidade": 853}

A ponte apenas transporta os dados da serial do Wokwi para o Node-RED. Ela não altera o firmware do Arduino e não publica diretamente no HiveMQ.

Validação no HiveMQ

No HiveMQ Cloud, abra o Web Client e assine o tópico:

Fiap/iot/3ESOA/CodeSquad/#

As mensagens publicadas pelo Node-RED devem aparecer automaticamente:

{"temperatura":14,"umidade":70,"luminosidade":853}

O tópico específico utilizado pelo projeto é:

Fiap/iot/3ESOA/CodeSquad/sensor

Compilação local

Com o arduino-cli instalado:

arduino-cli core install arduino:avr
arduino-cli lib install "DHT sensor library@1.4.7"
arduino-cli lib install "Adafruit Unified Sensor@1.1.15"
arduino-cli compile --fqbn arduino:avr:uno --output-dir Arduino/VinheriaSensores/build Arduino/VinheriaSensores

Os arquivos .hex e .elf gerados na pasta build são utilizados pelo Wokwi no VS Code.