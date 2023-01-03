Use Estacion
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

CREATE procedure [dbo].[getVentaPorId]
(
	@CONSECUTIVO int
)
as
begin try
    set nocount on; 

	select MANGUERA.COD_MAN, MANGUERA.COD_TANQ, ARTICULO.DESCRIPCION, MANGUERA.DS_ROM,
	v.*, CLIENTES.*, IMPRESOR.*
	
	from dbo.VENTAS v
	left join dbo.CLIENTES on v.COD_CLI = CLIENTES.COD_CLI
	left join dbo.MANGUERA on v.COD_MAN = MANGUERA.COD_MAN
	left join dbo.TANQUES on MANGUERA.COD_TANQ = TANQUES.COD_TANQ
	left join dbo.ARTICULO on v.Cod_art = ARTICULO.COD_ART
	left join dbo.POS on v.COD_ISL = POS.COD_ISL
	left join dbo.IMPRESOR on POS.NUM_POS = IMPRESOR.NUM_POS
	where CONSECUTIVO = @CONSECUTIVO
    
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
exec getVentaPorId @COD_CAR = 2, @identificacion = 1
select * from ventas
*/