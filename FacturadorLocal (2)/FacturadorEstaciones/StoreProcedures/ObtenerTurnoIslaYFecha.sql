-- Stored procedure to get turno by isla, numero (id), and fecha
-- Returns 3 tables: turno main data, turno surtidores, and bolsas
drop procedure if exists [dbo].[ObtenerTurnoIslaYFecha]
GO
CREATE procedure [dbo].[ObtenerTurnoIslaYFecha]
(@fecha datetime, @idIsla int, @num_tur int)
as
begin try
    set nocount on;
    
    -- Query main turno data (Table 0)
	select turno.Id as Numero, 
           ISNULL(Empleado.Nombre, '') as empleado, 
           turno.FechaApertura, 
           turno.FechaCierre, 
           turno.IdEstado, 
           Isla.Descripcion as Isla,
           (YEAR(turno.FechaApertura) - 2000) * 1000 + DAY(turno.FechaApertura) as FECHA
	from dbo.Turno 
	inner join isla on Turno.IdIsla = Isla.Id
	left join empleado on turno.IdEmpleado = empleado.Id
	where turno.Id = @num_tur
	  and turno.IdIsla = @idIsla
	  and CAST(turno.FechaApertura as DATE) = CAST(@fecha as DATE);
    
    -- Query turno surtidor data (Table 1) - currently returns empty result set
    select 
        CAST(0.0 as float) as Apertura,
        CAST(0.0 as float) as Cierre,
        '' as Combustible,
        CAST(0 as smallint) as Manguera,
        CAST(0.0 as decimal(18,2)) as precioCombustible,
        CAST(0 as smallint) as Surtidor
    where 1=0;
    
    -- Query bolsa data (Table 2) - currently returns empty result set
    select 
        GETDATE() as Fecha,
        0 as Consecutivo,
        @num_tur as NumeroTurno,
        CAST(@idIsla as varchar) as Isla,
        '' as Empleado,
        CAST(0.0 as decimal(18,2)) as Moneda,
        CAST(0.0 as decimal(18,2)) as Billete
    where 1=0;
    
end try
begin catch
    declare 
        @errorMessage varchar(2000),
        @errorProcedure varchar(255),
        @errorLine int;

    select  
        @errorMessage = error_message(),
        @errorProcedure = error_procedure(),
        @errorLine = error_line();

    raiserror (N'<message>Error occurred in %s :: %s :: Line number: %d</message>', 16, 1, @errorProcedure, @errorMessage, @errorLine);
end catch;
GO
