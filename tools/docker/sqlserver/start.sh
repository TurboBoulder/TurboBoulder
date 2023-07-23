#!/bin/bash

# Function to perform curl action with retries
perform_curl_with_retries() {
  local url="$1"
  local output_file="$2"
  local max_attempts=50
  local pause_duration=10

  for ((attempt = 1; attempt <= max_attempts; attempt++)); do
    curl -o "$output_file" "$url"
    if [ $? -eq 0 ]; then
      echo "Curl action successful!"
      break
    fi

    if [ $attempt -eq $max_attempts ]; then
      echo "Max number of attempts reached. Curl action failed."
      exit 1
    fi

    echo "Curl action failed. Retrying in $pause_duration seconds..."
    sleep $pause_duration
  done
}

# Download the CA certificate from the web server with retries
perform_curl_with_retries "http://ca:8000/rootCA.crt" "/var/opt/mssql/ca.crt"

# Download the certificate and the private key from the web server with retries
perform_curl_with_retries "http://ca:8000/sqlserver.crt" "/var/opt/mssql/sqlserver.crt"
perform_curl_with_retries "http://ca:8000/sqlserver.key" "/var/opt/mssql/sqlserver.key"

# Add the CA certificate to the trusted certificate store
cp /var/opt/mssql/ca.crt /usr/local/share/ca-certificates/
update-ca-certificates


echo "Running start actions"

# Configure SQL Server to use the certificate for encryption
/opt/mssql/bin/mssql-conf set network.tlscert /var/opt/mssql/sqlserver.crt
/opt/mssql/bin/mssql-conf set network.tlskey /var/opt/mssql/sqlserver.key
/opt/mssql/bin/mssql-conf set network.forceencryption 1

# Start SQL Server
gosu mssql /opt/mssql/bin/sqlservr &

# Function to check if SQL Server is ready
function check_sql {
    /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -Q "SELECT 1" >/dev/null
    echo $?
}

# Wait for the SQL Server to come up
echo "Checking SQL Server readiness..."
until [[ $(check_sql) == 0 ]]; do
    echo "SQL Server is not ready, sleeping..."
    sleep 5
done
echo "SQL Server is ready."

# Create the .sql script using the environment variables
echo "CREATE LOGIN $SQL_USER WITH PASSWORD='$SQL_PASSWORD';
USE YourDatabase;
CREATE USER $SQL_USER FOR LOGIN $SQL_USER;
EXEC sp_addrolemember 'db_datareader', '$SQL_USER';
EXEC sp_addrolemember 'db_datawriter', '$SQL_USER';
GRANT EXECUTE TO $SQL_USER;" > init.sql
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -d master -i init.sql

/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -d master -i createdb.sql


# Keep container running
tail -f /dev/null
