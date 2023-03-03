﻿CREATE TABLE [dbo].[OrdenesDeDespacho]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[Guid] UNIQUEIDENTIFIER NOT NULL, 
	IdFactura INT NULL,
	IdTercero INT NOT NULL,
	Combustible NVARCHAR(50),
	Cantidad Decimal(18,3),
	Precio Decimal(18,3),
	Total Decimal(18,3) NOT NULL,
	Descuento Decimal(18,3) NULL,
	IdInterno NVARCHAR(50),
	Placa NVARCHAR(50),
	Kilometraje NVARCHAR(50),
	IdEstadoActual Int NOT NULL,
	Surtidor NVARCHAR(50),
	Cara NVARCHAR(50),
	Manguera NVARCHAR(50),  
    Fecha DATETIME NULL,
	FormaDePago NVARCHAR(250) ,
	IdLocal Int  NULL,
	IdVentaLocal Int  NULL,
    [IdEstacion] INT NULL, 
	[SubTotal] DECIMAL(18, 3) NULL, 
    [FechaProximoMantenimiento] DATETIME NULL, 
    [Vendedor] NVARCHAR(50) NULL, 
    [FechaReporte] DATETIME NULL, 
    CONSTRAINT [FK_OrdenesDeDespacho_Facturas] FOREIGN KEY ([IdFactura]) REFERENCES [Facturas]([Id]), 
    CONSTRAINT [FK_OrdenesDeDespacho_Terceros] FOREIGN KEY ([IdTercero]) REFERENCES [Terceros]([Id]),
    CONSTRAINT [FK_OrdenesDeDespacho_ToTable] FOREIGN KEY ([IdEstadoActual]) REFERENCES [Estados]([Id])
)
