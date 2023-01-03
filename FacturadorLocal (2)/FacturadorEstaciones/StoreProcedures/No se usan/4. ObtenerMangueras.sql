USE Estacion
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




/*
*************************************************************************    
 Author: Ivan Baños    
 Create date: 2020-08-29
 Description: obtener mangueras    
**************************************************************************
History:
2020-08-28 primera version
*/

CREATE procedure [dbo].[ObtenerMangueras]
( 
	@COD_CAR smallint
)
as
begin try
    set nocount on;
	select COD_MAN, MANGUERA.COD_TANQ, ARTICULO.DESCRIPCION, DS_ROM from MANGUERA 
	inner join dbo.TANQUES on MANGUERA.COD_TANQ = TANQUES.COD_TANQ
	inner join dbo.ARTICULO on TANQUES.Cod_art = ARTICULO.COD_ART
	where ESTADO = 'A' AND COD_CAR = @COD_CAR
    
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
exec ObtenerMangueras @COD_CAR = 1
*/





