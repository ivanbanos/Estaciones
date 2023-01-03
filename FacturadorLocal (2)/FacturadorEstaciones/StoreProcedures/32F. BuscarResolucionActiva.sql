USE Facturacion_Electronica
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




/*
*************************************************************************    
 Author: Ivan Ba�os    
 Create date: 2020-08-29
 Description: obtener caras    
**************************************************************************
History:
2020-08-28 primera version
*/

CREATE procedure [dbo].[BuscarResolucionActiva]
as
begin try
    set nocount on;
	select * from Resoluciones where esPos = 'S' and estado = 'AC'
    
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
exec ObtenerCaras @COD_SUR = 1
*/



