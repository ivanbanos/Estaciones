USE Facturacion_Electronica
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




/*
*************************************************************************    
 Author: Ivan Baños    
 Create date: 2020-08-29
 Description: crear tercero
**************************************************************************
History:
2020-08-28 primera version
*/

CREATE procedure [dbo].[CrearTercero]
( 
	@terceroId INT = null,
    @tipoIdentificacion int,
    @identificacion VARCHAR (50) ,
    @nombre VARCHAR (50) ,
    @telefono VARCHAR (50) ,
    @correo VARCHAR (50) ,
    @direccion VARCHAR (50) ,
    @estado CHAR(2),
	@COD_CLI char(15)
)
as
begin try
    set nocount on;
	declare @idTerceroCreado int;
	If @terceroId is null
	begin
		INSERT INTO terceros (tipoIdentificacion,identificacion,nombre,telefono,correo,direccion,estado,COD_CLI) 
		values(@tipoIdentificacion,@identificacion,@nombre,@telefono,@correo,@direccion,@estado,@COD_CLI)

		select @idTerceroCreado = @@Identity
	end
    else
	begin
		update terceros
		set
		tipoIdentificacion = @tipoIdentificacion,
		identificacion = @identificacion,
		nombre = @nombre,
		telefono = @telefono,
		correo = @correo,
		direccion = @direccion,
		estado = @estado,
		COD_CLI = @COD_CLI
		where @terceroId = terceroId

		select @idTerceroCreado = @terceroId
	end
	select terceroId, TipoIdentificaciones.descripcion, tipoIdentificacion, identificacion, nombre, telefono, correo, direccion, terceros.estado, COD_CLI 
	from dbo.terceros 
    inner join dbo.TipoIdentificaciones on terceros.tipoIdentificacion = TipoIdentificaciones.TipoIdentificacionId
    where @tipoIdentificacion = tipoIdentificacion AND @identificacion = identificacion ANd @idTerceroCreado = terceroId
end try
begin catch
    declare 
        @errorMessage varchar(2000),
        @errorProcedure varchar(255),
        @errorLine int;

    select  
        @errorMessage = error_message(),
        @errorProcedure = error_procedure(),
        @errorLine = error_line();

    raiserror (	N'<message>Error occurred in %s :: %s :: Line number: %d</message>', 16, 1, @errorProcedure, @errorMessage, @errorLine);
end catch;

/*
---------------------
--TEST CASE SECTION--
---------------------
-- Debug Test Sample Script
exec ObtenerClientePorIdentificacion @NIT = 0000001
ALTER TABLE TipoIdentificaciones
ADD codigoDian smallint;
*/



