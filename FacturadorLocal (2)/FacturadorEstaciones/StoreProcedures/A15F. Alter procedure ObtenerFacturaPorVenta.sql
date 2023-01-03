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

CREATE procedure [dbo].[ObtenerFacturaPorVenta]
(
	@ventaId int
)
as
begin try
    set nocount on;
	select top(1)
	Resoluciones.descripcion as descripcionRes, Resoluciones.autorizacion, Resoluciones.consecutivoActual,
	Resoluciones.consecutivoFinal, Resoluciones.consecutivoInicio, Resoluciones.esPOS, Resoluciones.estado,
	Resoluciones.fechafinal, Resoluciones.fechaInicio, Resoluciones.ResolucionId, Resoluciones.habilitada, FacturasPOS.*, terceros.*, TipoIdentificaciones.*
	
	from Facturacion_Electronica.dbo.FacturasPOS
	left join Facturacion_Electronica.dbo.Resoluciones on FacturasPOS.resolucionId = Resoluciones.ResolucionId
	left join Facturacion_Electronica.dbo.terceros on FacturasPOS.terceroId = terceros.terceroId
    left join Facturacion_Electronica.dbo.TipoIdentificaciones on terceros.tipoIdentificacion = TipoIdentificaciones.TipoIdentificacionId
	
	where @ventaId = FacturasPOS.ventaId
    
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
exec ObtenerFacturaPorCara @COD_CAR = 2, @identificacion = 1
select * from ventas
*/