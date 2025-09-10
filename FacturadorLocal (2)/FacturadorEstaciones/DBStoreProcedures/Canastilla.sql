USE Facturacion_Electronica

GO

	IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Canastilla' and xtype='U')
BEGIN
    create table dbo.Canastilla(
    CanastillaId INT PRIMARY KEY IDENTITY (1, 1),
    descripcion VARCHAR (50) NOT NULL,
	unidad VARCHAR (50) NOT NULL,
	precio float NOT NULL,
	deleted bit not null default 0,
	iva int not null default 0,
	[guid] uniqueidentifier
);
END
GO
IF NOT EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'Canastilla' AND COLUMN_NAME = 'iva')
BEGIN
  
  ALTER TABLE Canastilla
ADD iva int not null default 0;
END;
GO
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='FacturasCanastilla' and xtype='U')
BEGIN
    create table dbo.FacturasCanastilla(
    FacturasCanastillaId INT PRIMARY KEY IDENTITY (1, 1),
    fecha DATETIME NOT NULL,
    resolucionId int NOT NULL,
    consecutivo int NOT NULL,
    estado CHAR(2),
	terceroId int,
	impresa int default 0,
	enviada bit default 0,
	codigoFormaPago int not null default 4,
	subtotal float not null,
	descuento  float NOT NULL,
	iva float not null,
	total float NOT NULL,
    FOREIGN KEY (resolucionId) REFERENCES dbo.Resoluciones (ResolucionId)
);

END

GO
IF EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'FacturasCanastilla' AND COLUMN_NAME = 'canastillaId')
BEGIN
  
  ALTER TABLE FacturasCanastilla
drop column canastillaId;
END;

GO
IF NOT EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'FacturasCanastilla' AND COLUMN_NAME = 'Vendedor')
BEGIN
  
  ALTER TABLE FacturasCanastilla
add Vendedor varchar(50) null;
  ALTER TABLE FacturasCanastilla
add isla varchar(50) null;
  ALTER TABLE FacturasCanastilla
add fechaturno int null;
  ALTER TABLE FacturasCanastilla
add turno int null;
END;
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='FacturasCanastillaDetalle' and xtype='U')
BEGIN
    create table dbo.FacturasCanastillaDetalle(
    FacturasCanastillaDetalleId INT PRIMARY KEY IDENTITY (1, 1),
	FacturasCanastillaId INT NOT NULL,
	canastillaId int not null,
	cantidad float not null,
	precio float NOT NULL,
	subtotal float not null,
	iva float not null,
	total float NOT NULL,
    FOREIGN KEY (FacturasCanastillaId) REFERENCES dbo.FacturasCanastilla (FacturasCanastillaId)
);

END

GO

declare @config int

select @config=configId from configuracionEstacion where descripcion = 'ivaCanastila'

IF @config is null
begin
INSERT INTO configuracionEstacion(descripcion,valor) values('ivaCanastila','19')
end
GO

GO
declare @config int

select @config=configId from configuracionEstacion where descripcion = 'mismaResolucion'

IF @config is null
begin
INSERT INTO configuracionEstacion(descripcion,valor) values('mismaResolucion','SI')
end

GO
IF type_id('[dbo].[canastillaType]') IS NOT NULL
begin
DROP PROCEDURE [dbo].[CrearFacturaCanastilla]
DROP PROCEDURE UpdateOrCreateCanastilla
        DROP TYPE [dbo].[canastillaType];
		end
GO

create TYPE [dbo].[CanastillaType] AS TABLE 
(CanastillaId INT null,
    [Guid] uniqueidentifier NULL,
    descripcion VARCHAR (50) NOT NULL,
	unidad VARCHAR (50) NOT NULL,
	precio float NOT NULL,
	deleted bit not null default 0,
	iva int not null default 0,
	cantidad float NULL
	);  
GO
IF type_id('[dbo].[UpdateOrCreateCanastilla]') IS NOT NULL
begin
DROP PROCEDURE [dbo].[UpdateOrCreateCanastilla]
		end
