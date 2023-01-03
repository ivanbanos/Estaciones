use Facturacion_Electronica
GO
Alter table FacturasPOS 
add enviada bit default 0
GO
Alter table Terceros 
add enviada bit default 0