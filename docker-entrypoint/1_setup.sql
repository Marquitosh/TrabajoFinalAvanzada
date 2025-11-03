-- 1_setup.sql
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'AvanzadaDB')
BEGIN
    CREATE DATABASE AvanzadaDB;
END
GO

USE AvanzadaDB;
GO

-- Creación de Tablas
CREATE TABLE NivelesUsuario (
    IDNivel INT PRIMARY KEY IDENTITY(1,1), 
    Descripcion NVARCHAR(50) NOT NULL, 
    RolNombre VARCHAR(50) NOT NULL DEFAULT 'Cliente', 
    UrlDefault VARCHAR(100) NULL
);
CREATE TABLE EstadosTurno ( 
    IDEstadoTurno INT PRIMARY KEY IDENTITY(1,1), 
    Descripcion NVARCHAR(50) NOT NULL 
);
CREATE TABLE TiposCombustible ( 
    IDCombustible INT PRIMARY KEY IDENTITY(1,1), 
    Descripcion NVARCHAR(50) NOT NULL 
);
CREATE TABLE TipoServicio (
    IdTipoServicio INT PRIMARY KEY IDENTITY(1,1),
    Nombre VARCHAR(100) NOT NULL,
    Precio DECIMAL(10, 2) NOT NULL,
    TiempoEstimado INT NOT NULL, -- Minutos
    Descripcion TEXT NULL
);

CREATE TABLE EstadoServicio (
    IdEstadoServicio INT PRIMARY KEY IDENTITY(1,1),
    Nombre VARCHAR(50) NOT NULL
);

CREATE TABLE HorariosDisponibles (
    IdHorario INT PRIMARY KEY IDENTITY(1,1),
    LunesInicio TIME NULL,
    LunesFin TIME NULL,
    MartesInicio TIME NULL,
    MartesFin TIME NULL,
    MiercolesInicio TIME NULL,
    MiercolesFin TIME NULL,
    JuevesInicio TIME NULL,
    JuevesFin TIME NULL,
    ViernesInicio TIME NULL,
    ViernesFin TIME NULL,
    SabadoInicio TIME NULL,
    SabadoFin TIME NULL,
    DomingoInicio TIME NULL,
    DomingoFin TIME NULL
);
CREATE TABLE Marcas (
    IDMarca INT PRIMARY KEY IDENTITY(1,1),
    Nombre NVARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE Modelos (
    IDModelo INT PRIMARY KEY IDENTITY(1,1),
    Nombre NVARCHAR(50) NOT NULL,
    IDMarca INT NOT NULL,
    CONSTRAINT FK_Modelo_Marca FOREIGN KEY (IDMarca) REFERENCES Marcas(IDMarca)
);
CREATE TABLE Usuarios ( 
    IDUsuario INT PRIMARY KEY IDENTITY(1,1), 
    Email NVARCHAR(100) UNIQUE NOT NULL, 
    Telefono NVARCHAR(20), 
    Nombre NVARCHAR(50) NOT NULL, 
    Apellido NVARCHAR(50) NOT NULL, 
    Contraseña VARBINARY(256) NOT NULL, 
    IDNivel INT FOREIGN KEY REFERENCES NivelesUsuario(IDNivel), 
    Foto VARBINARY(MAX), 
    FechaRegistro DATETIME DEFAULT GETDATE() 
);
CREATE TABLE Vehiculos ( 
IDVehiculo INT PRIMARY KEY IDENTITY(1,1), 
    Year INT NOT NULL, 
    Patente NVARCHAR(15) UNIQUE NOT NULL, 
    IDCombustible INT FOREIGN KEY REFERENCES TiposCombustible(IDCombustible), 
    Observaciones NVARCHAR(MAX), 
    
    IDUsuario INT FOREIGN KEY REFERENCES Usuarios(IDUsuario), 
    
    IDMarca INT NOT NULL,
    IDModelo INT NOT NULL,

    CONSTRAINT FK_Vehiculo_Marca FOREIGN KEY (IDMarca) REFERENCES Marcas(IDMarca),
    CONSTRAINT FK_Vehiculo_Modelo FOREIGN KEY (IDModelo) REFERENCES Modelos(IDModelo)
);
CREATE TABLE Turno ( 
    IDTurno INT PRIMARY KEY IDENTITY(1,1), 
    IDUsuario INT FOREIGN KEY REFERENCES Usuarios(IDUsuario), 
    IDVehiculo INT FOREIGN KEY REFERENCES Vehiculos(IDVehiculo), 
    FechaHora DATETIME NOT NULL, 
    IDEstadoTurno INT FOREIGN KEY REFERENCES EstadosTurno(IDEstadoTurno), 
    Observaciones NVARCHAR(MAX) 
);

CREATE TABLE Servicio (
    IdServicio INT PRIMARY KEY IDENTITY(1,1),
    IdTurno INT NOT NULL,
    IdTipoServicio INT NOT NULL,
    IdEstadoServicio INT NOT NULL,
    Observaciones TEXT NULL,
    
    CONSTRAINT FK_Servicio_Turno FOREIGN KEY (IdTurno) REFERENCES Turno(IDTurno) ON DELETE CASCADE,
    CONSTRAINT FK_Servicio_TipoServicio FOREIGN KEY (IdTipoServicio) REFERENCES TipoServicio(IdTipoServicio),
    CONSTRAINT FK_Servicio_EstadoServicio FOREIGN KEY (IdEstadoServicio) REFERENCES EstadoServicio(IdEstadoServicio)
);

CREATE TABLE Clientes ( IDCliente INT PRIMARY KEY IDENTITY(1,1), IDUsuario INT FOREIGN KEY REFERENCES Usuarios(IDUsuario), Telefono NVARCHAR(20), Direccion NVARCHAR(200), Localidad NVARCHAR(100), Provincia NVARCHAR(100), Observaciones NVARCHAR(MAX) );
CREATE TABLE PasswordResetTokens ( IDToken INT IDENTITY(1,1) PRIMARY KEY, IDUsuario INT NOT NULL, Token NVARCHAR(100) NOT NULL, FechaExpiracion DATETIME2 NOT NULL, Utilizado BIT NOT NULL DEFAULT 0, FechaCreacion DATETIME2 NOT NULL DEFAULT GETUTCDATE(), CONSTRAINT FK_PasswordResetTokens_Usuarios FOREIGN KEY (IDUsuario) REFERENCES Usuarios(IDUsuario) ON DELETE CASCADE );
CREATE TABLE Logs (
    IDLog INT IDENTITY(1,1) PRIMARY KEY,
    Fecha DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    Nivel NVARCHAR(50) NOT NULL,
    Mensaje NVARCHAR(MAX) NOT NULL,
    Detalles NVARCHAR(MAX) NULL,
    Metodo NVARCHAR(255) NULL,
    Usuario NVARCHAR(100) NULL
);
GO

-- Creación de índices
CREATE INDEX IX_PasswordResetTokens_Token ON PasswordResetTokens(Token); 
CREATE INDEX IX_PasswordResetTokens_FechaExpiracion ON PasswordResetTokens(FechaExpiracion);
GO