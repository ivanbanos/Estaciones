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
 Description: obtener caras    
**************************************************************************
History:
2020-08-28 primera version
*/

CREATE procedure [dbo].[ObtenerTercero]
( 
	@tipoIdentificacion int,
    @identificacion CHAR (15) 
)
as
begin try
    set nocount on;
	select terceroId, TipoIdentificaciones.descripcion, tipoIdentificacion, identificacion, nombre, telefono, correo, direccion, terceros.estado, COD_CLI 
	from dbo.terceros 
    inner join dbo.TipoIdentificaciones on terceros.tipoIdentificacion = TipoIdentificaciones.TipoIdentificacionId
    where @tipoIdentificacion = tipoIdentificacion AND @identificacion = identificacion
    
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
exec ObtenerTercero @tipoIdentificacion = 1, @identificacion = 1
*/



