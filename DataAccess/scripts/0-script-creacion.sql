USE [db-bolsa-trabajo-local];
GO

-- ========= Tablas (sin índices extra; PK/UNIQUE solamente) =========

IF OBJECT_ID('dbo.Rol','U') IS NULL
CREATE TABLE dbo.Rol (
  id INT IDENTITY(1,1) PRIMARY KEY,
  codigo NVARCHAR(45) NOT NULL,
  nombre NVARCHAR(45) NOT NULL,
  CONSTRAINT UQ_Rol_codigo UNIQUE(codigo)
);

IF OBJECT_ID('dbo.Genero','U') IS NULL
CREATE TABLE dbo.Genero (
  id INT IDENTITY(1,1) PRIMARY KEY,
  codigo NVARCHAR(45) NOT NULL,
  nombre NVARCHAR(45) NOT NULL,
  CONSTRAINT UQ_Genero_codigo UNIQUE(codigo)
);

IF OBJECT_ID('dbo.Modalidad','U') IS NULL
CREATE TABLE dbo.Modalidad (
  id INT IDENTITY(1,1) PRIMARY KEY,
  codigo NVARCHAR(45) NOT NULL,
  nombre NVARCHAR(45) NOT NULL,
  CONSTRAINT UQ_Modalidad_codigo UNIQUE(codigo)
);

IF OBJECT_ID('dbo.TipoContrato','U') IS NULL
CREATE TABLE dbo.TipoContrato (
  id INT IDENTITY(1,1) PRIMARY KEY,
  codigo NVARCHAR(45) NOT NULL,
  nombre NVARCHAR(45) NOT NULL,
  CONSTRAINT UQ_TipoContrato_codigo UNIQUE(codigo)
);

IF OBJECT_ID('dbo.EstadoOferta','U') IS NULL
CREATE TABLE dbo.EstadoOferta (
  id INT IDENTITY(1,1) PRIMARY KEY,
  codigo NVARCHAR(45) NOT NULL,
  nombre NVARCHAR(45) NOT NULL,
  CONSTRAINT UQ_EstadoOferta_codigo UNIQUE(codigo)
);

IF OBJECT_ID('dbo.EstadoPostulacion','U') IS NULL
CREATE TABLE dbo.EstadoPostulacion (
  id INT IDENTITY(1,1) PRIMARY KEY,
  codigo NVARCHAR(45) NOT NULL,
  nombre NVARCHAR(45) NULL,
  CONSTRAINT UQ_EstadoPostulacion_codigo UNIQUE(codigo)
);

IF OBJECT_ID('dbo.EstadoValidacion','U') IS NULL
CREATE TABLE dbo.EstadoValidacion (
  id INT IDENTITY(1,1) PRIMARY KEY,
  codigo NVARCHAR(45) NOT NULL,
  nombre NVARCHAR(45) NOT NULL,
  CONSTRAINT UQ_EstadoValidacion_codigo UNIQUE(codigo)
);

IF OBJECT_ID('dbo.Pais','U') IS NULL
CREATE TABLE dbo.Pais (
  id INT IDENTITY(1,1) PRIMARY KEY,
  nombre NVARCHAR(45) NULL
);

IF OBJECT_ID('dbo.Provincia','U') IS NULL
CREATE TABLE dbo.Provincia (
  id INT IDENTITY(1,1) PRIMARY KEY,
  nombre NVARCHAR(45) NULL,
  idPais INT NOT NULL
);

IF OBJECT_ID('dbo.Localidad','U') IS NULL
CREATE TABLE dbo.Localidad (
  id INT IDENTITY(1,1) PRIMARY KEY,
  nombre NVARCHAR(45) NULL,
  idProvincia INT NOT NULL,
  codPostal NVARCHAR(45) NULL
);

