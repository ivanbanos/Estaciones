CREATE PROCEDURE [dbo].[HabilitarResolucion]
	@fechaVencimiento DateTime,
	@estacion UNIQUEIDENTIFIER
AS
BEGIN
	DECLARE @idEstadoHabilitado INT, @idEstadoActivo INT, @idEstacion INT
	

	SELECT @idEstacion = Id FROM Estaciones WHERE Estaciones.Guid = @estacion
	
	SELECT @idEstadoActivo = Id FROM Estados WHERE Texto = 'Activo'
	SELECT @idEstadoHabilitado = Id FROM Estados WHERE Texto = 'Habilitada'

	UPDATE Resolucion
		SET Resolucion.IdEstado = @idEstadoHabilitado,
		Resolucion.FechaFinal = @fechaVencimiento,
		Resolucion.Habilitada = 1
	FROM Resolucion
	INNER JOIN Estaciones 
		ON Estaciones.Id = Resolucion.IdEstacion
	WHERE Estaciones.[Guid] = @estacion
	AND (Resolucion.IdEstado = @idEstadoActivo OR Resolucion.IdEstado = @idEstadoHabilitado)
END