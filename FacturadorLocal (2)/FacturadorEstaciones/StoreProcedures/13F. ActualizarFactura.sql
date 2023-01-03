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

CREATE  procedure [dbo].[ActualizarFactura]
( 
    @facturaPOSId int,
	@Placa varchar(50) = null,
	@Kilometraje varchar(50) = null
)
as
begin try
    set nocount on;
	Update FacturasPOS
	set Placa = @Placa,
	Kilometraje = @Kilometraje,
	impresa = impresa+1
	Where @facturaPOSId = facturaPOSId

	select 'Ok' as result
	
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
exec CrearFactura @ventaId = 0
select * FROM Resoluciones
delete Resoluciones where ResolucionId=3
select * from facturasPOS
insert into Resoluciones (descripcion,consecutivoInicio,consecutivoFinal,
    consecutivoActual, fechaInicio, fechafinal, estado, esPOS)values('CLIQ', 1, 2, 1, '20200822', 
    '20210821', 'AC', 'S')
*/



