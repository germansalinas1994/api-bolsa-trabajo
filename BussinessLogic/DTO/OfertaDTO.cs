using System;
using System.ComponentModel.DataAnnotations;
using DataAccess.Entities;

namespace BussinessLogic.DTO
{
    public class OfertaDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El título es obligatorio")]
        [StringLength(100, ErrorMessage = "El título no puede tener más de 100 caracteres")]
        public string Titulo { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria")]
        public string Descripcion { get; set; }

        public string? NombreLocalidad { get; set; }

        public string? NombreProvincia { get; set; }

        public string? NombreEmpresa { get; set; }

        public string? TipoContrato { get; set; }

        public string? Modalidad { get; set; }

        public string? FechaInicio { get; set; }

        public string? FechaFin { get; set; }


        //datos para postularse
        public string? CartaPresentacion { get; set; }
        public string? Observacion { get; set; }
    }
}

