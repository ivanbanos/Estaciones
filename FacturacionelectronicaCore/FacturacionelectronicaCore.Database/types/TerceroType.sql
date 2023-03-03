﻿CREATE TYPE [dbo].[TerceroType] AS TABLE
(
	[Id] INT ,
    [Guid] uniqueidentifier NULL,
	Nombre NVARCHAR(250) NULL,
	Segundo NVARCHAR(250) NULL,
	Apellidos NVARCHAR(250) NULL,
	TipoPersona int null,
	ResponsabilidadTributaria int null,
	Municipio NVARCHAR(250) NULL,
	Departamento NVARCHAR(250) NULL,
	Direccion NVARCHAR(250) NULL,
	Pais NVARCHAR(250) NULL,
	CodigoPostal NVARCHAR(250) NULL,
	Celular NVARCHAR(250) NULL,
	Telefono NVARCHAR(250) NULL,
	Telefono2 NVARCHAR(250) NULL,
	Correo NVARCHAR(250) NULL,
	Correo2 NVARCHAR(250) NULL,
	Vendedor NVARCHAR(250) NULL,
	Comentarios NVARCHAR(500) NULL,
	TipoIdentificacion INT NULL,
	Identificacion NVARCHAR(250) NULL,
	DescripcionTipoIdentificacion nvarchar(200) NULL,
	IdLocal Int  NULL,
	IdContable int null
)
