"""
Ponte entre a serial simulada do Wokwi (RFC2217) e o Node-RED.

Fluxo:
Arduino Uno (Wokwi) -> Serial 9600 -> RFC2217 localhost:4000
-> este script -> HTTP POST -> Node-RED -> MQTT -> HiveMQ

Dependências:
    pip install pyserial requests
"""

import json
import time

import requests
import serial

SERIAL_URL = "rfc2217://localhost:4000"
BAUD_RATE = 9600
NODE_RED_URL = "http://127.0.0.1:1880/vinheria/sensores"


def conectar_serial():
    while True:
        try:
            print(f"Conectando à serial do Wokwi em {SERIAL_URL}...")
            porta = serial.serial_for_url(
                SERIAL_URL,
                baudrate=BAUD_RATE,
                timeout=2,
            )
            print("Serial conectada.")
            return porta
        except Exception as erro:
            print(f"Falha ao conectar à serial: {erro}")
            print("Vou tentar novamente em 2 segundos...")
            time.sleep(2)


def enviar_para_node_red(dados):
    resposta = requests.post(
        NODE_RED_URL,
        json=dados,
        timeout=5,
    )
    resposta.raise_for_status()


def main():
    porta = conectar_serial()

    while True:
        try:
            linha = porta.readline().decode("utf-8", errors="ignore").strip()

            if not linha:
                continue

            try:
                dados = json.loads(linha)
            except json.JSONDecodeError:
                print(f"Ignorado: não é JSON válido -> {linha}")
                continue

            enviar_para_node_red(dados)
            print(f"Enviado ao Node-RED: {json.dumps(dados, ensure_ascii=False)}")

        except (serial.SerialException, ConnectionError) as erro:
            print(f"Conexão serial perdida: {erro}")
            try:
                porta.close()
            except Exception:
                pass
            porta = conectar_serial()

        except requests.RequestException as erro:
            print(f"Node-RED indisponível: {erro}")
            time.sleep(1)

        except KeyboardInterrupt:
            print("\nEncerrado pelo usuário.")
            break


if __name__ == "__main__":
    main()
