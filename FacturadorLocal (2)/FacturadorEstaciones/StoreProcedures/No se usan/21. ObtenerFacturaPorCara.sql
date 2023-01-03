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

CREATE procedure [dbo].[ObtenerFacturaPorCara]
(
	@COD_CAR int
)
as
begin try
    set nocount on;
	select top(1)Resoluciones.descripcion as descripcionRes, Resoluciones.autorizacion, Resoluciones.consecutivoActual,
	Resoluciones.consecutivoFinal, Resoluciones.consecutivoInicio, Resoluciones.esPOS, Resoluciones.estado,
	Resoluciones.fechafinal, Resoluciones.fechaInicio, Resoluciones.ResolucionId, FacturasPOS.*, v.*, terceros.*, TipoIdentificaciones.*, CLIENTES.*,
	MANGUERA.COD_MAN, MANGUERA.COD_TANQ, ARTICULO.DESCRIPCION, MANGUERA.DS_ROM
	from Facturacion_Electronica.dbo.FacturasPOS
	inner join Facturacion_Electronica.dbo.Resoluciones on FacturasPOS.resolucionId = Resoluciones.ResolucionId
	inner join dbo.VENTAS v on FacturasPOS.ventaId = v.CONSECUTIVO
	inner join Facturacion_Electronica.dbo.terceros on FacturasPOS.terceroId = terceros.terceroId
    inner join Facturacion_Electronica.dbo.TipoIdentificaciones on terceros.tipoIdentificacion = TipoIdentificaciones.TipoIdentificacionId
	inner join dbo.CLIENTES on v.COD_CLI = CLIENTES.COD_CLI
	inner join dbo.MANGUERA on v.COD_MAN = MANGUERA.COD_MAN
	inner join dbo.TANQUES on MANGUERA.COD_TANQ = TANQUES.COD_TANQ
	inner join dbo.ARTICULO on TANQUES.Cod_art = ARTICULO.COD_ART
	where @COD_CAR = v.COD_CAR
	order by v.CONSECUTIVO desc
    
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