GO
create PROCEDURE [dbo].[UpdateOrCreateCanastilla]
	@canastillas dbo.[CanastillaType] READONLY
AS
BEGIN
	

	UPDATE Canastilla SET Canastilla.descripcion =  c.descripcion,
	Canastilla.unidad =  c.unidad,
	Canastilla.precio =  c.precio,
	Canastilla.deleted =  c.deleted,
	Canastilla.iva =  c.iva
	
		from @canastillas c
		inner join Canastilla on  Canastilla.[Guid] = c.[Guid]

	Insert into Canastilla([Guid],descripcion,unidad,precio,deleted,iva)
	select c.[Guid],c.descripcion,c.unidad,c.precio,0,c.iva
	from @canastillas c
	left join Canastilla on  Canastilla.[Guid] = c.[Guid]
		where Canastilla.[Guid] is null

	UPDATE Canastilla SET 
	Canastilla.deleted =  1
	from Canastilla
		left join @canastillas c on  Canastilla.[Guid] = c.[Guid]
		where c.[Guid] is null

END
GO
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'CrearFacturaCanastilla')
	DROP PROCEDURE [dbo].[CrearFacturaCanastilla]
GO
CREATE procedure [dbo].[CrearFacturaCanastilla]
( 
	@terceroId int,
	@COD_FOR_PAG smallint,
	@canastillaIds [canastillaType] readonly,
	@descuento float,
	@imprimir bit = 1,
	@vendedor varchar(50) = null,
	@isla varchar(50) = null
)
as
begin try
    set nocount on;
    
    -- Variables principales
    declare @ResolucionId int, @consecutivoActual int, @fechafinal DATETIME, 
            @facturaCanastillaId int, @ConsecutivoFinal int, @cantidadCanastillas INT, 
            @mismaResolucion VARCHAR(50), @fecha int, @turno int;
    
    -- Variables para cálculos
    declare @subtotal float = 0, @totalIva float = 0, @total float = 0;
    declare @ivaPorcentaje bit = 0;
    
    -- Obtener información del turno
    select @fecha = FECHA, @turno = NUM_TUR 
    from ventas.dbo.TURN_EST 
    where TURN_EST.estado != 'C' and COD_ISL = @isla
    order by FECHA desc;
    
    -- Determinar tipo de IVA (porcentaje vs valor fijo)
    select @ivaPorcentaje = case when MAX(c.iva) < 30 then 1 else 0 end
    from @canastillaIds c;
    
    -- Calcular totales según tipo de IVA
    if @ivaPorcentaje = 1
    begin
        -- IVA como porcentaje - precio es sin IVA, calcular el IVA a agregar
        select 
            @subtotal = SUM(ROUND(c.cantidad * c.precio, 2)),
            @totalIva = SUM(ROUND((c.cantidad * c.precio) * (c.iva/100.0), 2))
        from @canastillaIds c;
    end
    else 
    begin
        -- IVA como valor fijo por unidad - precio es sin IVA
        select 
            @subtotal = SUM(ROUND(c.cantidad * c.precio, 2)),
            @totalIva = SUM(ROUND(c.cantidad * c.iva, 2))
        from @canastillaIds c;
    end
    
    -- Calcular total final
    set @total = @subtotal + @totalIva - @descuento;
    
    -- Validar que hay productos para facturar
    if @subtotal <= 0
    begin
        select 0 as facturaCanastillaId;
        return;
    end
    
    -- Determinar configuración de resolución
    select @cantidadCanastillas = count(ResolucionId) 
    from Resoluciones 
    where esPos = 'S' and estado = 'AC';
    
    set @mismaResolucion = case 
        when @cantidadCanastillas = 1 then 'SI' 
        else ISNULL((select valor from configuracionEstacion where descripcion = 'mismaResolucion'), 'NO')
    end;
    
    -- Obtener resolución y actualizar consecutivo
    select @ResolucionId = ResolucionId, 
           @consecutivoActual = consecutivoActual, 
           @fechafinal = fechafinal, 
           @ConsecutivoFinal = consecutivoFinal
    from Resoluciones 
    where esPos = 'S' and estado = 'AC' and (@mismaResolucion = 'SI' or tipo = 1);
    
    update Resoluciones 
    set consecutivoActual = @consecutivoActual + 1 
    where esPos = 'S' and estado = 'AC';
    
    -- Validar resolución activa
    if @fechafinal is null or @fechafinal < GETDATE() or @ConsecutivoFinal <= @consecutivoActual
    begin
        update Resoluciones 
        set estado = 'IN' 
        where esPos = 'S' and estado = 'AC' and (@mismaResolucion = 'SI' or tipo = 1);
        
        select @facturaCanastillaId as facturaCanastillaId;
        return;
    end
    
    -- Crear factura
    insert into FacturasCanastilla (
        fecha, resolucionId, consecutivo, estado, terceroid, enviada, 
        codigoFormaPago, subtotal, descuento, iva, total, impresa,
        vendedor, isla, fechaturno, turno
    )
    values (
        GETDATE(), @ResolucionId, @consecutivoActual, 'CR', @terceroId, 0, 
        @COD_FOR_PAG, @subtotal, @descuento, @totalIva, @total, -1,
        @vendedor, @isla, @fecha, @turno
    );
    
    set @facturaCanastillaId = SCOPE_IDENTITY();
    
    -- Crear detalle de factura
    if @ivaPorcentaje = 1
    begin
        -- IVA como porcentaje - precio es sin IVA, calcular total
        insert into FacturasCanastillaDetalle (
            FacturasCanastillaId, canastillaId, cantidad, precio, subtotal, iva, total
        )
        select 
            @facturaCanastillaId, 
            cids.canastillaId, 
            cids.cantidad, 
            cids.precio,
            ROUND(cids.cantidad * cids.precio, 2) as subtotal,
            ROUND((cids.cantidad * cids.precio) * (cids.iva/100.0), 2) as iva,
            ROUND((cids.cantidad * cids.precio) + ((cids.cantidad * cids.precio) * (cids.iva/100.0)), 2) as total
        from @canastillaIds cids;
    end
    else 
    begin
        -- IVA como valor fijo - precio es sin IVA, agregar IVA fijo
        insert into FacturasCanastillaDetalle (
            FacturasCanastillaId, canastillaId, cantidad, precio, subtotal, iva, total
        )
        select 
            @facturaCanastillaId, 
            cids.canastillaId, 
            cids.cantidad, 
            cids.precio,
            ROUND(cids.cantidad * cids.precio, 2) as subtotal,
            ROUND(cids.cantidad * cids.iva, 2) as iva,
            ROUND((cids.cantidad * cids.precio) + (cids.cantidad * cids.iva), 2) as total
        from @canastillaIds cids;
    end
    
    -- Retornar el consecutivo de la factura creada
    select consecutivo as facturaCanastillaId 
    from FacturasCanastilla 
    where FacturasCanastillaId = @facturaCanastillaId;
	
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
GO

IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'SetFacturaCanastillaImpresa')
	DROP PROCEDURE [dbo].[SetFacturaCanastillaImpresa]
GO
CREATE procedure [dbo].[SetFacturaCanastillaImpresa]
(
	@facturaCanastillaId int )
as
begin try
    
			
		Update FacturasCanastilla
				set impresa = 1
				from FacturasCanastilla
				where FacturasCanastilla.facturasCanastillaId = @facturaCanastillaId
			
    
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
GO

IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'MandarImprimirCanastilla')
	DROP PROCEDURE [dbo].[MandarImprimirCanastilla]
GO
CREATE procedure [dbo].[MandarImprimirCanastilla]
(
	@facturaCanastillaId int )
as
begin try
		declare @impresa int
		
		select @impresa = impresa from FacturasCanastilla
				where FacturasCanastilla.facturasCanastillaId = @facturaCanastillaId

		if @impresa >=0
		begin
		Update FacturasCanastilla
				set impresa = -1
				from FacturasCanastilla
				where FacturasCanastilla.facturasCanastillaId = @facturaCanastillaId
		end
		else begin
		
		Update FacturasCanastilla
				set impresa = impresa-1
				from FacturasCanastilla
				where FacturasCanastilla.facturasCanastillaId = @facturaCanastillaId
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
GO
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'getFacturaImprimirCanastilla')
	DROP PROCEDURE [dbo].[getFacturaImprimirCanastilla]