IF OBJECT_ID('dbo.Usuario','U') IS NULL
CREATE TABLE dbo.Usuario (
  id INT IDENTITY(1,1) PRIMARY KEY,
  email NVARCHAR(320) NOT NULL,
  nombre NVARCHAR(45) NULL,
  activo BIT NULL CONSTRAINT DF_Usuario_activo DEFAULT(1),
  idRol INT NOT NULL,
  fechaAlta DATETIME2 NOT NULL,
  fechaModificacion DATETIME2 NOT NULL,
  fechaBaja DATETIME2 NULL,
  CONSTRAINT UQ_Usuario_email UNIQUE(email)
);

IF OBJECT_ID('dbo.PerfilEmpresa','U') IS NULL
CREATE TABLE dbo.PerfilEmpresa (
  id INT IDENTITY(1,1) PRIMARY KEY,
  idUsuario INT NOT NULL,
  razonSocial NVARCHAR(200) NOT NULL,
  cuit NVARCHAR(20) NULL,
  descripcion NVARCHAR(MAX) NULL,
  idEstadoValidacion INT NULL,
  fechaAlta DATETIME2 NOT NULL,
  fechaModificacion DATETIME2 NOT NULL,
  fechaBaja DATETIME2 NULL
);

IF OBJECT_ID('dbo.PerfilCandidato','U') IS NULL
CREATE TABLE dbo.PerfilCandidato (
  id INT IDENTITY(1,1) PRIMARY KEY,
  idUsuario INT NOT NULL,
  idGenero INT NULL,
  idCarrera INT NULL,
  legajo NVARCHAR(45) NULL,
  anioEgreso INT NULL,
  cv VARBINARY(MAX) NULL,
  descripcion NVARCHAR(MAX) NULL,
  fechaAlta DATETIME2 NOT NULL,
  fechaModificacion DATETIME2 NOT NULL,
  fechaBaja DATETIME2 NULL
);

IF OBJECT_ID('dbo.Oferta','U') IS NULL
CREATE TABLE dbo.Oferta (
  id INT IDENTITY(1,1) PRIMARY KEY,
  idPerfilEmpresa INT NOT NULL,
  titulo NVARCHAR(200) NOT NULL,
  descripcion NVARCHAR(MAX) NOT NULL,
  idModalidad INT NOT NULL,
  idTipoContrato INT NOT NULL,
  fechaInicio DATETIME2 NOT NULL,
  fechaFin DATETIME2 NULL,
  idLocalidad INT NULL,
  fechaAlta DATETIME2 NOT NULL,
  fechaModificacion DATETIME2 NOT NULL,
  fechaBaja DATETIME2 NULL
);

IF OBJECT_ID('dbo.Postulacion','U') IS NULL
CREATE TABLE dbo.Postulacion (
  id INT IDENTITY(1,1) PRIMARY KEY,
  idPerfilCandidato INT NOT NULL,
  idOferta INT NOT NULL,
  cartaPresentacion NVARCHAR(MAX) NULL,
  observacion NVARCHAR(MAX) NULL,
  fechaAlta DATETIME2 NOT NULL,
  fechaModificacion DATETIME2 NOT NULL,
  fechaBaja DATETIME2 NULL
);

IF OBJECT_ID('dbo.Notificacion','U') IS NULL
CREATE TABLE dbo.Notificacion (
  id INT IDENTITY(1,1) PRIMARY KEY,
  idUsuario INT NOT NULL,
  mensaje NVARCHAR(MAX) NOT NULL,
  leido BIT NOT NULL CONSTRAINT DF_Notificacion_leido DEFAULT(0),
  fechaEnvio DATETIME2 NOT NULL CONSTRAINT DF_Notif_fechaEnvio DEFAULT (SYSUTCDATETIME()),
  fechaAlta DATETIME2 NOT NULL,
  fechaModificacion DATETIME2 NOT NULL,
  fechaBaja DATETIME2 NULL,
  Asunto NVARCHAR(45) NULL,
  idPostulacion INT NOT NULL
);

