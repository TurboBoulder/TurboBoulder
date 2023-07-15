FROM python:3.8-slim-buster

RUN apt-get update && \
    apt-get install -y openssl 

WORKDIR /app/

COPY ./* .

RUN chmod +x ./generate-certs.sh

CMD ["./generate-certs.sh"]


