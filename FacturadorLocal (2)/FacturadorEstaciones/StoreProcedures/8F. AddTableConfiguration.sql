USE Facturacion_Electronica
GO
create table dbo.configuracionEstacion(
    configId INT PRIMARY KEY IDENTITY (1, 1),
    descripcion VARCHAR (50) NOT NULL,
    valor VARCHAR (50) NOT NULL
);
GO
INSERT INTO configuracionEstacion(descripcion,valor) values('ClientesCreditosGeneranFactura','SI')