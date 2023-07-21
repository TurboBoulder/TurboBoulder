FROM python:3.8-slim-buster

LABEL org.opencontainers.image.source=https://github.com/turboboulder/turboboulder

RUN apt-get update && \
    apt-get install -y openssl 

WORKDIR /app/

COPY ./* .

RUN chmod +x ./generate-certs.sh

CMD ["./generate-certs.sh"]


