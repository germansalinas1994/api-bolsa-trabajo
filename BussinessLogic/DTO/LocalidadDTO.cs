using System.ComponentModel.DataAnnotations;

namespace BussinessLogic.DTO
{
    public class LocalidadDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string? CodPostal { get; set; }
        public string? NombreProvincia { get; set; }
        public string? NombrePais { get; set; }
    }
}

