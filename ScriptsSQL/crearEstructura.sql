IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = 'TermoMatic')
	CREATE DATABASE [TermoMatic]
GO

USE [TermoMatic]
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = 'bitacora')
	EXEC sp_executesql N'CREATE SCHEMA [bitacora]'
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = 'configuracion')
	EXEC sp_executesql N'CREATE SCHEMA [configuracion]'
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = 'registro')
	EXEC sp_executesql N'CREATE SCHEMA [registro]'
GO

IF NOT EXISTS (SELECT 1 FROM sysobjects WHERE name = 'Equipo' AND xtype = 'U')
BEGIN
	CREATE TABLE [configuracion].[Equipo](
		[id] [int] IDENTITY(1,1) PRIMARY KEY NOT NULL,
		[descripcion] [varchar](50) NULL
		)
END
GO


IF NOT EXISTS (SELECT 1 FROM sysobjects WHERE name = 'Lector' AND xtype = 'U')
BEGIN
	CREATE TABLE [configuracion].[Lector] (
		id INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
		descripcion VARCHAR(30) NULL,
		activo BIT NULL DEFAULT(1),
		remplazadoPor INT REFERENCES configuracion.Lector (id) DEFAULT(NULL)
	)
END
GO

IF NOT EXISTS (SELECT 1 FROM sysobjects WHERE name = 'Usuario' AND xtype = 'U')
BEGIN
	CREATE TABLE [configuracion].[Usuario](
		[id] [int] IDENTITY(1,1) PRIMARY KEY NOT NULL,
		[nombre] [varchar](50) NULL
		)
END
GO

IF NOT EXISTS (SELECT 1 FROM sysobjects WHERE name = 'Temperatura' AND xtype = 'U')
BEGIN
	CREATE TABLE [registro].[Temperatura](
		id INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
		fecha SMALLDATETIME NULL,
		temperatura DECIMAL(9,3) NULL,
		lectorId INT NULL REFERENCES configuracion.lector (id)
	)
END
GO

IF NOT EXISTS (SELECT 1 FROM sysobjects WHERE name = 'EdicionTemperaturaCabecera' AND xtype = 'U')
BEGIN
	CREATE TABLE [bitacora].[EdicionTemperaturaCabecera](
		[id] [int] IDENTITY(1,1) PRIMARY KEY NOT NULL,
		[usuarioId] [int] NULL REFERENCES [configuracion].[Usuario] ([id]),
		[equipoId] [int] NULL REFERENCES [configuracion].[Equipo] ([id]),
		[fechaEdicion] [smalldatetime] NULL
		)
END
GO

IF NOT EXISTS (SELECT 1 FROM sysobjects WHERE name = 'EdicionTemperaturaDetalle' AND xtype = 'U')
BEGIN
	CREATE TABLE [bitacora].[EdicionTemperaturaDetalle](
		[temperaturaId] [int] NOT NULL REFERENCES [registro].[Temperatura] ([id]),
		[temperaturaAnterior] [decimal](9, 3) NULL,
		[temperaturaNueva] [decimal](9, 3) NULL,
		[edicionTemperaturaCabeceraId] [int] NULL REFERENCES [bitacora].[EdicionTemperaturaCabecera] ([id])
	)
END
GO

IF NOT EXISTS (SELECT 1 FROM systypes WHERE name = 'TemperaturaSinProcesar')
BEGIN
	CREATE TYPE [dbo].[TemperaturaSinProcesar] AS TABLE(
		[LectorDescripcion] [varchar](15) NULL,
		[Temperatura] [varchar](15) NULL,
		[FechaRegistro] [varchar](30) NULL
	)
END
GO

