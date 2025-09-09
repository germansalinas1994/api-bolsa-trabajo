using System;
using DataAccess.Entities;

namespace BussinessLogic.DTO
{
    public sealed class PostulacionDTO
    {
        public int IdPostulacion { get; set; }
        public int IdOferta { get; set; }
        public string TituloOferta { get; set; } = null!;
        public string NombreEmpresa { get; set; } = null!;
        public string Estado { get; set; } = null!;       // "En revisión", "Entrevista", "Rechazada"
        public DateTime FechaPostulacion { get; set; }
        public string FechaPostulacionTexto { get; set; } = null!; // "Postulado el 21 Ago 2025"
        
        public int idPostulacionHistorial { get; set; } // para actualizaciones rápidas desde el front
    }
}