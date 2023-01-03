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
 Description: crear facturas electronica
**************************************************************************
History:
2020-08-28 primera version
*/

CREATE OR ALTER procedure [dbo].[CrearFacturaElectronica]
(
	@facturaPOSIds dbo.facturaPOSIdType readonly,
	@terceroId int
)
as
begin try
    set nocount on;
	declare @ResolucionId int, @consecutivoActual int, @fechafinal DATETIME, @facturaElectronicaId int

	select @ResolucionId = ResolucionId, @consecutivoActual = consecutivoActual, @fechafinal = fechafinal 
	from Resoluciones where esPos = 'N' and estado = 'AC'

	if @fechafinal is null or @fechafinal > GETDATE()
	begin
		update Resoluciones set estado = 'VE' WHERE esPos = 'N' and estado = 'AC'
		select @facturaElectronicaId as facturaElectronicaId
	end
	else
	begin
		insert into facturasElectronicas (fecha,resolucionId,consecutivo,estado,terceroId)
		values(GETDATE(), @ResolucionId, @consecutivoActual, 'CR',@terceroId)

		update Resoluciones set consecutivoActual = consecutivoActual+1 WHERE esPos = 'N' and estado = 'AC'

		select @facturaElectronicaId = @@IDENTITY

		update FacturasPOS set FacturasPOS.facturaElectronicaId = @facturaElectronicaId, FacturasPOS.estado = 'VE'
		from FacturasPOS
		inner join @facturaPOSIds as fpids on fpids.facturaPOSId = FacturasPOS.facturaPOSId

		select @facturaElectronicaId as facturaElectronicaId
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
exec ObtenerTercero @tipoIdentificacion = 1, @identificacion = 1

ALTER TABLE FacturasPOS
ADD terceroId int;
ALTER TABLE FacturasPOS
ADD FOREIGN KEY (terceroId) REFERENCES terceros(terceroId);
ALTER TABLE facturasElectronicas
ADD terceroId int;
ALTER TABLE facturasElectronicas
ADD FOREIGN KEY (terceroId) REFERENCES terceros(terceroId);


ALTER TABLE FacturasPOS
ADD facturaElectronicaId int;
ALTER TABLE FacturasPOS
ADD FOREIGN KEY (facturaElectronicaId) REFERENCES facturasElectronicas(facturaElectronicaId);

CREATE TYPE facturaPOSIdType AS TABLE   
    (  facturaPOSId INT );  
GO  
*/



