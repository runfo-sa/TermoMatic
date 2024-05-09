USE [master]
GO

IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = 'TermoMatic')
	CREATE DATABASE [TermoMatic]
GO

USE [TermoMatic]
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = 'registro')
	EXEC sp_executesql N'CREATE SCHEMA [registro]'
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = 'configuracion')
	EXEC sp_executesql N'CREATE SCHEMA [configuracion]'
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = 'bitacora')
	EXEC sp_executesql N'CREATE SCHEMA [bitacora]'
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
		id INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
		loginUsuario VARCHAR(25) NULL,
		nombreEquipo VARCHAR(25) NULL,
		fechaEdicion SMALLDATETIME NULL
	)
END
GO

IF NOT EXISTS (SELECT 1 FROM sysobjects WHERE name = 'EdicionTemperaturaDetalle' AND xtype = 'U')
BEGIN
	CREATE TABLE [bitacora].[EdicionTemperaturaDetalle] (
		temperaturaId INT NOT NULL REFERENCES registro.Temperatura (id),
		temperaturaAnterior DECIMAL(9,3),
		temperaturaNueva DECIMAL(9,3),
		edicionTemperaturaCabeceraId INT REFERENCES bitacora.EdicionTemperaturaCabecera (id)
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

CREATE OR ALTER PROCEDURE [registro].[InsertarTemperaturaDesdeTabla] @TemperaturaSinProcesar TemperaturaSinProcesar READONLY
AS
	--EXECUTE registro.InsertarTemperatura 'PRU7', '-32.0', '21/04/2024'
	SET NOCOUNT ON

	DECLARE @LectoresNuevos TABLE(descripcion VARCHAR(30))

	INSERT INTO @LectoresNuevos
	SELECT 
		tps.LectorDescripcion
	FROM @TemperaturaSinProcesar tps
		LEFT JOIN configuracion.Lector l on l.descripcion = tps.LectorDescripcion
	WHERE
		l.id is null
	GROUP BY tps.LectorDescripcion
	
	IF (SELECT COUNT(descripcion) FROM @LectoresNuevos) > 0
	BEGIN
		INSERT INTO configuracion.Lector (descripcion)
		SELECT 
			descripcion
		FROM @LectoresNuevos
	END

	INSERT INTO registro.Temperatura(fecha, temperatura, lectorId)
	SELECT 
		CONVERT(smalldatetime, tps.FechaRegistro) AS fecha,
        CAST(REPLACE(ISNULL(tps.Temperatura, 0), ',', '.') AS decimal(9,3)) AS temperatura,
        lec.id AS lectorId
	FROM @TemperaturaSinProcesar tps 
		LEFT JOIN configuracion.Lector lec ON tps.LectorDescripcion = lec.Descripcion and lec.activo = 1;
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

CREATE OR ALTER   FUNCTION [registro].[CantidadDeLectoresDelDia] (@dia VARCHAR(30))
RETURNS INT
AS
BEGIN
	DECLARE @CantLectores INT

	SET @CantLectores = (SELECT COUNT(DISTINCT LectorID) FROM registro.Temperatura WHERE datediff(DAYOFYEAR, fecha, CAST(@dia AS DATE)) = 0 )

	RETURN @CantLectores
END
GO
