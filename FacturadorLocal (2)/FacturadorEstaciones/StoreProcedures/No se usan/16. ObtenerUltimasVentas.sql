USE [Facturacion_Electronica]
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

CREATE OR ALTER procedure [dbo].[ObtenerUltimasVentasPorManguera]
( 
	@COD_MAN smallint,
	@Cant int = 1
)
as
begin try
    set nocount on;
	select top(@Cant)* from VENTAS 
	inner join CLIENTES on VENTAS.COD_CLI = CLIENTES.COD_CLI
	where COD_MAN = @COD_MAN order by CONSECUTIVO desc
    
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
exec ObtenerUltimasVentasPorManguera @COD_MAN = 1
*/





