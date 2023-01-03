USE estacion

GO
CREATE TRIGGER dbo.UpdateFacturaVolverImprimir
ON dbo.VENTAS
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
	declare  @ventaId int;

	select @ventaId = i.CONSECUTIVO
	from inserted i

    exec Facturacion_Electronica.dbo.SetFacturaNoImpresa @ventaId
END