-- 2_seed.sql
USE AvanzadaDB;
GO

--------------------------------------
-- INSERTAR NivelesUsuario
--------------------------------------
SET IDENTITY_INSERT NivelesUsuario ON;
GO

IF NOT EXISTS (SELECT 1 FROM NivelesUsuario WHERE IDNivel = 1)
BEGIN
    INSERT INTO NivelesUsuario (IDNivel, RolNombre, Descripcion) 
    VALUES (1, 'Cliente', 'Usuario estándar del sistema');
END

IF NOT EXISTS (SELECT 1 FROM NivelesUsuario WHERE IDNivel = 2)
BEGIN
    INSERT INTO NivelesUsuario (IDNivel, RolNombre, Descripcion) 
    VALUES (2, 'Admin', 'Administrador del sistema');
END
GO
SET IDENTITY_INSERT NivelesUsuario OFF;
GO

--------------------------------------
-- INSERTAR EstadosTurno
--------------------------------------
SET IDENTITY_INSERT EstadosTurno ON;
GO

INSERT INTO EstadosTurno (IDEstadoTurno, Descripcion) VALUES
(1, 'Pendiente'),
(2, 'Confirmado'),
(3, 'Cancelado');
GO

SET IDENTITY_INSERT EstadosTurno OFF;
GO

--------------------------------------
-- INSERTAR TiposCombustible
--------------------------------------
INSERT INTO TiposCombustible (Descripcion) VALUES
('Nafta Premium'),
('Nafta Súper'),
('Diesel');
GO

-- Insertar los estados de un servicio
SET IDENTITY_INSERT dbo.EstadoServicio ON;
INSERT INTO EstadoServicio (IdEstadoServicio, Nombre) 
VALUES
(1, 'Pendiente'),
(2, 'En Progreso'),
(3, 'Completado'),
(4, 'Cancelado');
SET IDENTITY_INSERT dbo.EstadoServicio OFF;
GO

-- Insertar los TIPOS de servicio (lo que antes estaba hardcodeado)
SET IDENTITY_INSERT dbo.TipoServicio ON;
INSERT INTO TipoServicio (IdTipoServicio, Nombre, Precio, TiempoEstimado, Descripcion) 
VALUES
(1, 'Cambio de aceite', 8500.00, 45, 'Cambio de aceite sintético y filtro de aceite.'),
(2, 'Alineación y balanceo', 7000.00, 60, 'Alineación 3D y balanceo computarizado de las 4 ruedas.'),
(3, 'Chequeo general', 5000.00, 75, 'Revisión completa de 25 puntos de seguridad, fluidos y filtros.'),
(4, 'Cambio de pastillas de freno', 12000.00, 90, 'Reemplazo de pastillas de freno delanteras (no incluye discos).');
SET IDENTITY_INSERT dbo.TipoServicio OFF;
GO

-- ========================
-- Horarios
-- ========================

-- Insertar un horario de atención estándar
SET IDENTITY_INSERT dbo.HorariosDisponibles ON;
INSERT INTO HorariosDisponibles (IdHorario, LunesInicio, LunesFin, MartesInicio, MartesFin, MiercolesInicio, MiercolesFin, JuevesInicio, JuevesFin, ViernesInicio, ViernesFin, SabadoInicio, SabadoFin, DomingoInicio, DomingoFin)
VALUES
(1, '09:00:00', '18:00:00', '09:00:00', '18:00:00', '09:00:00', '18:00:00', '09:00:00', '18:00:00', '09:00:00', '18:00:00', '09:00:00', '13:00:00', NULL, NULL);
SET IDENTITY_INSERT dbo.HorariosDisponibles OFF;
GO

-- ====================================================
-- SEED PARA Marcas y Modelos
-- ====================================================
BEGIN TRANSACTION;
DECLARE @MarcaID INT;

-- Toyota
IF NOT EXISTS (SELECT 1 FROM Marcas WHERE Nombre = 'Toyota')
BEGIN
    INSERT INTO Marcas (Nombre) VALUES ('Toyota');
    SET @MarcaID = SCOPE_IDENTITY();
    INSERT INTO Modelos (IDMarca, Nombre) VALUES (@MarcaID, 'Hilux'), (@MarcaID, 'Yaris'), (@MarcaID, 'Corolla'), (@MarcaID, 'Corolla Cross'), (@MarcaID, 'Etios'), (@MarcaID, 'SW4'), (@MarcaID, 'RAV4'), (@MarcaID, 'Innova');
