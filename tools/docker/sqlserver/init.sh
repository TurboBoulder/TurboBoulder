#!/bin/bash
set -e

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
