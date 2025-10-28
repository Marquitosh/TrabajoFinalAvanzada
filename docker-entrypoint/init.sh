#!/bin/bash
# init.sh (o 3_init_exec.sh)

# 1. Iniciar el proceso de SQL Server EN SEGUNDO PLANO
echo "Iniciando SQL Server en segundo plano..."
/opt/mssql/bin/sqlservr &

# 2. Esperar a que SQL Server esté listo para aceptar comandos
# Damos un tiempo inicial de "calentamiento" de 20 segundos
sleep 20

echo "Esperando que SQL Server se inicie (ping)..."
/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P $MSSQL_SA_PASSWORD -C -Q "SELECT 1" >> /dev/null 2>&1
while [ $? -ne 0 ]; do
    echo "SQL Server no está listo. Reintentando en 5 segundos..."
    sleep 5
    /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P $MSSQL_SA_PASSWORD -C -Q "SELECT 1" >> /dev/null 2>&1
done

echo "SQL Server está activo. Ejecutando scripts de inicialización..."

# 3. Ejecutar el script 1_setup.sql (Creación de DB/Tablas)
echo "Ejecutando 1_setup.sql..."
/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P $MSSQL_SA_PASSWORD -C -i /docker-entrypoint-initdb.d/1_setup.sql

# 4. Ejecutar el script 2_seed_data.sql (Inserción de Datos)
echo "Ejecutando 2_seed_data.sql..."
/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P $MSSQL_SA_PASSWORD -C -i /docker-entrypoint-initdb.d/2_seed_data.sql

echo "Inicialización completada. El servidor seguirá ejecutándose."

# 5. Esperar indefinidamente para mantener el contenedor vivo
# Si este script termina, el contenedor se detiene.
wait