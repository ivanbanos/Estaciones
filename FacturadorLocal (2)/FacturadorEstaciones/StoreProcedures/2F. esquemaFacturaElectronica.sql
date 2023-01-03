GO
 IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = 'Facturacion_Electronica')
  BEGIN
    CREATE DATABASE [Facturacion_Electronica]


    END
    GO
       USE [Facturacion_Electronica]
    GO

	IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Resoluciones' and xtype='U')
BEGIN
    create table dbo.Resoluciones(
    ResolucionId INT PRIMARY KEY IDENTITY (1, 1),
    descripcion VARCHAR (50) NOT NULL,
    consecutivoInicio int NOT NULL,
    consecutivoFinal int NOT NULL,
    consecutivoActual int NOT NULL,
    fechaInicio DATETIME NOT NULL,
    fechafinal DATETIME NOT NULL,
    estado CHAR(2),
    esPOS CHAR(1),
);
END

GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='terceros' and xtype='U')
BEGIN
    create table dbo.terceros(
    terceroId INT PRIMARY KEY IDENTITY (1, 1),
    tipoIdentificacion int NOT NULL,
    identificacion VARCHAR (50) NOT NULL,
    nombre VARCHAR (50) NOT NULL,
    telefono VARCHAR (50) NOT NULL,
    correo VARCHAR (50) NOT NULL,
    direccion VARCHAR (50) NOT NULL,
    estado CHAR(2)
);
END

GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TipoIdentificaciones' and xtype='U')
BEGIN
    create table dbo.TipoIdentificaciones(
    TipoIdentificacionId INT PRIMARY KEY IDENTITY (1, 1),

    descripcion VARCHAR (50) NOT NULL,
    estado CHAR(2)
);
END


GO
ALTER TABLE terceros
ADD FOREIGN KEY (tipoIdentificacion) REFERENCES TipoIdentificaciones(TipoIdentificacionId);
GO

IF NOT EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'terceros' AND COLUMN_NAME = 'COD_CLI')
BEGIN
  
  ALTER TABLE terceros
ADD COD_CLI varchar(15);
END;
GO
IF NOT EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'TipoIdentificaciones' AND COLUMN_NAME = 'codigoDian')
BEGIN
  ALTER TABLE TipoIdentificaciones
ADD codigoDian smallint;
END;

GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='FacturasPOS' and xtype='U')
BEGIN
    create table dbo.FacturasPOS(
    facturaPOSId INT PRIMARY KEY IDENTITY (1, 1),
    fecha DATETIME NOT NULL,
    resolucionId int NOT NULL,
    consecutivo int NOT NULL,
    ventaId int NOT NULL,
    estado CHAR(2)
    FOREIGN KEY (resolucionId) REFERENCES dbo.Resoluciones (ResolucionId)
);

END


GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='facturasElectronicas' and xtype='U')
BEGIN
    create table dbo.facturasElectronicas(
    facturaElectronicaId INT PRIMARY KEY IDENTITY (1, 1),
    fecha DATETIME NOT NULL,
    resolucionId int NOT NULL,
    consecutivo int NOT NULL,
    estado CHAR(2)
    FOREIGN KEY (resolucionId) REFERENCES dbo.Resoluciones (ResolucionId)
);
END

GO
IF NOT EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'FacturasPOS' AND COLUMN_NAME = 'terceroId')
BEGIN
  ALTER TABLE FacturasPOS
ADD terceroId smallint;
END;
GO
ALTER TABLE FacturasPOS
ADD FOREIGN KEY (terceroId) REFERENCES terceros(terceroId);
GO
IF NOT EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'facturasElectronicas' AND COLUMN_NAME = 'terceroId')
BEGIN
  ALTER TABLE facturasElectronicas
ADD terceroId smallint;
END;
GO
ALTER TABLE facturasElectronicas
ADD FOREIGN KEY (terceroId) REFERENCES terceros(terceroId);
GO
IF NOT EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'FacturasPOS' AND COLUMN_NAME = 'facturaElectronicaId')
BEGIN
  ALTER TABLE FacturasPOS
ADD facturaElectronicaId smallint;
END;
GO
ALTER TABLE FacturasPOS
ADD FOREIGN KEY (facturaElectronicaId) REFERENCES facturasElectronicas(facturaElectronicaId);
GO
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'CrearFacturaElectronica')
	DROP PROCEDURE [dbo].[CrearFacturaElectronica]
GO

GO
IF type_id('[dbo].[facturaPOSIdType]') IS NOT NULL
        DROP TYPE [dbo].[facturaPOSIdType];
GO
CREATE TYPE [dbo].[facturaPOSIdType] AS TABLE(
	[ventaId] [int] NOT NULL
)

GO

GO
IF NOT EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'Resoluciones' AND COLUMN_NAME = 'autorizacion')
BEGIN
  ALTER TABLE Resoluciones
ADD autorizacion smallint;
END;
GO
declare @resolucionId INT
select @resolucionId = ResolucionId from Resoluciones
if @resolucionId is null
Begin
insert into Resoluciones (descripcion,consecutivoInicio,consecutivoFinal,
    consecutivoActual, fechaInicio, fechafinal, estado, esPOS, autorizacion)values('POS', 1, 30000, 1, '20200828', 
    '20210828', 'AC', 'S', '18764003223891')
end
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON

GO
ALTER TABLE
  Terceros
ALTER COLUMN
  identificacion
    VARCHAR(50) NULL;
ALTER TABLE
  Terceros
ALTER COLUMN
  tipoIdentificacion
    int NULL;
ALTER TABLE
  Terceros
ALTER COLUMN
  nombre
    VARCHAR(50) NULL;
ALTER TABLE
  Terceros
ALTER COLUMN
  telefono
    VARCHAR(50) NULL;
ALTER TABLE
  Terceros
ALTER COLUMN
  correo
    VARCHAR(250) NULL;
ALTER TABLE
  Terceros
ALTER COLUMN
  direccion
    VARCHAR(50) NULL;
GO
