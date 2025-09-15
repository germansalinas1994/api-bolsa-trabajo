USE [db-bolsa-trabajo-local];
SET NOCOUNT ON;

------------------------------------------------------------
-- Script para insertar un candidato adicional con carrera
------------------------------------------------------------

DECLARE @now DATETIME2 = SYSUTCDATETIME();

-- Obtener IDs necesarios
DECLARE @idRolCand    INT = (SELECT id FROM dbo.Rol WHERE codigo='Candidato');
DECLARE @idGeneroFem  INT = (SELECT id FROM dbo.Genero WHERE codigo='Femenino');
DECLARE @idCarreraII  INT = (SELECT id FROM dbo.Carrera WHERE codigo='II');

-- Verificar que existan los datos necesarios
IF @idRolCand IS NULL
BEGIN
    PRINT 'ERROR: No existe el rol Candidato';
    RETURN;
END

IF @idGeneroFem IS NULL
BEGIN
    PRINT 'ERROR: No existe el género Femenino';
    RETURN;
END

IF @idCarreraII IS NULL
BEGIN
    PRINT 'ERROR: No existe la carrera II (Ingeniería Industrial)';
    RETURN;
END

-- Insertar nuevo usuario candidato
IF NOT EXISTS (SELECT 1 FROM dbo.Usuario WHERE email='maria.garcia@alumno.utn.edu.ar')
BEGIN
    INSERT dbo.Usuario(email,nombre,activo,idRol,fechaAlta,fechaModificacion,fechaBaja)
    VALUES ('maria.garcia@alumno.utn.edu.ar','María García',1,@idRolCand,@now,@now,NULL);
    
    PRINT 'Usuario María García creado correctamente';
END
ELSE
BEGIN
    PRINT 'El usuario María García ya existe';
END

-- Obtener ID del usuario recién creado
DECLARE @idUsuarioNuevo INT = (SELECT id FROM dbo.Usuario WHERE email='maria.garcia@alumno.utn.edu.ar');

-- Insertar perfil de candidato
IF NOT EXISTS (SELECT 1 FROM dbo.PerfilCandidato WHERE idUsuario=@idUsuarioNuevo)
BEGIN
    INSERT dbo.PerfilCandidato(idUsuario,idGenero,idCarrera,legajo,anioEgreso,cv,descripcion,fechaAlta,fechaModificacion,fechaBaja)
    VALUES (@idUsuarioNuevo,@idGeneroFem,@idCarreraII,'54321',2025,NULL,'Estudiante de Ingeniería Industrial con interés en procesos de manufactura y optimización',@now,@now,NULL);
    
    PRINT 'Perfil de candidato María García creado correctamente';
    PRINT 'Carrera asignada: Ingeniería Industrial';
    PRINT 'Legajo: 54321';
    PRINT 'Año de egreso: 2025';
END
ELSE
BEGIN
    PRINT 'El perfil de María García ya existe';
END

-- Mostrar información del candidato creado
SELECT 
    u.id as UsuarioId,
    u.email,
    u.nombre,
    pc.legajo,
    pc.anioEgreso,
    c.nombre as CarreraNombre,
    c.codigo as CarreraCodigo,
    g.nombre as GeneroNombre,
    pc.descripcion
FROM dbo.Usuario u
INNER JOIN dbo.PerfilCandidato pc ON u.id = pc.idUsuario
LEFT JOIN dbo.Carrera c ON pc.idCarrera = c.id
LEFT JOIN dbo.Genero g ON pc.idGenero = g.id
WHERE u.email = 'maria.garcia@alumno.utn.edu.ar';

PRINT 'Script ejecutado correctamente.';
