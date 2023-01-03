USE [Facturacion_Electronica]
GO
DECLARE @idtipoIdentificaciones int

select @idtipoIdentificaciones = TipoIdentificacionId from TipoIdentificaciones
if @idtipoIdentificaciones is null
begin
	insert into TipoIdentificaciones (descripcion, codigoDian)values('Cédula Ciudadanía', 1)
	insert into TipoIdentificaciones (descripcion, codigoDian)values('Nit', 2)
	insert into TipoIdentificaciones (descripcion, codigoDian)values('No especificada', 0)
end