IF OBJECT_ID('dbo.PostulacionHistorial','U') IS NULL
CREATE TABLE dbo.PostulacionHistorial (
  id INT IDENTITY(1,1) PRIMARY KEY,
  idPostulacion INT NOT NULL,
  idEstadoPostulacion INT NOT NULL,
  motivo NVARCHAR(MAX) NULL,
  fechaAlta DATETIME2 NOT NULL,
  fechaModificacion DATETIME2 NOT NULL,
  fechaBaja NVARCHAR(45) NULL
);

IF OBJECT_ID('dbo.Categoria','U') IS NULL
CREATE TABLE dbo.Categoria (
  id INT IDENTITY(1,1) PRIMARY KEY,
  nombre NVARCHAR(45) NOT NULL,
  codigo NVARCHAR(45) NOT NULL
);

IF OBJECT_ID('dbo.Carrera','U') IS NULL
CREATE TABLE dbo.Carrera (
  id INT IDENTITY(1,1) PRIMARY KEY,
  codigo NVARCHAR(45) NOT NULL,
  nombre NVARCHAR(45) NULL,
  CONSTRAINT UQ_Carrera_codigo UNIQUE(codigo)
);

IF OBJECT_ID('dbo.OfertaCategoria','U') IS NULL
CREATE TABLE dbo.OfertaCategoria (
  id INT IDENTITY(1,1) PRIMARY KEY,
  idCategoria INT NOT NULL,
  idOferta INT NOT NULL,
  fechaAlta DATETIME2 NOT NULL,
  fechaModificacion DATETIME2 NOT NULL,
  fechaBaja DATETIME2 NULL
);

IF OBJECT_ID('dbo.OfertaCarrera','U') IS NULL
CREATE TABLE dbo.OfertaCarrera (
  id INT IDENTITY(1,1) PRIMARY KEY,
  idOferta INT NOT NULL,
  idCarrera INT NOT NULL,
  fechaAlta DATETIME2 NOT NULL,
  fechaModificacion DATETIME2 NOT NULL,
  fechaBaja DATETIME2 NULL
);

IF OBJECT_ID('dbo.OfertaHistorial','U') IS NULL
CREATE TABLE dbo.OfertaHistorial (
  id INT IDENTITY(1,1) PRIMARY KEY,
  idOferta INT NOT NULL,
  idEstadoOferta INT NOT NULL,
  fechaAlta DATETIME2 NOT NULL,
  fechaModificacion DATETIME2 NOT NULL,
  fechaBaja DATETIME2 NULL,
  motivo NVARCHAR(45) NULL,
  cupos INT NULL
);

-- ========= Foreign Keys (sin ON DELETE CASCADE) =========

ALTER TABLE dbo.Provincia            ADD CONSTRAINT FK_Provincia_Pais
  FOREIGN KEY (idPais)            REFERENCES dbo.Pais(id);

ALTER TABLE dbo.Localidad            ADD CONSTRAINT FK_Localidad_Provincia
  FOREIGN KEY (idProvincia)       REFERENCES dbo.Provincia(id);

ALTER TABLE dbo.Usuario              ADD CONSTRAINT FK_Usuario_Rol
  FOREIGN KEY (idRol)             REFERENCES dbo.Rol(id);

ALTER TABLE dbo.PerfilEmpresa        ADD CONSTRAINT FK_PerfilEmpresa_Usuario
  FOREIGN KEY (idUsuario)         REFERENCES dbo.Usuario(id);
ALTER TABLE dbo.PerfilEmpresa        ADD CONSTRAINT FK_PerfilEmpresa_EstadoValidacion
  FOREIGN KEY (idEstadoValidacion) REFERENCES dbo.EstadoValidacion(id);

ALTER TABLE dbo.PerfilCandidato      ADD CONSTRAINT FK_PerfilCandidato_Usuario
  FOREIGN KEY (idUsuario)         REFERENCES dbo.Usuario(id);
