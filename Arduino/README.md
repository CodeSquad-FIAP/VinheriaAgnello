# Monitoramento IoT da adega — Vinheria Agnello

Este diretório contém o firmware e o circuito simulado da adega climatizada. A implementação segue a arquitetura de hardware definida para o projeto:

| Sensor físico | Conexão | Medições no JSON |
| --- | --- | --- |
| DHT22 | Dados no pino digital `D2` | `temperatura` e `umidade` |
| Módulo LDR | Saída analógica no pino `A0` | `luminosidade` |

O DHT22 é um único sensor físico com duas medições. Por isso, os dois sensores do circuito produzem três campos no JSON.

## Arquivos

- `VinheriaSensores/VinheriaSensores.ino`: firmware comentado para Arduino Uno.
- `VinheriaSensores/diagram.json`: circuito que pode ser aberto no Wokwi.
- `VinheriaSensores/libraries.txt`: dependências usadas pelo simulador.

## Funcionamento

O firmware lê os sensores a cada **4000 ms**, sem utilizar `delay()`, e envia uma linha JSON pela serial em **9600 baud**:

```json
{"temperatura":14.2,"umidade":70.0,"luminosidade":512}
```

Se o DHT22 falhar, o JSON continua válido e preserva a leitura do LDR:

```json
{"temperatura":null,"umidade":null,"luminosidade":512,"erro":"Falha de leitura do DHT22"}
```

Os valores de temperatura são expressos em graus Celsius, a umidade em percentual e a luminosidade é a leitura bruta do conversor analógico do Arduino Uno (`0` a `1023`). Uma conversão para lux poderá ser aplicada no Node-RED caso o dashboard exija essa unidade.

## Execução no Wokwi

1. Crie ou abra um projeto Arduino Uno no Wokwi.
2. Copie os três arquivos da pasta `VinheriaSensores` para o projeto.
3. Inicie a simulação.
4. No monitor serial, confirme a chegada de uma linha JSON a cada quatro segundos.
5. Altere os controles de temperatura, umidade e luminosidade dos sensores para validar as leituras.

## Integração com Node-RED e MQTT

O Arduino Uno não possui conectividade de rede neste circuito. A integração deve seguir este fluxo:

```text
Arduino Uno → porta serial → Node-RED → broker MQTT (HiveMQ) → dashboard
```

Configure o nó serial do Node-RED para `9600 baud`, 8 bits de dados, sem paridade, 1 bit de parada e divisão das mensagens por `\n`. Depois, utilize um nó JSON para converter cada linha antes de alimentar os gráficos ou publicar no broker.

## Compilação local

Com o `arduino-cli` instalado:

```bash
arduino-cli core install arduino:avr
arduino-cli lib install "DHT sensor library@1.4.7"
arduino-cli compile --fqbn arduino:avr:uno Arduino/VinheriaSensores
```
