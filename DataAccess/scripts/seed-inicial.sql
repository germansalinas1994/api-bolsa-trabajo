USE [db-bolsa-trabajo-local];
SET NOCOUNT ON;

------------------------------------------------------------
-- 1) Catálogos / Enums
------------------------------------------------------------
-- Rol
IF NOT EXISTS (SELECT 1 FROM dbo.Rol WHERE codigo='AdminUtn')
INSERT dbo.Rol(codigo,nombre) VALUES ('AdminUtn','Administrador UTN');
IF NOT EXISTS (SELECT 1 FROM dbo.Rol WHERE codigo='Empresa')
INSERT dbo.Rol(codigo,nombre) VALUES ('Empresa','Empresa');
IF NOT EXISTS (SELECT 1 FROM dbo.Rol WHERE codigo='Candidato')
INSERT dbo.Rol(codigo,nombre) VALUES ('Candidato','Candidato');

-- Genero
IF NOT EXISTS (SELECT 1 FROM dbo.Genero WHERE codigo='Masculino')
INSERT dbo.Genero(codigo,nombre) VALUES ('Masculino','Masculino');
IF NOT EXISTS (SELECT 1 FROM dbo.Genero WHERE codigo='Femenino')
INSERT dbo.Genero(codigo,nombre) VALUES ('Femenino','Femenino');
IF NOT EXISTS (SELECT 1 FROM dbo.Genero WHERE codigo='Otro')
INSERT dbo.Genero(codigo,nombre) VALUES ('Otro','Otro');

-- EstadoValidacion
IF NOT EXISTS (SELECT 1 FROM dbo.EstadoValidacion WHERE codigo='Aprobada')
INSERT dbo.EstadoValidacion(codigo,nombre) VALUES ('Aprobada','Aprobada');
IF NOT EXISTS (SELECT 1 FROM dbo.EstadoValidacion WHERE codigo='Pendiente')
INSERT dbo.EstadoValidacion(codigo,nombre) VALUES ('Pendiente','Pendiente');
IF NOT EXISTS (SELECT 1 FROM dbo.EstadoValidacion WHERE codigo='Rechazada')
INSERT dbo.EstadoValidacion(codigo,nombre) VALUES ('Rechazada','Rechazada');
INSERT dbo.EstadoValidacion(codigo,nombre) VALUES ('Iniciada','Iniciada');

-- EstadoPostulacion
IF NOT EXISTS (SELECT 1 FROM dbo.EstadoPostulacion WHERE codigo='Aprobada')
INSERT dbo.EstadoPostulacion(codigo,nombre) VALUES ('Aprobada','Aprobada');
IF NOT EXISTS (SELECT 1 FROM dbo.EstadoPostulacion WHERE codigo='Rechazada')
INSERT dbo.EstadoPostulacion(codigo,nombre) VALUES ('Rechazada','Rechazada');
IF NOT EXISTS (SELECT 1 FROM dbo.EstadoPostulacion WHERE codigo='EnRevision')
INSERT dbo.EstadoPostulacion(codigo,nombre) VALUES ('EnRevision','En revisión');
IF NOT EXISTS (SELECT 1 FROM dbo.EstadoPostulacion WHERE codigo='Iniciada')
INSERT dbo.EstadoPostulacion(codigo,nombre) VALUES ('Iniciada','Iniciada');

-- TipoContrato
IF NOT EXISTS (SELECT 1 FROM dbo.TipoContrato WHERE codigo='FullTime')
INSERT dbo.TipoContrato(codigo,nombre) VALUES ('FullTime','Full-Time');
IF NOT EXISTS (SELECT 1 FROM dbo.TipoContrato WHERE codigo='PartTime')
INSERT dbo.TipoContrato(codigo,nombre) VALUES ('PartTime','Part-Time');
IF NOT EXISTS (SELECT 1 FROM dbo.TipoContrato WHERE codigo='Temporal')
INSERT dbo.TipoContrato(codigo,nombre) VALUES ('Temporal','Temporal');