ALTER TABLE dbo.PerfilCandidato      ADD CONSTRAINT FK_PerfilCandidato_Genero
  FOREIGN KEY (idGenero)          REFERENCES dbo.Genero(id);
ALTER TABLE dbo.PerfilCandidato      ADD CONSTRAINT FK_PerfilCandidato_Carrera
  FOREIGN KEY (idCarrera)          REFERENCES dbo.Carrera(id);

ALTER TABLE dbo.Oferta               ADD CONSTRAINT FK_Oferta_PerfilEmpresa
  FOREIGN KEY (idPerfilEmpresa)   REFERENCES dbo.PerfilEmpresa(id);
ALTER TABLE dbo.Oferta               ADD CONSTRAINT FK_Oferta_Modalidad
  FOREIGN KEY (idModalidad)       REFERENCES dbo.Modalidad(id);
ALTER TABLE dbo.Oferta               ADD CONSTRAINT FK_Oferta_TipoContrato
  FOREIGN KEY (idTipoContrato)    REFERENCES dbo.TipoContrato(id);
ALTER TABLE dbo.Oferta               ADD CONSTRAINT FK_Oferta_Localidad
  FOREIGN KEY (idLocalidad)       REFERENCES dbo.Localidad(id);

ALTER TABLE dbo.Postulacion          ADD CONSTRAINT FK_Postulacion_PerfilCandidato
  FOREIGN KEY (idPerfilCandidato) REFERENCES dbo.PerfilCandidato(id);
ALTER TABLE dbo.Postulacion          ADD CONSTRAINT FK_Postulacion_Oferta
  FOREIGN KEY (idOferta)          REFERENCES dbo.Oferta(id);

ALTER TABLE dbo.Notificacion         ADD CONSTRAINT FK_Notificacion_Usuario
  FOREIGN KEY (idUsuario)         REFERENCES dbo.Usuario(id);
ALTER TABLE dbo.Notificacion         ADD CONSTRAINT FK_Notificacion_Postulacion
  FOREIGN KEY (idPostulacion)     REFERENCES dbo.Postulacion(id);

ALTER TABLE dbo.PostulacionHistorial ADD CONSTRAINT FK_PostHist_Postulacion
  FOREIGN KEY (idPostulacion)     REFERENCES dbo.Postulacion(id);
ALTER TABLE dbo.PostulacionHistorial ADD CONSTRAINT FK_PostHist_EstadoPostulacion
  FOREIGN KEY (idEstadoPostulacion) REFERENCES dbo.EstadoPostulacion(id);

ALTER TABLE dbo.OfertaCategoria      ADD CONSTRAINT FK_OfertaCategoria_Categoria
  FOREIGN KEY (idCategoria)       REFERENCES dbo.Categoria(id);
ALTER TABLE dbo.OfertaCategoria      ADD CONSTRAINT FK_OfertaCategoria_Oferta
  FOREIGN KEY (idOferta)          REFERENCES dbo.Oferta(id);

ALTER TABLE dbo.OfertaCarrera        ADD CONSTRAINT FK_OfertaCarrera_Oferta
  FOREIGN KEY (idOferta)          REFERENCES dbo.Oferta(id);
ALTER TABLE dbo.OfertaCarrera        ADD CONSTRAINT FK_OfertaCarrera_Carrera
  FOREIGN KEY (idCarrera)         REFERENCES dbo.Carrera(id);

ALTER TABLE dbo.OfertaHistorial      ADD CONSTRAINT FK_OfertaHistorial_Oferta
  FOREIGN KEY (idOferta)          REFERENCES dbo.Oferta(id);
ALTER TABLE dbo.OfertaHistorial      ADD CONSTRAINT FK_OfertaHistorial_EstadoOferta
  FOREIGN KEY (idEstadoOferta)    REFERENCES dbo.EstadoOferta(id);
GO