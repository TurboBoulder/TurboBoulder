FROM mcr.microsoft.com/mssql/server:2022-latest

LABEL org.opencontainers.image.source=https://github.com/turboboulder/turboboulder

WORKDIR /app

USER root

# Update the package lists and install curl
RUN apt-get update \
    && apt-get install -y curl \
    && apt-get install -y gosu \
    && rm -rf /var/lib/apt/lists/*

# Copy the script into the image
COPY start.sh start.sh
# COPY prepare-certs.sh prepare-certs.sh

# Make the scripts executable
RUN chmod +x start.sh
# RUN chmod +x prepare-certs.sh

# Run prepare-certs.sh with root privileges
#RUN /prepare-certs.sh

ENV ACCEPT_EULA=Y
# ENV MSSQL_SA_PASSWORD=insertsqlpasswordhere
ENV MSSQL_PID=Express

EXPOSE 1433

# Run the start.sh script when a container is started
CMD ["/app/start.sh"]