-- Modalidad
IF NOT EXISTS (SELECT 1 FROM dbo.Modalidad WHERE codigo='Hibrido')
INSERT dbo.Modalidad(codigo,nombre) VALUES ('Hibrido','Híbrido');
IF NOT EXISTS (SELECT 1 FROM dbo.Modalidad WHERE codigo='Remoto')
INSERT dbo.Modalidad(codigo,nombre) VALUES ('Remoto','Remoto');
IF NOT EXISTS (SELECT 1 FROM dbo.Modalidad WHERE codigo='Presencial')
INSERT dbo.Modalidad(codigo,nombre) VALUES ('Presencial','Presencial');

-- EstadoOferta
IF NOT EXISTS (SELECT 1 FROM dbo.EstadoOferta WHERE codigo='Publicada')
INSERT dbo.EstadoOferta(codigo,nombre) VALUES ('Publicada','Publicada');
IF NOT EXISTS (SELECT 1 FROM dbo.EstadoOferta WHERE codigo='Cerrada')
INSERT dbo.EstadoOferta(codigo,nombre) VALUES ('Cerrada','Cerrada');
IF NOT EXISTS (SELECT 1 FROM dbo.EstadoOferta WHERE codigo='Pendiente')
INSERT dbo.EstadoOferta(codigo,nombre) VALUES ('Pendiente','Pendiente');

------------------------------------------------------------
-- 2) País / Provincia / Localidad mínimos
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM dbo.Pais WHERE nombre='Argentina')
INSERT dbo.Pais(nombre) VALUES ('Argentina');

DECLARE @idPaisAR INT = (SELECT id FROM dbo.Pais WHERE nombre='Argentina');

IF NOT EXISTS (SELECT 1 FROM dbo.Provincia WHERE nombre='Buenos Aires' AND idPais=@idPaisAR)
INSERT dbo.Provincia(nombre,idPais) VALUES ('Buenos Aires', @idPaisAR);

DECLARE @idProvBA INT = (SELECT id FROM dbo.Provincia WHERE nombre='Buenos Aires' AND idPais=@idPaisAR);

IF NOT EXISTS (SELECT 1 FROM dbo.Localidad WHERE nombre='La Plata' AND idProvincia=@idProvBA)
INSERT dbo.Localidad(nombre,idProvincia,codPostal) VALUES ('La Plata', @idProvBA, '1900');

------------------------------------------------------------
-- 3) Usuarios (admin, empresa, candidato)
------------------------------------------------------------
DECLARE @now DATETIME2 = SYSUTCDATETIME();

-- Rol IDs
DECLARE @idRolAdmin   INT = (SELECT id FROM dbo.Rol WHERE codigo='AdminUtn');
DECLARE @idRolEmp     INT = (SELECT id FROM dbo.Rol WHERE codigo='Empresa');
DECLARE @idRolCand    INT = (SELECT id FROM dbo.Rol WHERE codigo='Candidato');

-- Admin
IF NOT EXISTS (SELECT 1 FROM dbo.Usuario WHERE email='admin@utnfrlp.edu.ar')
INSERT dbo.Usuario(email,nombre,activo,idRol,fechaAlta,fechaModificacion,fechaBaja)
VALUES ('admin@utnfrlp.edu.ar','Admin UTN',1,@idRolAdmin,@now,@now,NULL);

-- Empresa user
IF NOT EXISTS (SELECT 1 FROM dbo.Usuario WHERE email='empresa@acme.com')
INSERT dbo.Usuario(email,nombre,activo,idRol,fechaAlta,fechaModificacion,fechaBaja)
VALUES ('empresa@acme.com','ACME S.A.',1,@idRolEmp,@now,@now,NULL);

-- Candidato user
IF NOT EXISTS (SELECT 1 FROM dbo.Usuario WHERE email='candidato@alumno.utn.edu.ar')
INSERT dbo.Usuario(email,nombre,activo,idRol,fechaAlta,fechaModificacion,fechaBaja)
VALUES ('candidato@alumno.utn.edu.ar','Juan Pérez',1,@idRolCand,@now,@now,NULL);

