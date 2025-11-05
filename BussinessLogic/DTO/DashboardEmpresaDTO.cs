using MimeKit.Encodings;

namespace BussinessLogic.DTO
{
    public class DashboardAdminDTO
    {
        public MetricasDTO Metricas { get; set; }
        public Dictionary<string, int> CantidadCarreras { get; set; }
        public List<PostulacionesMesDTO> PostulacionesPorMes { get; set; }
    }

    public class MetricasDTO
    {
        public int OfertasPublicadas { get; set; }
        public int PostulacionesRecibidas { get; set; }

        public int EmpresasRegistradas { get; set; }
        public int CandidatosRegistrados { get; set; }
        public int EmpresasVerificadas { get; set; }
        public int EmpresasNoVerificadas { get; set; }
        public int ofertasSistemas { get; set; }
        public int ofertasIndustrial { get; set; }
        public int ofertasElectrica { get; set; }
        public int ofertasCivil { get; set; }
        public int ofertasQuimica { get; set; }
        public int ofertasMecanica { get; set; }
        public int CandidatosUnicos { get; set; }
        public int OfertasActivas { get; set; }
    }

    public class PostulacionesMesDTO
    {
        public string Mes { get; set; }
        public int Total { get; set; }
    }
}
