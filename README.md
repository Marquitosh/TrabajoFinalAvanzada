Pasos para levantar el proyecto

1)Instalar Docker Desktop
  Asegurate de tener instalado Docker Desktop en tu PC.

2)Levantar el contenedor de SQL Server
  Una vez instalado, abrí la terminal (icono </> abajo a la derecha en Docker Desktop) y pegá el siguiente comando para crear el contenedor:
  docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Avanzada@1234" -p 1433:1433 --name sql-container -d mcr.microsoft.com/mssql/server:2022-latest


3)Configurar la base de datos
  Si el contenedor se creó correctamente, abrí SQL Server y ejecutá el script que está más abajo en este README.
  Ese script ya tiene cargados algunos usuarios, tipos de combustible, etc., para que puedan probar las rutas.

4)Ejecutar el proyecto AvanzadaDB
  Una vez hecho el pull desde visual studio, o directamente pueden descargar las carpetas y tirarlo donde tengan los proyectos en visual studio.
  Corran el proyecto llamado AvanzadaDB.
  Se va a abrir una página que permite probar directamente los CRUDs.
  Usen esa página para testear lo que necesiten.

5)Ejecutar ambos proyectos
  Si esta andando todo, para correr todo junto Click derecho en la solución -> Propiedades -> Proyectos de Inicio de Inicio Múltiple -> Selecciona "Iniciar" para ambos proyectos.
  Ahora ya cuando ejecuten el proyecto se va a abrir tambien el front.

6)Miren un poco la organizacion del proyecto 
  en las carpetas controllers es donde estan los metodos. 
  si quieren agregar una pagina creen una nueva carpeta en views.
  si necesitan ver las estructuras de datos esta todo en models.
  
7)Reportar errores
Si algo no funciona, avisen por el grupo y les doy una mano.



CREATE DATABASE AvanzadaDB;
GO

USE AvanzadaDB;
GO

-- Tabla para niveles de usuario
CREATE TABLE NivelesUsuario (
    IDNivel INT PRIMARY KEY IDENTITY(1,1),
    Descripcion NVARCHAR(20) NOT NULL
);

-- Tabla de usuarios
CREATE TABLE Usuarios (
    IDUsuario INT PRIMARY KEY IDENTITY(1,1),
    Email NVARCHAR(100) UNIQUE NOT NULL,
    Telefono NVARCHAR(20),
    Nombre NVARCHAR(50) NOT NULL,
    Apellido NVARCHAR(50) NOT NULL,
    Contraseña VARBINARY(256) NOT NULL, -- Almacenará el hash de la contraseña
    IDNivel INT FOREIGN KEY REFERENCES NivelesUsuario(IDNivel),
    Foto NVARCHAR(255), -- Ruta de la imagen
    FechaRegistro DATETIME DEFAULT GETDATE()
);

-- Tabla de tipos de combustible
CREATE TABLE TiposCombustible (
    IDCombustible INT PRIMARY KEY IDENTITY(1,1),
    Descripcion NVARCHAR(50) NOT NULL
);

-- Tabla de vehículos
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

-- Tabla de servicios
CREATE TABLE Servicios (
    IDServicio INT PRIMARY KEY IDENTITY(1,1),
    Nombre NVARCHAR(100) NOT NULL,
    Precio DECIMAL(10,2) NOT NULL,
    TiempoEstimado INT NOT NULL, -- En minutos
    Descripcion NVARCHAR(MAX)
);

-- Tabla de estados de turno
CREATE TABLE EstadosTurno (
    IDEstadoTurno INT PRIMARY KEY IDENTITY(1,1),
    Descripcion NVARCHAR(20) NOT NULL
);

-- Tabla de turnos
CREATE TABLE Turnos (
    IDTurno INT PRIMARY KEY IDENTITY(1,1),
    IDUsuario INT FOREIGN KEY REFERENCES Usuarios(IDUsuario),
    IDVehiculo INT FOREIGN KEY REFERENCES Vehiculos(IDVehiculo),
    IDServicio INT FOREIGN KEY REFERENCES Servicios(IDServicio),
    FechaHora DATETIME NOT NULL,
    IDEstadoTurno INT FOREIGN KEY REFERENCES EstadosTurno(IDEstadoTurno),
    Observaciones NVARCHAR(MAX)
);

-- Tabla de clientes (información adicional)
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
INSERT INTO NivelesUsuario (Descripcion) VALUES 
('Cliente'),
('Administrador');

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

-- Insertar un usuario administrador de ejemplo (contraseña: "admin123")
INSERT INTO Usuarios (Email, Telefono, Nombre, Apellido, Contraseña, IDNivel, Foto)
VALUES ('admin@taller.com', '1122334455', 'Admin', 'Sistema', 
HASHBYTES('SHA2_256', 'admin123'), 2, '/images/admin.jpg');

GO
