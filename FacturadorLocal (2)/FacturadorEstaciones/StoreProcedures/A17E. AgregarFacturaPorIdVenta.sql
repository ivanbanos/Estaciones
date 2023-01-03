Use Estacion
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

CREATE procedure [dbo].[AgregarFacturaPorIdVenta]
(
	@ventaId int
)
as
begin try
    set nocount on;
	declare @terceroId int, @Placa varchar(50), @Kilometraje varchar(50), @COD_FOR_PAG smallint;

	select @ventaId = i.CONSECUTIVO,
	@Placa = i.PLACA,
	@Kilometraje = i.KIL_ACT,
	@COD_FOR_PAG = i.COD_FOR_PAG
	from VENTAS i
	where i.CONSECUTIVO = @ventaId

    select @terceroId = terceroId
	from Facturacion_Electronica.dbo.terceros t
    inner join VENTAS v on v.COD_CLI = t.COD_CLI
	where v.CONSECUTIVO = @ventaId

	If @terceroId is null
	begin
		declare @tipoIdentificacion int,
		@identificacion VARCHAR (50),
		@nombre VARCHAR (50),
		@telefono VARCHAR (50),
		@correo VARCHAR (50),
		@direccion VARCHAR (50),
		@COD_CLI char(15)

		select @tipoIdentificacion = TipoIdentificacionId,
		@identificacion = c.NIT,
		@nombre = c.NOMBRE,
		@telefono = c.TEL_PARTICULAR,
		@direccion = c.DIR_PARTICULAR,
		@COD_CLI = c.COD_CLI
		from Facturacion_Electronica.dbo.TipoIdentificaciones ti
		inner join CLIENTES c on c.TIPO_NIT = ti.descripcion
		inner join VENTAS i on i.COD_CLI = c.COD_CLI 
		where i.CONSECUTIVO = @ventaId
		
		If @tipoIdentificacion is null
		Begin
			select @tipoIdentificacion = TipoIdentificacionId 
			from Facturacion_Electronica.dbo.TipoIdentificaciones ti
			where ti.descripcion = 'No especificada'
		End
		if @COD_CLI is not null
		begin
		select @terceroId = t.terceroId  from Facturacion_Electronica.dbo.terceros t
			where @COD_CLI = t.COD_CLI
			if @terceroId is null
			begin
			
		insert into Facturacion_Electronica.dbo.terceros(COD_CLI,correo,direccion,estado,identificacion,nombre,telefono,tipoIdentificacion)
		values(@COD_CLI, 'no informado', @direccion, 'AC', @identificacion, @nombre, @telefono, @tipoIdentificacion)
			select @terceroId = SCOPE_IDENTITY()
			end

		end
		else
		begin
			
			select @terceroId = t.terceroId from Facturacion_Electronica.dbo.terceros t
			where t.nombre like '%CONSUMIDOR FINAL%'
			if @terceroId is null
			begin
			insert into Facturacion_Electronica.dbo.terceros(COD_CLI,correo,direccion,estado,identificacion,nombre,telefono,tipoIdentificacion)
			values(@COD_CLI, 'no informado', 'no informado', 'AC', '222222222222', 'CONSUMIDOR FINAL', 'no informado', @tipoIdentificacion)

			select @terceroId = SCOPE_IDENTITY()
			end
		end
	end
    exec Facturacion_Electronica.dbo.CrearFactura @ventaId, @terceroId, @Placa, @Kilometraje, @COD_FOR_PAG
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