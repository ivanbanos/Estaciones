USE estacion

GO
ALTER TRIGGER dbo.crearFacturaVenta
ON dbo.VENTAS
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
	declare @terceroId int, @ventaId int, @Placa varchar(50), @Kilometraje varchar(50), @COD_FOR_PAG smallint, @COD_CLI varchar(15);

	select @ventaId = i.CONSECUTIVO,
	@Placa = i.PLACA,
	@Kilometraje = i.KIL_ACT,
	@COD_FOR_PAG = COD_FOR_PAG,
	@COD_CLI = i.COD_CLI
	from inserted i

	select @terceroId = terceroId
	from Facturacion_Electronica.dbo.terceros t
    where @COD_CLI= t.COD_CLI



	If @terceroId is null
	begin
		declare @tipoIdentificacion int,
		@identificacion VARCHAR (50),
		@nombre VARCHAR (50),
		@telefono VARCHAR (50),
		@correo VARCHAR (50),
		@direccion VARCHAR (50),
		@TIPO_NIT VARCHAR (50)



		select @TIPO_NIT = TIPO_NIT,
		@identificacion = c.NIT,
		@nombre = c.NOMBRE,
		@telefono = c.TEL_PARTICULAR,
		@direccion = c.DIR_PARTICULAR,
		@COD_CLI = c.COD_CLI
		from CLIENTES c
		inner join inserted i on i.COD_CLI = c.COD_CLI 

		
		select @tipoIdentificacion = TipoIdentificacionId
		from Facturacion_Electronica.dbo.TipoIdentificaciones ti
		where @TIPO_NIT = ti.descripcion
		
		If @tipoIdentificacion is null
		Begin
			select @tipoIdentificacion = TipoIdentificacionId 
			from Facturacion_Electronica.dbo.TipoIdentificaciones ti
			where ti.descripcion = 'No especificada'
		End
		if @COD_CLI is not null and @identificacion is not null
		begin
		insert into Facturacion_Electronica.dbo.terceros(COD_CLI,correo,direccion,estado,identificacion,nombre,telefono,tipoIdentificacion)
		values(@COD_CLI, 'no informado', @direccion, 'AC', @identificacion, @nombre, @telefono, @tipoIdentificacion)

		select @terceroId = SCOPE_IDENTITY()
		end
		else
		begin
			select @terceroId from Facturacion_Electronica.dbo.terceros
			where nombre = 'CONSUMIDOR FINAL'
			if @terceroId is null
			begin
			insert into Facturacion_Electronica.dbo.terceros(COD_CLI,correo,direccion,estado,identificacion,nombre,telefono,tipoIdentificacion)
			values(@COD_CLI, 'no informado', 'no informado', 'AC', '222222222222', 'CONSUMIDOR FINAL', 'no informado', @tipoIdentificacion)

			select @terceroId = SCOPE_IDENTITY()
			end
			
		end
	end
    exec Facturacion_Electronica.dbo.CrearFactura @ventaId, @terceroId, @Placa, @Kilometraje, @COD_FOR_PAG
END