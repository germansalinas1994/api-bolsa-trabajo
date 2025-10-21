using System;

namespace BussinessLogic.DTO
{
    public class PerfilCandidatoDTO
    {
        // Campos de PerfilCandidato (según diagrama)
        public int Id { get; set; }
        public string? Descripcion { get; set; }
        public int IdUsuario { get; set; }
        public int? IdGenero { get; set; }
        public string? Legajo { get; set; }
        public int? AnioEgreso { get; set; }
        public string? Cv { get; set; } // Convertido de byte[] a string base64
        public DateTime FechaAlta { get; set; }
        public DateTime FechaModificacion { get; set; }
        public DateTime? FechaBaja { get; set; }
        
        // Campos derivados del Usuario (según diagrama)
        public string? Nombre { get; set; }
        public string? Email { get; set; }
        public bool? UsuarioActivo { get; set; }
        public int? IdRol { get; set; }
        
        // Campos derivados del Genero (según diagrama)
        public string? GeneroNombre { get; set; }
        public string? GeneroCodigo { get; set; }
        
        // Campos derivados del Rol (según diagrama)
        public string? RolNombre { get; set; }
        public string? RolCodigo { get; set; }
        
        // Campos derivados de la Carrera (según diagrama)
        public int? IdCarrera { get; set; }
        public string? CarreraNombre { get; set; }
        public string? CarreraCodigo { get; set; }
        
        // Campos calculados por el backend
        public int? PorcentajePerfil { get; set; }
        
        // Campos que no están en el diagrama (para compatibilidad UI)
        public string? Telefono { get; set; } // NULL - no existe en diagrama
        public string? Localidad { get; set; } // NULL - no está relacionado
        public string? Carrera { get; set; } // DEPRECATED - usar CarreraNombre
    }
}