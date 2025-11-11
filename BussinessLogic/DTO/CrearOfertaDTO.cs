using System.ComponentModel.DataAnnotations;

namespace BussinessLogic.DTO
{
    public class CrearOfertaDTO
    {
        [Required(ErrorMessage = "El título es obligatorio")]
        [StringLength(100, ErrorMessage = "El título no puede tener más de 100 caracteres")]
        public string? Titulo { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria")]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "El ID de modalidad es obligatorio")]
        public int? IdModalidad { get; set; }

        [Required(ErrorMessage = "El ID de tipo de contrato es obligatorio")]
        public int? IdTipoContrato { get; set; }

        // FechaInicio es requerida solo al crear, no al actualizar
        public DateTime? FechaInicio { get; set; }

        public DateTime? FechaFin { get; set; }

        public int? IdLocalidad { get; set; }

        [Required(ErrorMessage = "Los cupos son obligatorios")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe haber al menos 1 cupo disponible")]
        public int? Cupos { get; set; }
        public List<int>? IdCarreras { get; set; }
    }
}

