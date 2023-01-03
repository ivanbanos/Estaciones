USE Facturacion_Electronica
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