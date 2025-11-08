namespace BussinessLogic.DTO
{
    /// <summary>
    /// DTO principal del dashboard del administrador.
    /// Contiene métricas generales, series temporales y desgloses por carrera, género, modalidad, etc.
    /// </summary>
    public class DashboardAdminDTO
    {
        //  Métricas resumidas (tarjetas principales)
        public MetricasDTO Metricas { get; set; }

        //  Serie temporal de postulaciones por mes
        public List<PostulacionesMesDTO> PostulacionesPorMes { get; set; }

        //  Distribución de candidatos y ofertas por carrera
        public Dictionary<string, int> CantidadCandidatosPorCarrera { get; set; }
        public Dictionary<string, int> CantidadOfertasPorCarrera { get; set; }

        //  Distribución de candidatos por género
        public Dictionary<string, int> CandidatosPorGenero { get; set; }

        //  Distribución de empresas según verificación
        public Dictionary<string, int> EmpresasPorVerificacion { get; set; }

        //  Distribución de ofertas por tipo de contrato
        public Dictionary<string, int> OfertasPorTipoContrato { get; set; }

        //  Distribución de ofertas por modalidad (Remoto, Presencial, Híbrido)
        public Dictionary<string, int> OfertasPorModalidad { get; set; }

        //  Distribución de ofertas por ubicación / sede
        public Dictionary<string, int> OfertasPorLocalidad { get; set; }
    }

    /// <summary>
    /// Métricas de cabecera (tarjetas principales del dashboard)
    /// </summary>
    public class MetricasDTO
    {
        // 🏢 Empresas
        public int EmpresasRegistradas { get; set; }
        public int EmpresasVerificadas { get; set; }
        public int EmpresasNoVerificadas { get; set; }

        // 👨‍🎓 Candidatos
        public int CandidatosRegistrados { get; set; }          // Todos los perfiles activos
        public int CandidatosConPostulaciones { get; set; }     // Distintos candidatos que realizaron al menos una postulación

        // 📄 Ofertas
        public int OfertasPublicadas { get; set; }
        public int OfertasActivas { get; set; }

        // 💌 Postulaciones
        public int PostulacionesRecibidas { get; set; }

        // 🎓 Métricas específicas por carrera (si querés mantenerlas)
        public int OfertasSistemas { get; set; }
        public int OfertasIndustrial { get; set; }
        public int OfertasElectrica { get; set; }
        public int OfertasCivil { get; set; }
        public int OfertasQuimica { get; set; }
        public int OfertasMecanica { get; set; }
    }

    /// <summary>
    /// DTO para mostrar la evolución temporal de postulaciones.
    /// </summary>
    public class PostulacionesMesDTO
    {
        public string Mes { get; set; }
        public int Total { get; set; }
    }
}
