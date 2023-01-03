USE [Facturacion_Electronica]
GO
alter table Resoluciones ADD autorizacion varchar(50)
GO
update Resoluciones set estado = 'VE'
insert into Resoluciones (descripcion,consecutivoInicio,consecutivoFinal,
    consecutivoActual, fechaInicio, fechafinal, estado, esPOS, autorizacion)values('POS', 1, 30000, 1, '20200828', 
    '20210828', 'AC', 'S', '18764003223891')
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
