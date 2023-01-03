use Facturacion_Electronica
GO
Alter table FacturasPOS
Add Placa varchar(50) null;
Alter table FacturasPOS
Add Kilometraje varchar(50) null;