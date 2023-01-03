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

CREATE procedure [dbo].[CrearFactura]
( 
    @ventaId int,
	@terceroId int,
	@Placa varchar(50) = null,
	@Kilometraje varchar(50) = null,
	@COD_FOR_PAG smallint
)
as
begin try
    set nocount on;
	declare @ResolucionId int, @consecutivoActual int, @fechafinal DATETIME, @facturaPOSId int, @ConsecutivoFinal int,
	@clientesCreditoGeneranFactura VARCHAR (50)


	select @facturaPOSId = facturaPOSId from FacturasPOS where ventaId = @ventaId

	select @clientesCreditoGeneranFactura=valor from configuracionEstacion where descripcion = 'ClientesCreditosGeneranFactura'

	if @facturaPOSId is not null
	begin
		select @facturaPOSId as facturaPOSId
	end
	else
	begin
		select @ResolucionId = ResolucionId, @consecutivoActual = consecutivoActual, @fechafinal = fechafinal, @ConsecutivoFinal = consecutivoFinal
		from Resoluciones where esPos = 'S' and estado = 'AC'

		if @fechafinal is null or @fechafinal < GETDATE() or @ConsecutivoFinal <= @consecutivoActual
		begin
			update Resoluciones set estado = 'VE' WHERE esPos = 'S' and estado = 'AC'
			select @facturaPOSId as facturaPOSId
		end
		else
		begin
			if @clientesCreditoGeneranFactura != 'SI' and @COD_FOR_PAG !=4
			begin
				insert into FacturasPOS (fecha,resolucionId,consecutivo,ventaId,estado,terceroid, Placa, Kilometraje)
				values(GETDATE(), @ResolucionId, 0, @ventaId, 'CR',@terceroId, @Placa, @Kilometraje)
			
				select @facturaPOSId = SCOPE_IDENTITY()

				select @facturaPOSId as facturaPOSId
				
			end
			else
			begin
			insert into FacturasPOS (fecha,resolucionId,consecutivo,ventaId,estado,terceroid, Placa, Kilometraje, enviada)
				values(GETDATE(), @ResolucionId, @consecutivoActual, @ventaId, 'CR',@terceroId, @Placa, @Kilometraje, 0)
			
				select @facturaPOSId = SCOPE_IDENTITY()

				update Resoluciones set consecutivoActual = consecutivoActual+1 WHERE esPos = 'S' and estado = 'AC'


				select @facturaPOSId as facturaPOSId
			end
		end
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
exec CrearFactura @ventaId = 0
select * FROM Resoluciones
delete Resoluciones where ResolucionId=3
select * from facturasPOS
insert into Resoluciones (descripcion,consecutivoInicio,consecutivoFinal,
    consecutivoActual, fechaInicio, fechafinal, estado, esPOS)values('CLIQ', 1, 2, 1, '20200822', 
    '20210821', 'AC', 'S')
*/



