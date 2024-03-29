﻿CREATE TYPE [dbo].[OrdenesDeDespachoType] AS TABLE 
([Guid] UNIQUEIDENTIFIER NULL, 
	IdFactura INT NULL,
	Identificacion NVARCHAR(250),
	NombreTercero NVARCHAR(250),
	Combustible NVARCHAR(50),
	Cantidad Decimal(18,3),
	Precio Decimal(18,3),
	Total Decimal(18,3) NULL,
	Descuento Decimal(18,3) NULL,
	IdInterno NVARCHAR(50),
	Placa NVARCHAR(50),
	Kilometraje NVARCHAR(50),
	IdEstadoActual Int NULL,
	Surtidor NVARCHAR(50),
	Cara NVARCHAR(50),
	Manguera NVARCHAR(50),  
    Fecha DATETIME,
	Estado NVARCHAR(50),  
	IdentificacionTercero NVARCHAR(250) ,
	FormaDePago NVARCHAR(250) ,
	IdLocal Int  NULL,
	IdVentaLocal Int  NULL,
	IdTerceroLocal Int  NULL,
    [IdEstacion] INT NULL,
	[SubTotal] DECIMAL(18, 3) NOT NULL, 
    [FechaProximoMantenimiento] DATETIME NOT NULL, 
    [Vendedor] NVARCHAR(50) NULL
	);  