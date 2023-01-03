USE [Facturacion_Electronica]
GO
/****** Object:  StoredProcedure [dbo].[CambiarConsecutivoActual]    Script Date: 10/19/2020 6:36:42 AM ******/
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

ALTER procedure [dbo].[CambiarConsecutivoActual]
(
	@consecutivoActual int
)
as
begin try
    
			
		Update Resoluciones
				set consecutivoActual = @consecutivoActual
				where estado = 'AC'
    
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
declare @ventas [ventasIds] 
insert into @ventas(ventaId) values(948677),
(948678),
(1222222222),
(948680)

exec [ObtenerFacturasPorVentas] @ventas

  ALTER TABLE FacturasPOS
ADD consolidadoId int;
*/