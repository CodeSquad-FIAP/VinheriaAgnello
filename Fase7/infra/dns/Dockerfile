FROM alpine:3.20
RUN apk add --no-cache dnsmasq tzdata
# -k mantém em foreground; --log-facility=- manda o log para o stdout (sem syslog no container)
ENTRYPOINT ["dnsmasq", "-k", "--log-queries", "--log-dhcp", "--log-facility=-"]