DECLARE @idUsuarioEmpresa  INT = (SELECT id FROM dbo.Usuario WHERE email='empresa@acme.com');
DECLARE @idUsuarioCandidato INT = (SELECT id FROM dbo.Usuario WHERE email='candidato@alumno.utn.edu.ar');

------------------------------------------------------------
-- 4) Perfiles
------------------------------------------------------------
DECLARE @idEstadoValPend  INT = (SELECT id FROM dbo.EstadoValidacion WHERE codigo='Pendiente');
DECLARE @idGeneroMasc     INT = (SELECT id FROM dbo.Genero WHERE codigo='Masculino');

IF NOT EXISTS (SELECT 1 FROM dbo.PerfilEmpresa WHERE idUsuario=@idUsuarioEmpresa)
INSERT dbo.PerfilEmpresa(idUsuario,razonSocial,cuit,descripcion,idEstadoValidacion,fechaAlta,fechaModificacion,fechaBaja)
VALUES (@idUsuarioEmpresa,'ACME S.A.','30-12345678-9','Empresa de ejemplo para seeds',@idEstadoValPend,@now,@now,NULL);

IF NOT EXISTS (SELECT 1 FROM dbo.PerfilCandidato WHERE idUsuario=@idUsuarioCandidato)
INSERT dbo.PerfilCandidato(idUsuario,idGenero,legajo,anioEgreso,cv,descripcion,fechaAlta,fechaModificacion,fechaBaja)
VALUES (@idUsuarioCandidato,@idGeneroMasc,'12345',2024,NULL,'Candidato seed para pruebas',@now,@now,NULL);

DECLARE @idPerfilEmp INT = (SELECT id FROM dbo.PerfilEmpresa   WHERE idUsuario=@idUsuarioEmpresa);
DECLARE @idPerfilCan INT = (SELECT id FROM dbo.PerfilCandidato WHERE idUsuario=@idUsuarioCandidato);

------------------------------------------------------------
-- 5) Catálogos carrera / categoría (mínimos)
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM dbo.Carrera WHERE codigo='ISI')
INSERT dbo.Carrera(codigo,nombre) VALUES ('ISI','Ingeniería en Sistemas de Información');
IF NOT EXISTS (SELECT 1 FROM dbo.Carrera WHERE codigo='II')
INSERT dbo.Carrera(codigo,nombre) VALUES ('II','Ingeniería Industrial');

IF NOT EXISTS (SELECT 1 FROM dbo.Categoria WHERE codigo='BACKEND')
INSERT dbo.Categoria(nombre,codigo) VALUES ('Desarrollo Backend','BACKEND');
IF NOT EXISTS (SELECT 1 FROM dbo.Categoria WHERE codigo='DATA')
INSERT dbo.Categoria(nombre,codigo) VALUES ('Datos/Analytics','DATA');

DECLARE @idCarreraISI INT = (SELECT id FROM dbo.Carrera  WHERE codigo='ISI');
DECLARE @idCategoriaBE INT = (SELECT id FROM dbo.Categoria WHERE codigo='BACKEND');

------------------------------------------------------------
-- 6) Oferta de ejemplo
------------------------------------------------------------
DECLARE @idModalidadH  INT = (SELECT id FROM dbo.Modalidad     WHERE codigo='Hibrido');
DECLARE @idContratoFT  INT = (SELECT id FROM dbo.TipoContrato  WHERE codigo='FullTime');
DECLARE @idLocalidadLP INT = (SELECT id FROM dbo.Localidad     WHERE nombre='La Plata' AND idProvincia=@idProvBA);

IF NOT EXISTS (SELECT 1 FROM dbo.Oferta WHERE titulo='Desarrollador .NET Jr')
INSERT dbo.Oferta
(idPerfilEmpresa,titulo,descripcion,idModalidad,idTipoContrato,fechaInicio,fechaFin,idLocalidad,fechaAlta,fechaModificacion,fechaBaja)
VALUES
(@idPerfilEmp,'Desarrollador .NET Jr',
 'Participación en desarrollo de plataforma UTN (seed).',
 @idModalidadH,@idContratoFT, DATEADD(DAY,1,@now), NULL, @idLocalidadLP, @now, @now, NULL);