END

-- Volkswagen
IF NOT EXISTS (SELECT 1 FROM Marcas WHERE Nombre = 'Volkswagen')
BEGIN
    INSERT INTO Marcas (Nombre) VALUES ('Volkswagen');
    SET @MarcaID = SCOPE_IDENTITY();
    INSERT INTO Modelos (IDMarca, Nombre) VALUES (@MarcaID, 'Gol'), (@MarcaID, 'Gol Trend'), (@MarcaID, 'Amarok'), (@MarcaID, 'Polo'), (@MarcaID, 'Taos'), (@MarcaID, 'T-Cross'), (@MarcaID, 'Nivus'), (@MarcaID, 'Suran'), (@MarcaID, 'Fox'), (@MarcaID, 'Vento');
END

-- Fiat
IF NOT EXISTS (SELECT 1 FROM Marcas WHERE Nombre = 'Fiat')
BEGIN
    INSERT INTO Marcas (Nombre) VALUES ('Fiat');
    SET @MarcaID = SCOPE_IDENTITY();
    INSERT INTO Modelos (IDMarca, Nombre) VALUES (@MarcaID, 'Cronos'), (@MarcaID, 'Palio'), (@MarcaID, 'Uno'), (@MarcaID, 'Strada'), (@MarcaID, 'Fiorino'), (@MarcaID, 'Punto'), (@MarcaID, 'Siena'), (@MarcaID, 'Mobi');
END

-- Peugeot
IF NOT EXISTS (SELECT 1 FROM Marcas WHERE Nombre = 'Peugeot')
BEGIN
    INSERT INTO Marcas (Nombre) VALUES ('Peugeot');
    SET @MarcaID = SCOPE_IDENTITY();
    INSERT INTO Modelos (IDMarca, Nombre) VALUES (@MarcaID, '208'), (@MarcaID, '2008'), (@MarcaID, 'Partner'), (@MarcaID, '308'), (@MarcaID, '307'), (@MarcaID, '408'), (@MarcaID, '504'), (@MarcaID, '405');
END

-- Ford
IF NOT EXISTS (SELECT 1 FROM Marcas WHERE Nombre = 'Ford')
BEGIN
    INSERT INTO Marcas (Nombre) VALUES ('Ford');
    SET @MarcaID = SCOPE_IDENTITY();
    INSERT INTO Modelos (IDMarca, Nombre) VALUES (@MarcaID, 'Ranger'), (@MarcaID, 'EcoSport'), (@MarcaID, 'Ka'), (@MarcaID, 'Falcon'), (@MarcaID, 'Fiesta'), (@MarcaID, 'Mondeo'), (@MarcaID, 'Focus'), (@MarcaID, 'Ka+');
END

-- Renault
IF NOT EXISTS (SELECT 1 FROM Marcas WHERE Nombre = 'Renault')
BEGIN
    INSERT INTO Marcas (Nombre) VALUES ('Renault');
    SET @MarcaID = SCOPE_IDENTITY();
    INSERT INTO Modelos (IDMarca, Nombre) VALUES (@MarcaID, 'Sandero'), (@MarcaID, 'Kangoo'), (@MarcaID, 'Logan'), (@MarcaID, 'Clio'), (@MarcaID, 'Scenic'), (@MarcaID, 'Fluence'), (@MarcaID, 'Stepway'), (@MarcaID, '12');
END

-- Chevrolet
IF NOT EXISTS (SELECT 1 FROM Marcas WHERE Nombre = 'Chevrolet')
BEGIN
    INSERT INTO Marcas (Nombre) VALUES ('Chevrolet');
    SET @MarcaID = SCOPE_IDENTITY();
    INSERT INTO Modelos (IDMarca, Nombre) VALUES (@MarcaID, 'Corsa'), (@MarcaID, 'Classic'), (@MarcaID, 'Onix'), (@MarcaID, 'Cruze'), (@MarcaID, 'S10'), (@MarcaID, 'Tahoe'), (@MarcaID, 'Silverado'), (@MarcaID, 'Spark'), (@MarcaID, 'Meriva'), (@MarcaID, 'Captiva');
END

