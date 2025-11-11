using System;
using System.Collections.Generic;

namespace DataAccess.Entities
{
    public class Competencia
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;

        public virtual ICollection<CompetenciaPerfilCandidato> CompetenciasPerfilCandidato { get; set; } = new List<CompetenciaPerfilCandidato>();
    }
}