DECLARE @idOferta INT = (SELECT TOP 1 id FROM dbo.Oferta WHERE idPerfilEmpresa=@idPerfilEmp ORDER BY id DESC);

-- Cruces
IF NOT EXISTS (SELECT 1 FROM dbo.OfertaCategoria WHERE idOferta=@idOferta AND idCategoria=@idCategoriaBE)
INSERT dbo.OfertaCategoria(idCategoria,idOferta,fechaAlta,fechaModificacion,fechaBaja)
VALUES (@idCategoriaBE,@idOferta,@now,@now,NULL);

IF NOT EXISTS (SELECT 1 FROM dbo.OfertaCarrera WHERE idOferta=@idOferta AND idCarrera=@idCarreraISI)
INSERT dbo.OfertaCarrera(idOferta,idCarrera,fechaAlta,fechaModificacion,fechaBaja)
VALUES (@idOferta,@idCarreraISI,@now,@now,NULL);

-- Historial Oferta (Pendiente -> Publicada)
DECLARE @idEstOferPend INT = (SELECT id FROM dbo.EstadoOferta WHERE codigo='Pendiente');
DECLARE @idEstOferPubl INT = (SELECT id FROM dbo.EstadoOferta WHERE codigo='Publicada');

IF NOT EXISTS (SELECT 1 FROM dbo.OfertaHistorial WHERE idOferta=@idOferta AND idEstadoOferta=@idEstOferPend)
INSERT dbo.OfertaHistorial(idOferta,idEstadoOferta,fechaAlta,fechaModificacion,fechaBaja,motivo,cupos)
VALUES (@idOferta,@idEstOferPend,@now,@now,NULL,'Creación de oferta',1);

IF NOT EXISTS (SELECT 1 FROM dbo.OfertaHistorial WHERE idOferta=@idOferta AND idEstadoOferta=@idEstOferPubl)
INSERT dbo.OfertaHistorial(idOferta,idEstadoOferta,fechaAlta,fechaModificacion,fechaBaja,motivo,cupos)
VALUES (@idOferta,@idEstOferPubl,@now,@now,NULL,'Publicación inicial',1);

------------------------------------------------------------
-- 7) Postulación + Historial + Notificación
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM dbo.Postulacion WHERE idPerfilCandidato=@idPerfilCan AND idOferta=@idOferta)
INSERT dbo.Postulacion(idPerfilCandidato,idOferta,cartaPresentacion,observacion,fechaAlta,fechaModificacion,fechaBaja)
VALUES (@idPerfilCan,@idOferta,N'Estimados, me postulo a la posición .NET Jr.',NULL,@now,@now,NULL);

DECLARE @idPostulacion INT = (SELECT TOP 1 id FROM dbo.Postulacion WHERE idPerfilCandidato=@idPerfilCan AND idOferta=@idOferta ORDER BY id DESC);
DECLARE @idEstPostRev INT = (SELECT id FROM dbo.EstadoPostulacion WHERE codigo='EnRevision');

IF NOT EXISTS (SELECT 1 FROM dbo.PostulacionHistorial WHERE idPostulacion=@idPostulacion AND idEstadoPostulacion=@idEstPostRev)
INSERT dbo.PostulacionHistorial(idPostulacion,idEstadoPostulacion,motivo,fechaAlta,fechaModificacion,fechaBaja)
VALUES (@idPostulacion,@idEstPostRev,N'Postulación recibida',@now,@now,NULL);

-- Notificación al candidato
IF NOT EXISTS (SELECT 1 FROM dbo.Notificacion WHERE idPostulacion=@idPostulacion AND idUsuario=@idUsuarioCandidato)
INSERT dbo.Notificacion(idUsuario,mensaje,leido,fechaEnvio,fechaAlta,fechaModificacion,fechaBaja,asunto,idPostulacion)
VALUES (@idUsuarioCandidato,N'Tu postulación fue recibida',0,@now,@now,@now,NULL,N'Postulación recibida',@idPostulacion);

PRINT 'Seed ejecutado OK.';
