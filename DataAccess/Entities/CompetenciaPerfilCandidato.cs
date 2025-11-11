using System;

namespace DataAccess.Entities
{
    public class CompetenciaPerfilCandidato
    {
        public int Id { get; set; }
        public int IdPerfilCandidato { get; set; }
        public int IdCompetencia { get; set; }
        public DateTime FechaAlta { get; set; }
        public DateTime FechaModificacion { get; set; }
        public DateTime? FechaBaja { get; set; }

        public virtual PerfilCandidato PerfilCandidato { get; set; } = null!;
        public virtual Competencia Competencia { get; set; } = null!;
    }
}
