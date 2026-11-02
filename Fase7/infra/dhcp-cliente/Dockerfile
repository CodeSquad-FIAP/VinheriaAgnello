FROM alpine:3.20
RUN apk add --no-cache iproute2 busybox
COPY udhcpc-hook.sh /scripts/udhcpc-hook.sh
COPY lease.sh /scripts/lease.sh
RUN chmod +x /scripts/*.sh
CMD ["sleep", "infinity"]