GO
CREATE procedure [dbo].[getFacturaImprimirCanastilla]
as
begin try
    set nocount on;



	select top(1)
	Resoluciones.descripcion as descripcionRes, Resoluciones.autorizacion, Resoluciones.consecutivoActual,
	Resoluciones.consecutivoFinal, Resoluciones.consecutivoInicio, Resoluciones.esPOS, Resoluciones.estado,
	Resoluciones.fechafinal, Resoluciones.fechaInicio, Resoluciones.ResolucionId, Resoluciones.habilitada, FacturasCanastilla.[FacturasCanastillaId]
      ,FacturasCanastilla.*, terceros.*, TipoIdentificaciones.*
	
	from dbo.FacturasCanastilla
	left join dbo.Resoluciones on FacturasCanastilla.resolucionId = Resoluciones.ResolucionId
	left join dbo.terceros on FacturasCanastilla.terceroId = terceros.terceroId
    left join dbo.TipoIdentificaciones on terceros.tipoIdentificacion = TipoIdentificaciones.TipoIdentificacionId
	where FacturasCanastilla.impresa <= -1
	

    
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
GO
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'getFacturaCanatillaDetalle')
	DROP PROCEDURE [dbo].[getFacturaCanatillaDetalle]
GO
CREATE procedure [dbo].[getFacturaCanatillaDetalle]
(@FacturaCanastillaId int)
as
begin try
    set nocount on;



	select *
	
	from dbo.FacturasCanastillaDetalle
	left join dbo.Canastilla on FacturasCanastillaDetalle.canastillaId = Canastilla.canastillaId
	where FacturasCanastillaDetalle.facturasCanastillaId  = @FacturaCanastillaId
	

    
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
GO
GO
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'GetCanastilla')
	DROP PROCEDURE [dbo].[GetCanastilla]
GO
CREATE procedure [dbo].[GetCanastilla]
as
begin try
    set nocount on;



	select *
	
	from dbo.Canastilla
	where deleted = 0

    
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
GO 
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'getFacturaImprimirCanastilla')
	DROP PROCEDURE [dbo].[getFacturaEnviarCanastilla]
GO
CREATE procedure [dbo].[getFacturaEnviarCanastilla]
as
begin try
    set nocount on;



	select top(10)
	Resoluciones.descripcion as descripcionRes, Resoluciones.autorizacion, Resoluciones.consecutivoActual,
	Resoluciones.consecutivoFinal, Resoluciones.consecutivoInicio, Resoluciones.esPOS, Resoluciones.estado,
	Resoluciones.fechafinal, Resoluciones.fechaInicio, Resoluciones.ResolucionId, Resoluciones.habilitada, FacturasCanastilla.[FacturasCanastillaId]
      ,FacturasCanastilla.*, terceros.*, TipoIdentificaciones.*
	
	from dbo.FacturasCanastilla
	left join dbo.Resoluciones on FacturasCanastilla.resolucionId = Resoluciones.ResolucionId
	left join dbo.terceros on FacturasCanastilla.terceroId = terceros.terceroId
    left join dbo.TipoIdentificaciones on terceros.tipoIdentificacion = TipoIdentificaciones.TipoIdentificacionId
	where FacturasCanastilla.enviada = 0
	order by FacturasCanastilla.FacturasCanastillaId desc
	

    
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
GO
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'SetFacturaCanastillaEnviada')
	DROP PROCEDURE [dbo].[SetFacturaCanastillaEnviada]
