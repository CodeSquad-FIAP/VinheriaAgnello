#include <DHT.h>

/**
 * Vinheria Agnello - Monitoramento da adega climatizada
 *
 * Sensores definidos pelo projeto de hardware:
 *   - DHT22 no pino digital D2: temperatura e umidade;
 *   - Modulo LDR no pino analogico A0: luminosidade bruta.
 *
 * A cada quatro segundos, o firmware publica uma unica linha JSON pela porta
 * serial. O Node-RED pode separar as mensagens por quebra de linha, converter
 * o JSON e encaminhar os dados ao dashboard ou ao broker MQTT.
 */

// Pinagem definida no circuito do Wokwi.
const uint8_t DHT_PIN = 2;
const uint8_t LDR_PIN = A0;
const uint8_t DHT_TYPE = DHT22;

// Configuracoes compartilhadas com o monitor serial e com o Node-RED.
const unsigned long SERIAL_BAUD_RATE = 9600UL;
const unsigned long INTERVALO_LEITURA_MS = 4000UL;

// Objeto responsavel pela comunicacao com o sensor digital DHT22.
DHT dht(DHT_PIN, DHT_TYPE);

// millis() retorna unsigned long. Usar o mesmo tipo permite que a verificacao
// de intervalo continue correta mesmo quando o contador interno transbordar.
unsigned long instanteUltimaLeitura = 0UL;

/**
 * Inicializa a comunicacao serial e os sensores.
 *
 * Nenhuma mensagem de inicializacao e enviada pela serial, pois cada linha da
 * saida deve ser um JSON valido para ser consumido automaticamente.
 */
void setup() {
  Serial.begin(SERIAL_BAUD_RATE);
  dht.begin();
  pinMode(LDR_PIN, INPUT);
}

/**
 * Imprime um numero do DHT22 ou null quando a leitura for invalida.
 *
 * JSON nao reconhece NaN como numero. Publicar null mantem a mensagem valida e
 * permite que o fluxo no Node-RED trate uma falha sem descartar a luminosidade.
 */
void imprimirMedicaoDht(const float valor, const bool leituraValida) {
  if (leituraValida) {
    // Uma casa decimal e suficiente para a precisao nominal do DHT22.
    Serial.print(valor, 1);
    return;
  }

  Serial.print(F("null"));
}

/**
 * Envia as medicoes em uma unica linha JSON terminada por \n.
 *
 * Exemplo de sucesso:
 * {"temperatura":14.2,"umidade":70.0,"luminosidade":512}
 *
 * Em caso de falha do DHT22, temperatura e umidade recebem null, mas a leitura
 * independente do LDR continua disponivel. O campo erro facilita diagnostico.
 */
void enviarLeiturasComoJson(
    const float temperatura,
    const float umidade,
    const int luminosidade,
    const bool leituraDhtValida) {
  Serial.print(F("{\"temperatura\":"));
  imprimirMedicaoDht(temperatura, leituraDhtValida);
  Serial.print(F(",\"umidade\":"));
  imprimirMedicaoDht(umidade, leituraDhtValida);
  Serial.print(F(",\"luminosidade\":"));
  Serial.print(luminosidade);

  if (!leituraDhtValida) {
    Serial.print(F(",\"erro\":\"Falha de leitura do DHT22\""));
  }

  Serial.println(F("}"));
}

/**
 * Executa a amostragem sem bloquear o microcontrolador com delay().
 *
 * O DHT22 e o LDR sao lidos somente depois que os 4000 ms solicitados passam.
 * Durante o restante do tempo, loop() retorna imediatamente e deixa a placa
 * livre para futuras funcionalidades.
 */
void loop() {
  const unsigned long instanteAtual = millis();

  if (instanteAtual - instanteUltimaLeitura < INTERVALO_LEITURA_MS) {
    return;
  }

  instanteUltimaLeitura = instanteAtual;

  const float temperatura = dht.readTemperature();
  const float umidade = dht.readHumidity();
  const int luminosidade = analogRead(LDR_PIN);
  const bool leituraDhtValida = !isnan(temperatura) && !isnan(umidade);

  enviarLeiturasComoJson(
      temperatura,
      umidade,
      luminosidade,
      leituraDhtValida);
}
