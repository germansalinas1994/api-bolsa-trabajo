using System;
using System.Collections.Generic;

namespace DataAccess.Entities;

public partial class Postulacion
{
    public int Id { get; set; }

    public int IdPerfilCandidato { get; set; }

    public int IdOferta { get; set; }

    public string? CartaPresentacion { get; set; }

    public string? Observacion { get; set; }

    public DateTime FechaAlta { get; set; }

    public DateTime FechaModificacion { get; set; }

    public DateTime? FechaBaja { get; set; }


    public virtual Oferta Oferta { get; set; } = null!;

    public virtual PerfilCandidato PerfilCandidato { get; set; } = null!;

    public virtual ICollection<PostulacionHistorial>? Historial { get; set; }

}
