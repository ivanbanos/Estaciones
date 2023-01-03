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

CREATE OR ALTER procedure [dbo].[ObtenerFacturaElectronicaPorTercerosYFechas]
(
	@terceroId int,
	@FechaInicio DATETIME,
	@FechaFinal DATETIME
)
as
begin try
    set nocount on;
	select * 
	from facturasElectronicas
	inner join Resoluciones on facturasElectronicas.resolucionId = Resoluciones.ResolucionId
	where (@terceroId is null or @terceroId = terceroId)
	AND (@FechaInicio is null or @FechaInicio <= facturasElectronicas.fecha)
	AND (@FechaFinal is null or @FechaFinal >= facturasElectronicas.fecha)
    
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
*/



