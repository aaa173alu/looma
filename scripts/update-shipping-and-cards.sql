-- Agregar campos de envío predeterminados al usuario
ALTER TABLE Usuario ADD NombreEnvio NVARCHAR(200) NULL;
ALTER TABLE Usuario ADD DireccionEnvio NVARCHAR(300) NULL;
ALTER TABLE Usuario ADD CiudadEnvio NVARCHAR(150) NULL;
ALTER TABLE Usuario ADD CPEnvio NVARCHAR(20) NULL;
ALTER TABLE Usuario ADD TelefonoEnvio NVARCHAR(30) NULL;
GO

-- Crear tabla de tarjetas guardadas
CREATE TABLE Tarjeta (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UsuarioId BIGINT NOT NULL,
    Marca NVARCHAR(50) NULL,
    NumeroEnmascarado NVARCHAR(50) NULL,
    Ultimos4 NVARCHAR(10) NULL,
    MesExp INT NULL,
    AnioExp INT NULL,
    NombreTitular NVARCHAR(200) NULL,
    EsPredeterminada BIT NOT NULL DEFAULT(0),
    FechaAlta DATETIME2 NOT NULL DEFAULT(GETUTCDATE()),
    CONSTRAINT FK_Tarjeta_Usuario FOREIGN KEY (UsuarioId) REFERENCES Usuario(Id) ON DELETE CASCADE
);
GO

-- Dejar solo una tarjeta predeterminada por usuario (regla a nivel de app)
-- Para garantizarlo en SQL Server, se puede crear un índice filtrado:
CREATE UNIQUE INDEX IX_Tarjeta_Predeterminada
ON Tarjeta(UsuarioId)
WHERE EsPredeterminada = 1;
GO
