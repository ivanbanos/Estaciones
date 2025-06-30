-- =============================================
-- Author:      Auto-generated based on SPSiesa and EstacionSIGES
-- Description: Procedimientos y tablas para integración Siesa SIGES
-- =============================================

-- Agregar columna para control de envío a Siesa en tablas SIGES
IF COL_LENGTH('Terceros', 'enviadoSiesa') IS NULL
BEGIN
    ALTER TABLE Terceros ADD enviadoSiesa bit NULL;
END
GO

IF COL_LENGTH('OrdenesDeDespachoSiges', 'EnviadaSiesa') IS NULL
BEGIN
    ALTER TABLE OrdenesDeDespachoSiges ADD EnviadaSiesa bit DEFAULT 1;
END
GO

-- Procedimiento para marcar terceros enviados a Siesa
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'CambiarEstadoTerceroEnviadoSiesaSiges')
    DROP PROCEDURE [dbo].[CambiarEstadoTerceroEnviadoSiesaSiges]
GO
CREATE PROCEDURE [dbo].[CambiarEstadoTerceroEnviadoSiesaSiges]
(
    @terceros [ventasIds] READONLY
)
AS
BEGIN TRY
    SET NOCOUNT ON;
    UPDATE Terceros SET enviadoSiesa = 1
    FROM Terceros
    INNER JOIN @terceros t ON t.ventaId = Terceros.terceroId
END TRY
BEGIN CATCH
    DECLARE 
        @errorMessage VARCHAR(2000),
        @errorProcedure VARCHAR(255),
        @errorLine INT;
    SELECT  
        @errorMessage = ERROR_MESSAGE(),
        @errorProcedure = ERROR_PROCEDURE(),
        @errorLine = ERROR_LINE();
    RAISERROR (N'<message>Error occurred in %s :: %s :: Line number: %d</message>', 16, 1, @errorProcedure, @errorMessage, @errorLine);
END CATCH;
GO

-- Procedimiento para buscar terceros no enviados a Siesa
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'BuscarTercerosNoEnviadosSiesaSiges')
    DROP PROCEDURE [dbo].[BuscarTercerosNoEnviadosSiesaSiges]
GO
CREATE PROCEDURE [dbo].[BuscarTercerosNoEnviadosSiesaSiges]
AS
BEGIN TRY
    SET NOCOUNT ON;
    DECLARE @tercerosTemp AS TABLE(id INT)
    INSERT INTO @tercerosTemp (id)
    SELECT TOP(25) terceroId FROM Terceros WHERE enviadoSiesa = 0 OR enviadoSiesa IS NULL
    SELECT  terceroId, tipoIdentificacion, ISNULL(NULLIF(identificacion, ''), 'No informado') AS identificacion, ISNULL(NULLIF(nombre, ''), 'No informado') AS nombre, ISNULL(NULLIF(telefono, ''), 'No informado') AS telefono, ISNULL(NULLIF(correo, ''), 'No informado') AS correo, ISNULL(NULLIF(direccion, ''), 'No informado') AS direccion, estado, COD_CLI, enviadoSiesa
    FROM Terceros
    INNER JOIN @tercerosTemp tmp ON tmp.id = Terceros.terceroId
END TRY
BEGIN CATCH
    DECLARE 
        @errorMessage VARCHAR(2000),
        @errorProcedure VARCHAR(255),
        @errorLine INT;
    SELECT  
        @errorMessage = ERROR_MESSAGE(),
        @errorProcedure = ERROR_PROCEDURE(),
        @errorLine = ERROR_LINE();
    RAISERROR (N'<message>Error occurred in %s :: %s :: Line number: %d</message>', 16, 1, @errorProcedure, @errorMessage, @errorLine);
END CATCH;
GO

-- Procedimiento para marcar facturas enviadas a Siesa
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'ActuralizarEnviadasSiesaSiges')
    DROP PROCEDURE [dbo].[ActuralizarEnviadasSiesaSiges]
GO
CREATE PROCEDURE [dbo].[ActuralizarEnviadasSiesaSiges]
(
    @facturas [ventasIds] READONLY
)
AS
BEGIN TRY
    SET NOCOUNT ON;
    UPDATE OrdenesDeDespachoSiges SET EnviadaSiesa = 1
    FROM OrdenesDeDespachoSiges
    INNER JOIN @facturas f ON f.ventaId = OrdenesDeDespachoSiges.ventaId
END TRY
BEGIN CATCH
    DECLARE 
        @errorMessage VARCHAR(2000),
        @errorProcedure VARCHAR(255),
        @errorLine INT;
    SELECT  
        @errorMessage = ERROR_MESSAGE(),
        @errorProcedure = ERROR_PROCEDURE(),
        @errorLine = ERROR_LINE();
    RAISERROR (N'<message>Error occurred in %s :: %s :: Line number: %d</message>', 16, 1, @errorProcedure, @errorMessage, @errorLine);
END CATCH;
GO

-- Procedimiento para obtener facturas no enviadas a Siesa
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'getFacturaSinEnviarSiesaSiges')
    DROP PROCEDURE [dbo].[getFacturaSinEnviarSiesaSiges]
GO
CREATE PROCEDURE [dbo].[getFacturaSinEnviarSiesaSiges]
AS
BEGIN TRY
    SET NOCOUNT ON;
    DECLARE @facturasTemp AS TABLE(id INT)
    INSERT INTO @facturasTemp (id)
    SELECT TOP(20) ventaId FROM OrdenesDeDespachoSiges WHERE (EnviadaSiesa = 0 OR EnviadaSiesa IS NULL) AND fecha < DATEADD(minute, -10, GETDATE()) ORDER BY ventaId DESC
    SELECT * FROM OrdenesDeDespachoSiges
    INNER JOIN @facturasTemp tmp ON tmp.id = OrdenesDeDespachoSiges.ventaId
END TRY
BEGIN CATCH
    DECLARE 
        @errorMessage VARCHAR(2000),
        @errorProcedure VARCHAR(255),
        @errorLine INT;
    SELECT  
        @errorMessage = ERROR_MESSAGE(),
        @errorProcedure = ERROR_PROCEDURE(),
        @errorLine = ERROR_LINE();
    RAISERROR (N'<message>Error occurred in %s :: %s :: Line number: %d</message>', 16, 1, @errorProcedure, @errorMessage, @errorLine);
END CATCH;
GO