-- Citroën
IF NOT EXISTS (SELECT 1 FROM Marcas WHERE Nombre = 'Citroën')
BEGIN
    INSERT INTO Marcas (Nombre) VALUES ('Citroën');
    SET @MarcaID = SCOPE_IDENTITY();
    INSERT INTO Modelos (IDMarca, Nombre) VALUES (@MarcaID, 'C3'), (@MarcaID, 'C4 Cactus'), (@MarcaID, 'Berlingo'), (@MarcaID, 'Aircross'), (@MarcaID, 'C4'), (@MarcaID, 'C5'), (@MarcaID, 'Spacetourer'), (@MarcaID, 'Picasso');
END

-- Jeep
IF NOT EXISTS (SELECT 1 FROM Marcas WHERE Nombre = 'Jeep')
BEGIN
    INSERT INTO Marcas (Nombre) VALUES ('Jeep');
    SET @MarcaID = SCOPE_IDENTITY();
    INSERT INTO Modelos (IDMarca, Nombre) VALUES (@MarcaID, 'Renegade'), (@MarcaID, 'Compass'), (@MarcaID, 'Grand Cherokee'), (@MarcaID, 'Patriot'), (@MarcaID, 'Cherokee'), (@MarcaID, 'Wrangler'), (@MarcaID, 'Liberty'), (@MarcaID, 'Wagoneer');
END

-- Nissan
IF NOT EXISTS (SELECT 1 FROM Marcas WHERE Nombre = 'Nissan')
BEGIN
    INSERT INTO Marcas (Nombre) VALUES ('Nissan');
    SET @MarcaID = SCOPE_IDENTITY();
    INSERT INTO Modelos (IDMarca, Nombre) VALUES (@MarcaID, 'Frontier'), (@MarcaID, 'Kicks'), (@MarcaID, 'Versa'), (@MarcaID, 'Sentra'), (@MarcaID, 'X-Trail'), (@MarcaID, 'Patrol'), (@MarcaID, 'March'), (@MarcaID, 'Tiida');
END

COMMIT TRANSACTION;
GO

--------------------------------------
-- INSERTAR Usuarios de Prueba
--------------------------------------
-- NOTA: IDUsuario = 1 (Admin), IDUsuario = 2 (User)
SET IDENTITY_INSERT dbo.Usuarios ON;
GO

-- 1. Admin (Password: 123456)
IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE Email = 'admin@test.com')
BEGIN
    INSERT INTO Usuarios (IDUsuario, Email, Telefono, Nombre, Apellido, Contraseña, IDNivel)
    VALUES (1, 'admin@test.com', '1122334455', 'Admin', 'Test', HASHBYTES('SHA2_256', '123456'), 2); -- IDNivel 2 = Admin
END

-- 2. User (Password: 123456)
IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE Email = 'user@test.com')
BEGIN
    INSERT INTO Usuarios (IDUsuario, Email, Telefono, Nombre, Apellido, Contraseña, IDNivel)
    VALUES (2, 'user@test.com', '6677889900', 'User', 'Test', HASHBYTES('SHA2_256', '123456'), 1); -- IDNivel 1 = Cliente
END
GO
SET IDENTITY_INSERT dbo.Usuarios OFF;
GO

--------------------------------------
-- INSERTAR Vehiculos de Prueba
--------------------------------------
SET IDENTITY_INSERT dbo.Vehiculos ON;
GO

-- 1. Vehiculo para Admin (IDUsuario = 1)
IF NOT EXISTS (SELECT 1 FROM Vehiculos WHERE Patente = 'ADM123')
BEGIN
    INSERT INTO Vehiculos (IDVehiculo, IDMarca, IDModelo, Year, Patente, IDCombustible, IDUsuario)
    VALUES (
        1, 
        (SELECT IDMarca FROM Marcas WHERE Nombre = 'Ford'), 
        (SELECT IDModelo FROM Modelos WHERE Nombre = 'Ranger' AND IDMarca = (SELECT IDMarca FROM Marcas WHERE Nombre = 'Ford')), 
        2022, 
        'ADM123', 
        (SELECT IDCombustible FROM TiposCombustible WHERE Descripcion = 'Diesel'), 
        1 -- IDUsuario 1 = Admin
    );
END

