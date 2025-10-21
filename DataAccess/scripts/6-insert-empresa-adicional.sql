USE [db-bolsa-trabajo-local];
SET NOCOUNT ON;

------------------------------------------------------------
-- Script para agregar una nueva empresa con 3 ofertas laborales
------------------------------------------------------------
DECLARE @now DATETIME2 = SYSUTCDATETIME();

-- Obtener IDs de roles y catálogos
DECLARE @idRolEmp INT = (SELECT id FROM dbo.Rol WHERE codigo='Empresa');
DECLARE @idEstadoValAprobada INT = (SELECT id FROM dbo.EstadoValidacion WHERE codigo='Aprobada');

-- Obtener IDs de carreras
DECLARE @idCarreraISI INT = (SELECT id FROM dbo.Carrera WHERE codigo='ISI');
DECLARE @idCarreraII INT = (SELECT id FROM dbo.Carrera WHERE codigo='II');

-- Obtener IDs de categorías
DECLARE @idCategoriaBE INT = (SELECT id FROM dbo.Categoria WHERE codigo='BACKEND');
DECLARE @idCategoriaDATA INT = (SELECT id FROM dbo.Categoria WHERE codigo='DATA');

-- Obtener IDs de modalidades
DECLARE @idModalidadH INT = (SELECT id FROM dbo.Modalidad WHERE codigo='Hibrido');
DECLARE @idModalidadR INT = (SELECT id FROM dbo.Modalidad WHERE codigo='Remoto');
DECLARE @idModalidadP INT = (SELECT id FROM dbo.Modalidad WHERE codigo='Presencial');

-- Obtener IDs de tipos de contrato
DECLARE @idContratoFT INT = (SELECT id FROM dbo.TipoContrato WHERE codigo='FullTime');
DECLARE @idContratoPT INT = (SELECT id FROM dbo.TipoContrato WHERE codigo='PartTime');
DECLARE @idContratoTemp INT = (SELECT id FROM dbo.TipoContrato WHERE codigo='Temporal');

-- Obtener ID de localidad
DECLARE @idProvBA INT = (SELECT id FROM dbo.Provincia WHERE nombre='Buenos Aires');
DECLARE @idLocalidadLP INT = (SELECT id FROM dbo.Localidad WHERE nombre='La Plata' AND idProvincia=@idProvBA);

-- Obtener IDs de estados de oferta
DECLARE @idEstOferPend INT = (SELECT id FROM dbo.EstadoOferta WHERE codigo='Pendiente');
DECLARE @idEstOferPubl INT = (SELECT id FROM dbo.EstadoOferta WHERE codigo='Publicada');

------------------------------------------------------------
-- 1) Crear Usuario Empresa
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM dbo.Usuario WHERE email='empresa@techsolutions.com')
BEGIN
    INSERT dbo.Usuario(email, nombre, activo, idRol, fechaAlta, fechaModificacion, fechaBaja)
    VALUES ('empresa@techsolutions.com', 'Tech Solutions SRL', 1, @idRolEmp, @now, @now, NULL);
    PRINT '✓ Usuario Tech Solutions creado';
END

DECLARE @idUsuarioTechSol INT = (SELECT id FROM dbo.Usuario WHERE email='empresa@techsolutions.com');

------------------------------------------------------------
-- 2) Crear Perfil Empresa
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM dbo.PerfilEmpresa WHERE idUsuario=@idUsuarioTechSol)
BEGIN
    INSERT dbo.PerfilEmpresa(idUsuario, razonSocial, cuit, descripcion, idEstadoValidacion, fechaAlta, fechaModificacion, fechaBaja)
    VALUES (
        @idUsuarioTechSol,
        'Tech Solutions SRL',
        '30-98765432-1',
        'Somos una empresa líder en desarrollo de software y soluciones tecnológicas innovadoras. Contamos con más de 10 años de experiencia en el mercado y un equipo multidisciplinario de profesionales.',
        @idEstadoValAprobada,
        @now,
        @now,
        NULL
    );
    PRINT '✓ Perfil de empresa Tech Solutions creado';
END

DECLARE @idPerfilTechSol INT = (SELECT id FROM dbo.PerfilEmpresa WHERE idUsuario=@idUsuarioTechSol);

------------------------------------------------------------
-- 3) Crear Oferta 1: Desarrollador Backend Node.js
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM dbo.Oferta WHERE titulo='Desarrollador Backend Node.js' AND idPerfilEmpresa=@idPerfilTechSol)
BEGIN
    INSERT dbo.Oferta(
        idPerfilEmpresa, titulo, descripcion, idModalidad, idTipoContrato, 
        fechaInicio, fechaFin, idLocalidad, fechaAlta, fechaModificacion, fechaBaja
    )
    VALUES (
        @idPerfilTechSol,
        'Desarrollador Backend Node.js',
        'Buscamos desarrollador con experiencia en Node.js, Express y MongoDB. Se requiere conocimientos en arquitectura de microservicios, Docker y metodologías ágiles. Ambiente colaborativo y proyectos desafiantes.',
        @idModalidadH,  -- Híbrido
        @idContratoFT,  -- Full-Time
        DATEADD(DAY, 1, @now),
        DATEADD(MONTH, 6, @now),
        @idLocalidadLP,
        @now,
        @now,
        NULL
    );
    PRINT '✓ Oferta 1: Desarrollador Backend Node.js creada';
