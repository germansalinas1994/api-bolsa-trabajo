using System;
using System.ComponentModel.DataAnnotations;
using DataAccess.Entities;

namespace BussinessLogic.DTO
{
    public class PostulacionDTO
    {
        public int? IdPerfilCandidato { get; set; }
        public int? IdOferta { get; set; }
        public string? CartaPresentacion { get; set; }
        public string? Observacion { get; set; }
    }
}

