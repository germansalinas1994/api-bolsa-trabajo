using System;

namespace BussinessLogic.DTO
{
    public class NotificacionDTO
    {
        public int Id { get; set; }
        public int IdUsuario { get; set; }
        public string Mensaje { get; set; } = null!;
        public bool Leido { get; set; }
        public DateTime FechaEnvio { get; set; }
        public string? Asunto { get; set; }
        public int IdPostulacion { get; set; }
        
        // Campos adicionales para el frontend
        public string? TituloOferta { get; set; }
        public string? NombreEmpresa { get; set; }
        public string? NombreCandidato { get; set; }
        public string? EmailUsuario { get; set; }
    }
    
    public class CrearNotificacionDTO
    {
        public int IdUsuario { get; set; }
        public string Mensaje { get; set; } = null!;
        public string? Asunto { get; set; }
        public int IdPostulacion { get; set; }
    }
    
    public class NotificacionCountDTO
    {
        public int NoLeidas { get; set; }
        public int Total { get; set; }
    }
}