END

DECLARE @idOferta1 INT = (SELECT id FROM dbo.Oferta WHERE titulo='Desarrollador Backend Node.js' AND idPerfilEmpresa=@idPerfilTechSol);

-- Asociar con categoría y carrera
IF @idOferta1 IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.OfertaCategoria WHERE idOferta=@idOferta1 AND idCategoria=@idCategoriaBE)
    BEGIN
        INSERT dbo.OfertaCategoria(idCategoria, idOferta, fechaAlta, fechaModificacion, fechaBaja)
        VALUES (@idCategoriaBE, @idOferta1, @now, @now, NULL);
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.OfertaCarrera WHERE idOferta=@idOferta1 AND idCarrera=@idCarreraISI)
    BEGIN
        INSERT dbo.OfertaCarrera(idOferta, idCarrera, fechaAlta, fechaModificacion, fechaBaja)
        VALUES (@idOferta1, @idCarreraISI, @now, @now, NULL);
    END

    -- Historial de oferta
    IF NOT EXISTS (SELECT 1 FROM dbo.OfertaHistorial WHERE idOferta=@idOferta1 AND idEstadoOferta=@idEstOferPend)
    BEGIN
        INSERT dbo.OfertaHistorial(idOferta, idEstadoOferta, fechaAlta, fechaModificacion, fechaBaja, motivo, cupos)
        VALUES (@idOferta1, @idEstOferPend, @now, @now, NULL, 'Creación de oferta', 2);
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.OfertaHistorial WHERE idOferta=@idOferta1 AND idEstadoOferta=@idEstOferPubl)
    BEGIN
        INSERT dbo.OfertaHistorial(idOferta, idEstadoOferta, fechaAlta, fechaModificacion, fechaBaja, motivo, cupos)
        VALUES (@idOferta1, @idEstOferPubl, @now, @now, NULL, 'Publicación inicial', 2);
    END
END

------------------------------------------------------------
-- 4) Crear Oferta 2: Analista de Datos
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM dbo.Oferta WHERE titulo='Analista de Datos' AND idPerfilEmpresa=@idPerfilTechSol)
BEGIN
    INSERT dbo.Oferta(
        idPerfilEmpresa, titulo, descripcion, idModalidad, idTipoContrato, 
        fechaInicio, fechaFin, idLocalidad, fechaAlta, fechaModificacion, fechaBaja
    )
    VALUES (
        @idPerfilTechSol,
        'Analista de Datos',
        'Incorporamos analista de datos con conocimientos en SQL, Python y Power BI. Trabajarás en proyectos de análisis predictivo y visualización de datos. Ideal para egresados recientes o estudiantes avanzados.',
        @idModalidadR,  -- Remoto
        @idContratoPT,  -- Part-Time
        DATEADD(DAY, 5, @now),
        DATEADD(MONTH, 3, @now),
        @idLocalidadLP,
        @now,
        @now,
        NULL
    );
    PRINT '✓ Oferta 2: Analista de Datos creada';
END

DECLARE @idOferta2 INT = (SELECT id FROM dbo.Oferta WHERE titulo='Analista de Datos' AND idPerfilEmpresa=@idPerfilTechSol);

-- Asociar con categoría y carrera
IF @idOferta2 IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.OfertaCategoria WHERE idOferta=@idOferta2 AND idCategoria=@idCategoriaDATA)
    BEGIN
        INSERT dbo.OfertaCategoria(idCategoria, idOferta, fechaAlta, fechaModificacion, fechaBaja)
        VALUES (@idCategoriaDATA, @idOferta2, @now, @now, NULL);
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.OfertaCarrera WHERE idOferta=@idOferta2 AND idCarrera=@idCarreraISI)
    BEGIN
        INSERT dbo.OfertaCarrera(idOferta, idCarrera, fechaAlta, fechaModificacion, fechaBaja)
        VALUES (@idOferta2, @idCarreraISI, @now, @now, NULL);
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.OfertaCarrera WHERE idOferta=@idOferta2 AND idCarrera=@idCarreraII)
    BEGIN
        INSERT dbo.OfertaCarrera(idOferta, idCarrera, fechaAlta, fechaModificacion, fechaBaja)
        VALUES (@idOferta2, @idCarreraII, @now, @now, NULL);
    END

    -- Historial de oferta
    IF NOT EXISTS (SELECT 1 FROM dbo.OfertaHistorial WHERE idOferta=@idOferta2 AND idEstadoOferta=@idEstOferPend)
    BEGIN
        INSERT dbo.OfertaHistorial(idOferta, idEstadoOferta, fechaAlta, fechaModificacion, fechaBaja, motivo, cupos)
        VALUES (@idOferta2, @idEstOferPend, @now, @now, NULL, 'Creación de oferta', 1);
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.OfertaHistorial WHERE idOferta=@idOferta2 AND idEstadoOferta=@idEstOferPubl)
    BEGIN
        INSERT dbo.OfertaHistorial(idOferta, idEstadoOferta, fechaAlta, fechaModificacion, fechaBaja, motivo, cupos)
        VALUES (@idOferta2, @idEstOferPubl, @now, @now, NULL, 'Publicación inicial', 1);
    END
