Use Facturacion_Electronica
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

CREATE procedure [dbo].[AnularFacturasYgenerar]
(
	@facturasIds [facturasIds] readonly
)
as
begin try
    set nocount on;
	declare @ResolucionId int, @consecutivoActual int, @fechafinal DATETIME, @facturaPOSId int, @ConsecutivoFinal int,
	@clientesCreditoGeneranFactura VARCHAR (50), @FacturasPOSConsolidadasId int, @terceroId int

	select @ResolucionId = ResolucionId, @consecutivoActual = consecutivoActual, @fechafinal = fechafinal, @ConsecutivoFinal = consecutivoFinal
		from Resoluciones where esPos = 'S' and estado = 'AC'

		if @fechafinal is null or @fechafinal < GETDATE() or @ConsecutivoFinal <= @consecutivoActual
		begin
			update Resoluciones set estado = 'VE' WHERE esPos = 'S' and estado = 'AC'
			select @facturaPOSId as facturaPOSId
		end
		else
		begin
			
			select @terceroId = terceros.terceroId
			from terceros
			inner join FacturasPOS on FacturasPOS.terceroId = terceros.terceroId
			Inner join @facturasIds fi on fi.facturaId = FacturasPOS.facturaPOSId

			insert into FacturasPOS (fecha,resolucionId,consecutivo,ventaId,estado,terceroid, Placa, Kilometraje)
				values(GETDATE(), @ResolucionId, @consecutivoActual, 0, 'CF',@terceroId, '', '')
			
				select @FacturasPOSConsolidadasId = SCOPE_IDENTITY()

				update Resoluciones set consecutivoActual = consecutivoActual+1 WHERE esPos = 'S' and estado = 'AC'


				
				Update FacturasPOS
				set estado = 'AN',
				consolidadoId = @FacturasPOSConsolidadasId
				from FacturasPOS
				Inner join @facturasIds fi on fi.facturaId = FacturasPOS.facturaPOSId

				select @FacturasPOSConsolidadasId as facturaPOSId
			
		end
    
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