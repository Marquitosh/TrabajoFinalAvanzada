Pasos para levantar el proyecto
1) Instalar Docker Desktop

Asegurate de tener instalado Docker Desktop en tu PC.

2) Levantar el contenedor de SQL Server

Una vez instalado, abrí la terminal (icono </> abajo a la derecha en Docker Desktop) y pegá el siguiente comando para crear el contenedor:

docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Avanzada@1234" -p 1433:1433 --name sql-container -d mcr.microsoft.com/mssql/server:2022-latest

3) Configurar la base de datos

primero tenemos que tener instalado SQL server management studio.
El la pestaña ponemos nombre de servidor: Localhost,1433.
Autentificacion de sql server.
Inicio de sesion: sa y contraseña: Avanzada@1234.
Si el contenedor se creó correctamente,te dejo iniciar bien a SQL ya podes ejecutar el script que está más abajo en este README.
Ese script ya tiene cargados algunos usuarios, tipos de combustible, etc., para que puedan probar las rutas.

4) Ejecutar el proyecto AvanzadaDB

Una vez hecho el pull desde Visual Studio (o descargando directamente las carpetas), abrí el proyecto en Visual Studio.

Corré el proyecto llamado AvanzadaDB.

Se va a abrir una página que permite probar directamente los CRUDs.

Usen esa página para testear lo que necesiten.

5) Ejecutar ambos proyectos (backend + frontend)

Si todo anda bien, para correr todo junto:

Click derecho en la solución

Ir a Propiedades

Seleccionar Proyectos de inicio múltiple

Marcar ambos proyectos con la opción Iniciar

De esta forma, cuando ejecuten la solución se va a levantar también el front.

6) Organización del proyecto

En la carpeta Controllers están los métodos de la API.

Si necesitan ver las estructuras de datos, está todo en Models.

Si quieren agregar una nueva página, créenla en la carpeta Views.

👉 Además, ya les dejé creada una carpeta dentro de AvanzadaWeb/Views llamada Usuarios.
Ahí están los archivos listos (vacíos por ahora) par que ustedes le agreguen su parte de codigo, pero ya está todo apuntando hacia ellos.

👉 El Dashboard muestra el panel de usuario. Para el diseño, fíjense que estoy usando el archivo _UserLayout.cshtml, que está en Views/Shared.
Solo tienen que llamarlo al inicio de su archivo, tal como se hace en el Dashboard, para que se renderice la parte de diseño que aparece arriba.

7) Reportar errores

Si algo no funciona, avisen por el grupo y les doy una mano.

ESTE CODIGO LO TIRAN EN SQL server management studio

CREATE DATABASE AvanzadaDB;
GO

USE AvanzadaDB;
GO

-- Tabla para niveles de usuario (con nueva columna URL para seguridad)

CREATE TABLE NivelesUsuario (
    IDNivel INT PRIMARY KEY IDENTITY(1,1),
    Descripcion NVARCHAR(20) NOT NULL,
    URL NVARCHAR(100) NOT NULL -- Nueva columna para gestión por URL
);

-- Tabla de estados para turnos

CREATE TABLE EstadosTurno (
    IDEstadoTurno INT PRIMARY KEY IDENTITY(1,1),
    Descripcion NVARCHAR(20) NOT NULL
);

-- Tabla de usuarios

CREATE TABLE Usuarios (
    IDUsuario INT PRIMARY KEY IDENTITY(1,1),
    Email NVARCHAR(100) UNIQUE NOT NULL,
    Telefono NVARCHAR(20),
    Nombre NVARCHAR(50) NOT NULL,
    Apellido NVARCHAR(50) NOT NULL,
    Contraseña VARBINARY(256) NOT NULL,
    IDNivel INT FOREIGN KEY REFERENCES NivelesUsuario(IDNivel),
    Foto VARBINARY(MAX), -- Cambiado de NVARCHAR a VARBINARY(MAX) :cite[1]:cite[5]
    FechaRegistro DATETIME DEFAULT GETDATE()
);


CREATE TABLE TiposCombustible (
    IDCombustible INT PRIMARY KEY IDENTITY(1,1),
    Descripcion NVARCHAR(50) NOT NULL
);

CREATE TABLE Vehiculos (
    IDVehiculo INT PRIMARY KEY IDENTITY(1,1),
    Marca NVARCHAR(50) NOT NULL,
    Modelo NVARCHAR(50) NOT NULL,
    Year INT NOT NULL,
    Patente NVARCHAR(15) UNIQUE NOT NULL,
    IDCombustible INT FOREIGN KEY REFERENCES TiposCombustible(IDCombustible),
    Observaciones NVARCHAR(MAX),
    IDUsuario INT FOREIGN KEY REFERENCES Usuarios(IDUsuario)
);

CREATE TABLE Servicios (
    IDServicio INT PRIMARY KEY IDENTITY(1,1),
    Nombre NVARCHAR(100) NOT NULL,
    Precio DECIMAL(10,2) NOT NULL,
    TiempoEstimado INT NOT NULL,
    Descripcion NVARCHAR(MAX)
);

CREATE TABLE Turnos (
    IDTurno INT PRIMARY KEY IDENTITY(1,1),
    IDUsuario INT FOREIGN KEY REFERENCES Usuarios(IDUsuario),
    IDVehiculo INT FOREIGN KEY REFERENCES Vehiculos(IDVehiculo),
    IDServicio INT FOREIGN KEY REFERENCES Servicios(IDServicio),
    FechaHora DATETIME NOT NULL,
    IDEstadoTurno INT FOREIGN KEY REFERENCES EstadosTurno(IDEstadoTurno),
    Observaciones NVARCHAR(MAX)
);

CREATE TABLE Clientes (
    IDCliente INT PRIMARY KEY IDENTITY(1,1),
    IDUsuario INT FOREIGN KEY REFERENCES Usuarios(IDUsuario),
    Telefono NVARCHAR(20),
    Direccion NVARCHAR(200),
    Localidad NVARCHAR(100),
    Provincia NVARCHAR(100),
    Observaciones NVARCHAR(MAX)
);

-- Insertar datos básicos
INSERT INTO NivelesUsuario (Descripcion, URL) VALUES 
('Cliente', '/cliente'),
('Administrador', '/admin');

INSERT INTO EstadosTurno (Descripcion) VALUES 
('Pendiente'),
('Confirmado'),
('En Proceso'),
('Completado'),
('Cancelado');

INSERT INTO TiposCombustible (Descripcion) VALUES 
('Nafta'),
('Diesel'),
('Eléctrico'),
('Híbrido'),
('GNC');

INSERT INTO Servicios (Nombre, Precio, TiempoEstimado, Descripcion) VALUES 
('Cambio de aceite', 2500.00, 30, 'Cambio de aceite y filtro'),
('Alineación y balanceo', 4500.00, 60, 'Alineación y balanceo de ruedas'),
('Service completo', 12000.00, 120, 'Service completo de vehículo'),
('Cambio de pastillas de freno', 8000.00, 45, 'Cambio de pastillas y discos de freno');

GO