END

------------------------------------------------------------
-- 5) Crear Oferta 3: Pasantía en Desarrollo Full Stack
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM dbo.Oferta WHERE titulo='Pasantía en Desarrollo Full Stack' AND idPerfilEmpresa=@idPerfilTechSol)
BEGIN
    INSERT dbo.Oferta(
        idPerfilEmpresa, titulo, descripcion, idModalidad, idTipoContrato, 
        fechaInicio, fechaFin, idLocalidad, fechaAlta, fechaModificacion, fechaBaja
    )
    VALUES (
        @idPerfilTechSol,
        'Pasantía en Desarrollo Full Stack',
        'Ofrecemos pasantía para estudiantes de últimos años interesados en desarrollo web. Aprenderás React, Node.js y PostgreSQL trabajando en proyectos reales. Incluye capacitación y mentoría de desarrolladores senior.',
        @idModalidadP,  -- Presencial
        @idContratoTemp,  -- Temporal
        DATEADD(DAY, 10, @now),
        DATEADD(MONTH, 4, @now),
        @idLocalidadLP,
        @now,
        @now,
        NULL
    );
    PRINT '✓ Oferta 3: Pasantía en Desarrollo Full Stack creada';
END

DECLARE @idOferta3 INT = (SELECT id FROM dbo.Oferta WHERE titulo='Pasantía en Desarrollo Full Stack' AND idPerfilEmpresa=@idPerfilTechSol);

-- Asociar con categoría y carrera
IF @idOferta3 IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.OfertaCategoria WHERE idOferta=@idOferta3 AND idCategoria=@idCategoriaBE)
    BEGIN
        INSERT dbo.OfertaCategoria(idCategoria, idOferta, fechaAlta, fechaModificacion, fechaBaja)
        VALUES (@idCategoriaBE, @idOferta3, @now, @now, NULL);
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.OfertaCarrera WHERE idOferta=@idOferta3 AND idCarrera=@idCarreraISI)
    BEGIN
        INSERT dbo.OfertaCarrera(idOferta, idCarrera, fechaAlta, fechaModificacion, fechaBaja)
        VALUES (@idOferta3, @idCarreraISI, @now, @now, NULL);
    END

    -- Historial de oferta
    IF NOT EXISTS (SELECT 1 FROM dbo.OfertaHistorial WHERE idOferta=@idOferta3 AND idEstadoOferta=@idEstOferPend)
    BEGIN
        INSERT dbo.OfertaHistorial(idOferta, idEstadoOferta, fechaAlta, fechaModificacion, fechaBaja, motivo, cupos)
        VALUES (@idOferta3, @idEstOferPend, @now, @now, NULL, 'Creación de oferta', 3);
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.OfertaHistorial WHERE idOferta=@idOferta3 AND idEstadoOferta=@idEstOferPubl)
    BEGIN
        INSERT dbo.OfertaHistorial(idOferta, idEstadoOferta, fechaAlta, fechaModificacion, fechaBaja, motivo, cupos)
        VALUES (@idOferta3, @idEstOferPubl, @now, @now, NULL, 'Publicación inicial', 3);
    END
END

------------------------------------------------------------
-- Resumen
------------------------------------------------------------
PRINT '';
PRINT '═══════════════════════════════════════════════════';
PRINT '✓ Script ejecutado exitosamente';
PRINT '═══════════════════════════════════════════════════';
PRINT 'Empresa creada: Tech Solutions SRL';
PRINT 'Email: empresa@techsolutions.com';
PRINT 'CUIT: 30-98765432-1';
PRINT 'Estado: Aprobada';
PRINT '';
PRINT 'Ofertas creadas:';
PRINT '  1. Desarrollador Backend Node.js (Híbrido, Full-Time)';
PRINT '  2. Analista de Datos (Remoto, Part-Time)';
PRINT '  3. Pasantía en Desarrollo Full Stack (Presencial, Temporal)';
PRINT '═══════════════════════════════════════════════════';