CREATE OR ALTER   PROCEDURE [configuracion].[ImportarDatosDeFormatoAnterior]
AS
BEGIN
	PRINT('ESTE PROCEDIMIENTO SE HIZO PARA IMPORTAR LAS TEMPERATURAS DEL FORMATO VIEJO AL NUEVO, PERO SOLO ES VÁLIDO PARA EL SERVER DE TEST PORQUE ACÁ NO TENEMOS LAS TEMPERATURAS VIEJAS')

	---- Desarrollador: Alcides L.
	---- Objetivo: Queremos pasar las temperaturas ya registras en la db vieja del sistema a la nueva.
	--
	--SET NOCOUNT ON
	--
	---- Nuestras variables son una tabla con las temperaturas que tenemos que procesar y un poco de sql dinámico.
	--DECLARE @cmdSQL NVARCHAR(MAX)
	--DECLARE @tAuxi TemperaturaSinProcesar
	--
	---- Vamos a armar un SELECT con las temperaturas viejas en el formato nuevo, es decir, LECTOR-TEMPERATURA-FECHAHORA.
	--SET @cmdSQL = (
	--	SELECT 
	--		STRING_AGG(CAST('SELECT ' AS VARCHAR(MAX)) + QUOTENAME(TRIM(NOMB_CAMPO), '''') + ', ' + QUOTENAME(TRIM(NOMB_TALA)) + ', [CAMPO2] FROM OFFAL_DB.dbo.temp2018', ' UNION ALL ')
	--	FROM OFFAL_DB.dbo.DESCRIPCIONES_CAMPOS
	--	WHERE NOMB_CAMPO NOT IN ('SIN USO', 'KEY', 'FECHA')
	--)
	--
	---- Insertamos en nuestra querida tabla las mieles del éxito.
	--INSERT INTO @tAuxi
	--EXECUTE sp_executesql @cmdSQL
	--
	---- Arreglamos las fechas mal registradas.
	--UPDATE @tAuxi
	--SET FechaRegistro = REPLACE(REPLACE(FechaRegistro, 'a.m.', 'AM'), 'p.m.', 'PM') 
	--WHERE ISDATE(FechaRegistro) = 0
	--
	---- Aprovechamos que ya tenemos un SP que genera registros en la db nueva desde una tabla.
	--EXECUTE registro.InsertarTemperaturaDesdeTabla @tAuxi
END
GO

CREATE OR ALTER   PROCEDURE [registro].[ActualizarTemperaturaDesdeTabla] @TemperaturaSinProcesar TemperaturaSinProcesar READONLY, @Usuario VARCHAR(50), @Equipo VARCHAR(50)
AS
BEGIN
	-- Desarrollador: Alcides L.
	-- Objetivo: Actualizar (y registrar eso) las temperaturas por lote. 
	-- Fecha:  26/06/2024

	SET NOCOUNT ON

	-- Nuestras variables necesarias son una tabla auxiliar para las temperaturas y donde guardar el id de la cabecera que creamos.
	DECLARE @TemperaturasExistentes TABLE(TemperaturaId INT, TemperaturaNueva DECIMAL(9,3), TemperaturaVieja DECIMAL(9,3))
	DECLARE @EdicionTemperaturaCabeceraId INT

	-- Verificamos si el usuario es nuevo, en ese caso lo creamos.
	IF (SELECT 1 FROM configuracion.Usuario WHERE nombre = @Usuario) IS NULL
	BEGIN
		INSERT INTO configuracion.Usuario(nombre)
		VALUES(@Usuario)
	END

	-- Verificamos si la pc es nueva, en ese caso la creamos.
	IF (SELECT 1 FROM configuracion.Equipo WHERE descripcion = @Equipo) IS NULL
	BEGIN
		INSERT INTO configuracion.Equipo(descripcion)
		VALUES(@Equipo)
	END

	-- Hacemos un insert en la tabla de cabeceras los datos de la pc y el usuario, además del cuando de la edición.
	INSERT INTO bitacora.EdicionTemperaturaCabecera(usuarioId, equipoId, fechaEdicion)
	SELECT TOP 1
		u.id,
		e.id,
		getdate()
	FROM configuracion.Usuario u, configuracion.Equipo e
	WHERE e.descripcion = @Equipo AND u.nombre = @Usuario

	-- Leemos el id recién generado.
	SET @EdicionTemperaturaCabeceraId = (SELECT TOP 1 
											etc.id
										FROM bitacora.EdicionTemperaturaCabecera etc
											INNER JOIN configuracion.Equipo e on e.id = etc.equipoId
											INNER JOIN configuracion.Usuario u on u.id = etc.usuarioId
										WHERE e.descripcion = @Equipo AND u.nombre = @Usuario
										ORDER BY etc.id DESC)

	-- Guardamos las temperaturas en la tabla auxiliar.
	INSERT INTO @TemperaturasExistentes (TemperaturaId, TemperaturaNueva, TemperaturaVieja)
	SELECT
		t.id,
		CAST(REPLACE(ISNULL(tps.Temperatura, 0), ',', '.') AS DECIMAL(9,3)),
		t.temperatura
	FROM @TemperaturaSinProcesar tps
		INNER JOIN configuracion.Lector l on l.descripcion = tps.LectorDescripcion
		INNER JOIN registro.Temperatura t on t.lectorId = l.id AND t.fecha = tps.FechaRegistro

	-- Insertamos en la bitacora los cambios realizados en los registros. 
	INSERT INTO bitacora.EdicionTemperaturaDetalle (temperaturaId, temperaturaAnterior, temperaturaNueva, edicionTemperaturaCabeceraId)
		SELECT
			TemperaturaId,
			TemperaturaVieja,
			TemperaturaNueva,
			@EdicionTemperaturaCabeceraId
		FROM @TemperaturasExistentes

	-- Y finalizamos guardando los cambios.
	UPDATE temp
		SET temp.temperatura = tempNueva.temperaturaNueva
	FROM registro.Temperatura temp
		INNER JOIN @TemperaturasExistentes tempNueva ON temp.id = tempNueva.TemperaturaId
END
GO

CREATE OR ALTER     PROCEDURE [registro].[InsertarTemperaturaDesdeTabla] @TemperaturaSinProcesar TemperaturaSinProcesar READONLY
AS
BEGIN
	-- Desarrollador: Alcides L.

	-- Fecha:  12/04/2024

	SET NOCOUNT ON

	---- NECESITAMOS UNA TABLA CON LOS LECTORES NUEVOS Y OTRA CON LAS MEDICIONES DE TEMPERATURA QUE NO ESTÉN YA INGRESADAS.
	DECLARE @LectoresNuevos TABLE(descripcion VARCHAR(30))
	DECLARE @TemperaturaSinRepetir TemperaturaSinProcesar

	---- PROCESAMOS LOS LECTORES NUEVOS.
		INSERT INTO @LectoresNuevos
		SELECT 
			tps.LectorDescripcion
		FROM @TemperaturaSinProcesar tps
			LEFT JOIN configuracion.Lector l on l.descripcion = tps.LectorDescripcion
		WHERE
			l.id IS NULL
		GROUP BY tps.LectorDescripcion
	
	---- SI TENEMOS ALGÚN LECTOR NUEVO LO GUARDAMOS.
		IF (SELECT COUNT(descripcion) FROM @LectoresNuevos) > 0
		BEGIN
			INSERT INTO configuracion.Lector (descripcion)
			SELECT 
				descripcion
			FROM @LectoresNuevos
		END
	-----------------------------------------------

	---- GUARDAMOS DE FORMA TEMPORAL LAS LECTURAS QUE NO ESTAN REPETIDAS.
	INSERT INTO @TemperaturaSinRepetir
	SELECT
		tps.LectorDescripcion,
		tps.Temperatura,
		tps.FechaRegistro
	FROM @TemperaturaSinProcesar tps
		INNER JOIN configuracion.Lector l on l.descripcion = tps.LectorDescripcion
		LEFT JOIN registro.Temperatura t on t.lectorId = l.id AND t.fecha = tps.FechaRegistro
	WHERE t.fecha IS NULL

	---- INSERTAMOS LAS TEMPERATURAS NO REPETIDAS.
	INSERT INTO registro.Temperatura(fecha, temperatura, lectorId)
	SELECT 
		CONVERT(SMALLDATETIME, tps.FechaRegistro) AS fecha,
        CAST(REPLACE(ISNULL(tps.Temperatura, 0), ',', '.') AS decimal(9,3)) AS temperatura,
        lec.id AS lectorId
	FROM @TemperaturaSinRepetir tps 
		INNER JOIN configuracion.Lector lec ON tps.LectorDescripcion = lec.Descripcion AND lec.activo = 1

	---- ¿QUÉ HACEMOS CON LAS TEMPERATURAS REPETIDAS?
	---- POR AHORA NADA, NO DESCARTO A FUTURO TENER QUE HACER UN UPDATE.
END
GO

CREATE OR ALTER FUNCTION [registro].[CantidadDeLectoresDelDia] (@dia VARCHAR(30))
RETURNS INT
AS
BEGIN
	DECLARE @CantLectores INT

	SET @CantLectores = (SELECT COUNT(DISTINCT LectorID) FROM registro.Temperatura WHERE datediff(DAYOFYEAR, fecha, CAST(@dia AS DATE)) = 0 )

	RETURN @CantLectores
END
GO