GO
CREATE procedure [dbo].[SetFacturaCanastillaEnviada]
(
	
	@facturas [ventasIds] readonly )
as
begin try
    
			
		Update FacturasCanastilla
				set enviada = 1
				from FacturasCanastilla
    inner join @facturas f on f.ventaId = FacturasCanastilla.FacturasCanastillaId
			
    
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
GO
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'BuscarFacturaCanastillaPorConsecutivo')
	DROP PROCEDURE [dbo].[BuscarFacturaCanastillaPorConsecutivo]
GO
CREATE procedure [dbo].[BuscarFacturaCanastillaPorConsecutivo]
(@consecutivo INT)
as
begin try
    set nocount on;



	select 
	Resoluciones.descripcion as descripcionRes, Resoluciones.autorizacion, Resoluciones.consecutivoActual,
	Resoluciones.consecutivoFinal, Resoluciones.consecutivoInicio, Resoluciones.esPOS, Resoluciones.estado,
	Resoluciones.fechafinal, Resoluciones.fechaInicio, Resoluciones.ResolucionId, Resoluciones.habilitada, FacturasCanastilla.[FacturasCanastillaId]
      ,FacturasCanastilla.*, terceros.*, TipoIdentificaciones.*
	
	from dbo.FacturasCanastilla
	left join dbo.Resoluciones on FacturasCanastilla.resolucionId = Resoluciones.ResolucionId
	left join dbo.terceros on FacturasCanastilla.terceroId = terceros.terceroId
    left join dbo.TipoIdentificaciones on terceros.tipoIdentificacion = TipoIdentificaciones.TipoIdentificacionId
	where FacturasCanastilla.consecutivo = @consecutivo
	

    
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
GO
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'FacturasCanastillaPorImprimir')
	DROP PROCEDURE [dbo].[FacturasCanastillaPorImprimir]
GO
CREATE procedure [dbo].[FacturasCanastillaPorImprimir]
as
begin try
    set nocount on;



	select FacturasCanastillaId
	from FacturasCanastilla
	where FacturasCanastilla.impresa <= -1
	

    
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
GO


delete FacturasCanastilla from FacturasCanastilla fc
left join FacturasCanastillaDetalle fcd
on fc.FacturasCanastillaId = fcd.FacturasCanastillaId
where fcd.FacturasCanastillaDetalleId is null

GO
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'GetFacturasCanastillaIslaTurno')
    DROP PROCEDURE [dbo].[GetFacturasCanastillaIslaTurno]
GO
CREATE PROCEDURE [dbo].[GetFacturasCanastillaIslaTurno]
    @isla VARCHAR(50) = NULL,
    @fechaturno INT = NULL,
    @turno INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    select 
	Resoluciones.descripcion as descripcionRes, Resoluciones.autorizacion, Resoluciones.consecutivoActual,
	Resoluciones.consecutivoFinal, Resoluciones.consecutivoInicio, Resoluciones.esPOS, Resoluciones.estado,
	Resoluciones.fechafinal, Resoluciones.fechaInicio, Resoluciones.ResolucionId, Resoluciones.habilitada, FacturasCanastilla.[FacturasCanastillaId]
      ,FacturasCanastilla.*, terceros.*, TipoIdentificaciones.*
	
	from dbo.FacturasCanastilla
	left join dbo.Resoluciones on FacturasCanastilla.resolucionId = Resoluciones.ResolucionId
	left join dbo.terceros on FacturasCanastilla.terceroId = terceros.terceroId
    left join dbo.TipoIdentificaciones on terceros.tipoIdentificacion = TipoIdentificaciones.TipoIdentificacionId
	
    WHERE (@isla IS NULL OR isla = @isla)
      AND (@fechaturno IS NULL OR fechaturno = @fechaturno)
      AND (@turno IS NULL OR turno = @turno)
END
GO