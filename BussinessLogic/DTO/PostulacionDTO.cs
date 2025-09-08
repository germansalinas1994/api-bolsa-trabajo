// BussinessLogic/DTO/Postulaciones/ApplicationCardDto.cs
namespace BussinessLogic.DTO.Postulaciones
{
    public sealed class ApplicationCardDto
    {
        public int IdPostulacion { get; set; }
        public int IdOferta { get; set; }
        public string TituloOferta { get; set; } = null!;
        public string NombreEmpresa { get; set; } = null!;
        public string Estado { get; set; } = null!;       // "En revisión", "Entrevista", "Rechazada"
        public DateTime FechaPostulacion { get; set; }
        public string FechaPostulacionTexto { get; set; } = null!; // "Postulado el 21 Ago 2025"
    }
}