USE [db-bolsa-trabajo-local];
GO
SET NOCOUNT ON;

PRINT '========================================';
PRINT '🚀 Iniciando creación de tablas y seeding';
PRINT '========================================';
GO

/* ========================================================
   DROP en orden de dependencias
   ======================================================== */
IF OBJECT_ID('dbo.CompetenciaPerfilCandidato', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.CompetenciaPerfilCandidato;
    PRINT '🗑️ Tabla CompetenciaPerfilCandidato eliminada';
END;
IF OBJECT_ID('dbo.Competencia', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Competencia;
    PRINT '🗑️ Tabla Competencia eliminada';
END;
GO

/* ========================================================
   CREATE: Competencia (usar collation de la base)
   ======================================================== */
CREATE TABLE dbo.Competencia (
    id      INT IDENTITY(1,1) PRIMARY KEY,
    nombre  NVARCHAR(100) COLLATE Modern_Spanish_CI_AS NOT NULL,
    CONSTRAINT UQ_Competencia_nombre UNIQUE(nombre)
);
PRINT '✅ Tabla Competencia creada';
GO

/* ========================================================
   CREATE: CompetenciaPerfilCandidato
   ======================================================== */
CREATE TABLE dbo.CompetenciaPerfilCandidato (
    id                  INT IDENTITY(1,1) PRIMARY KEY,
    idPerfilCandidato   INT NOT NULL,
    idCompetencia       INT NOT NULL,
    fechaAlta           DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    fechaModificacion   DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    fechaBaja           DATETIME2 NULL,
    CONSTRAINT FK_CompetenciaPerfilCandidato_PerfilCandidato 
        FOREIGN KEY (idPerfilCandidato) REFERENCES dbo.PerfilCandidato(id),
    CONSTRAINT FK_CompetenciaPerfilCandidato_Competencia 
        FOREIGN KEY (idCompetencia) REFERENCES dbo.Competencia(id)
);
PRINT '✅ Tabla CompetenciaPerfilCandidato creada';
GO

/* ========================================================
   SEED: Lista ampliada de competencias (usar misma collation)
   ======================================================== */
DECLARE @Competencias TABLE (
    nombre NVARCHAR(100) COLLATE Modern_Spanish_CI_AS
);

INSERT INTO @Competencias(nombre)
VALUES
-- 🧠 Generales / Transversales
(N'Programación'),
(N'Algoritmos y Estructuras de Datos'),
(N'Base de Datos'),
(N'Bases de Datos NoSQL'),
(N'Control de Versiones (Git)'),
(N'Testing y QA'),
(N'Automatización de Pruebas'),
(N'Metodologías Ágiles'),
(N'Gestión de Proyectos (PMI/Agile)'),
(N'Análisis de Requerimientos'),
(N'Comunicación Efectiva'),
(N'Trabajo en Equipo'),
(N'Liderazgo'),
(N'Pensamiento Crítico'),
(N'Resolución de Problemas'),
(N'Gestión del Tiempo'),
(N'Inglés Técnico'),

-- 💻 Ingeniería en Sistemas / Informática
(N'Desarrollo Web Frontend'),
(N'Desarrollo Web Backend'),
(N'Desarrollo Móvil'),
(N'Arquitectura de Software'),
(N'Microservicios'),
(N'Diseño de APIs REST'),
(N'GraphQL'),
(N'gRPC'),
(N'Cloud Computing'),
(N'Docker'),
(N'Kubernetes'),
(N'Terraform / IaC'),
(N'DevOps'),
(N'SRE / Observabilidad'),
(N'CI/CD'),
(N'Linux / Administración de Servidores'),
(N'Redes y Protocolos TCP/IP'),
(N'Ciberseguridad y Criptografía'),
(N'Ingeniería de Datos / ETL-ELT'),
(N'Data Warehousing'),
(N'Streaming de Datos (Kafka)'),
(N'Machine Learning'),
(N'Python'),
(N'Java'),
(N'C# / .NET'),
(N'JavaScript / TypeScript'),
(N'React'),
(N'Node.js'),
(N'Spring'),
(N'Django'),
(N'Flask'),

-- 🧱 Ingeniería Civil
(N'Cálculo Estructural'),
(N'Estructuras de Hormigón'),
(N'Estructuras Metálicas'),
(N'Dinámica y Sismorresistente'),
(N'Geotecnia'),
(N'Topografía'),
(N'AutoCAD'),
(N'Revit y BIM'),
(N'Gestión de Obras'),
(N'Planificación y Costos'),
(N'Infraestructura Vial'),
(N'Hidráulica Aplicada'),
(N'Gestión Ambiental en Obras'),
(N'Inspección y Control de Calidad (Civil)'),

-- ⚙️ Ingeniería Mecánica
(N'Diseño Mecánico'),
(N'CAD/CAM'),
(N'CAE / Análisis FEA'),
(N'Manufactura Asistida por Computadora'),
(N'Procesos de Manufactura'),
(N'Mecánica de Fluidos'),
(N'Termodinámica'),
(N'Transferencia de Calor'),
(N'HVAC'),
(N'Automatización y Control'),
(N'Instrumentación Industrial'),
(N'Mantenimiento Industrial'),
(N'Confiabilidad y Mantenimiento Predictivo'),
(N'MeCatronica'),
(N'CNC'),

-- 🔌 Ingeniería Eléctrica / Electrónica
(N'Diseño de Circuitos'),
(N'Electrónica Analógica'),
(N'Electrónica Digital'),
(N'Microcontroladores'),
(N'FPGA / HDL'),
(N'Electrónica de Potencia'),
(N'Sistemas de Potencia'),
(N'Protecciones Eléctricas'),
(N'Control Automático'),
(N'Robótica'),
(N'IoT (Internet de las Cosas)'),
(N'Instrumentación y Medición'),
(N'Comunicaciones / Señales'),
(N'PLC y SCADA'),
(N'Energías Renovables (Eléctrica)'),

-- ⚗️ Ingeniería Química
(N'Diseño de Procesos'),
(N'Reactores Químicos'),
(N'Operaciones Unitarias'),
(N'Termodinámica Química'),
(N'Transferencia de Calor y Masa'),
(N'Simulación de Procesos'),
(N'Optimización de Procesos'),
(N'Control de Calidad (Químico)'),
(N'Gestión Ambiental (Químico)'),
(N'Seguridad de Procesos / HAZOP'),
(N'Catálisis'),
(N'Balance de Materia y Energía'),
(N'Química Industrial');

-- Insertar solo si no existen (misma collation en la comparación)
INSERT INTO dbo.Competencia(nombre)
SELECT C.nombre
FROM @Competencias AS C
WHERE NOT EXISTS (
    SELECT 1
    FROM dbo.Competencia AS T
    WHERE T.nombre = C.nombre  -- ambas Modern_Spanish_CI_AS ⇒ sin conflicto
);

PRINT '✅ Competencias cargadas correctamente';

PRINT '';
PRINT '========================================';
PRINT '🎯 Script completado exitosamente';
PRINT '========================================';
GO