-- 2. Vehiculo para User (IDUsuario = 2)
IF NOT EXISTS (SELECT 1 FROM Vehiculos WHERE Patente = 'USR456')
BEGIN
    INSERT INTO Vehiculos (IDVehiculo, IDMarca, IDModelo, Year, Patente, IDCombustible, IDUsuario)
    VALUES (
        2, 
        (SELECT IDMarca FROM Marcas WHERE Nombre = 'Volkswagen'), 
        (SELECT IDModelo FROM Modelos WHERE Nombre = 'Gol' AND IDMarca = (SELECT IDMarca FROM Marcas WHERE Nombre = 'Volkswagen')), 
        2019, 
        'USR456', 
        (SELECT IDCombustible FROM TiposCombustible WHERE Descripcion = 'Nafta Premium'), 
        2 -- IDUsuario 2 = User
    );
END
GO
SET IDENTITY_INSERT dbo.Vehiculos OFF;
GO
--------------------------------------
-- INSERTAR Turnos y Servicios de Prueba
--------------------------------------
SET IDENTITY_INSERT dbo.Turno ON;
GO

-- TURNO 1: (ID=1) Confirmado, para mañana, Usuario 2 (User)
IF NOT EXISTS (SELECT 1 FROM Turno WHERE IDTurno = 1)
BEGIN
    INSERT INTO Turno (IDTurno, IDUsuario, IDVehiculo, FechaHora, IDEstadoTurno, Observaciones)
    VALUES (1, 2, 2, DATEADD(day, 1, CAST(GETDATE() AS DATE)) + '09:00:00', 2, 'Turno confirmado para mañana.'); -- IDUsuario 2, IDVehiculo 2, IDEstadoTurno 2 = Confirmado
    
    -- Servicios para Turno 1
    INSERT INTO Servicio (IdTurno, IdTipoServicio, IdEstadoServicio) VALUES (1, 1, 1); -- Cambio de aceite, Pendiente
    INSERT INTO Servicio (IdTurno, IdTipoServicio, IdEstadoServicio) VALUES (1, 3, 1); -- Chequeo general, Pendiente
END
GO

-- TURNO 2: (ID=2) Pendiente, para pasado mañana, Usuario 2 (User)
IF NOT EXISTS (SELECT 1 FROM Turno WHERE IDTurno = 2)
BEGIN
    INSERT INTO Turno (IDTurno, IDUsuario, IDVehiculo, FechaHora, IDEstadoTurno, Observaciones)
    VALUES (2, 2, 2, DATEADD(day, 2, CAST(GETDATE() AS DATE)) + '11:00:00', 1, 'Solicitud pendiente.'); -- IDUsuario 2, IDVehiculo 2, IDEstadoTurno 1 = Pendiente
    
    -- Servicios para Turno 2
    INSERT INTO Servicio (IdTurno, IdTipoServicio, IdEstadoServicio) VALUES (2, 4, 1); -- Cambio de frenos, Pendiente
END
GO

-- TURNO 3: (ID=3) Cancelado, era para ayer, Usuario 1 (Admin)
IF NOT EXISTS (SELECT 1 FROM Turno WHERE IDTurno = 3)
BEGIN
    INSERT INTO Turno (IDTurno, IDUsuario, IDVehiculo, FechaHora, IDEstadoTurno, Observaciones)
    VALUES (3, 1, 1, DATEADD(day, -1, CAST(GETDATE() AS DATE)) + '14:00:00', 3, 'Turno cancelado por el cliente.'); -- IDUsuario 1, IDVehiculo 1, IDEstadoTurno 3 = Cancelado
    
    -- Servicios para Turno 3
    INSERT INTO Servicio (IdTurno, IdTipoServicio, IdEstadoServicio) VALUES (3, 2, 4); -- Alineación, Cancelado
END
GO

-- TURNO 4: (ID=4) Confirmado, para la semana que viene, Usuario 1 (Admin)
IF NOT EXISTS (SELECT 1 FROM Turno WHERE IDTurno = 4)
BEGIN
    INSERT INTO Turno (IDTurno, IDUsuario, IDVehiculo, FechaHora, IDEstadoTurno, Observaciones)
    VALUES (4, 1, 1, DATEADD(day, 7, CAST(GETDATE() AS DATE)) + '15:30:00', 2, 'Confirmado por Admin.'); -- IDUsuario 1, IDVehiculo 1, IDEstadoTurno 2 = Confirmado
    
    -- Servicios para Turno 4
    INSERT INTO Servicio (IdTurno, IdTipoServicio, IdEstadoServicio) VALUES (4, 1, 1); -- Cambio de aceite, Pendiente
END
GO

SET IDENTITY_INSERT dbo.Turno OFF;
GO