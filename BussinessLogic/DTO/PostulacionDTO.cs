using System;
using System.ComponentModel.DataAnnotations;
using DataAccess.Entities;

namespace BussinessLogic.DTO
{
    public class PostulacionDTO
    {
        public int? Id { get; set; }

        public int? IdPerfilCandidato { get; set; }
        public int? IdOferta { get; set; }
        public string? CartaPresentacion { get; set; }
        public string? Observacion { get; set; }

        public string? EstadoPostulacion { get; set; }
        public string? FechaPostulacion { get; set; }

        public string? NombreEmpresa { get; set; }
        public string? TituloOferta { get; set; }

        public string? DescripcionOferta { get; set; }
        public string? DescripcionModalidad { get; set; }
        public string? DescripcionTipoContrato { get; set; }

    }

}

