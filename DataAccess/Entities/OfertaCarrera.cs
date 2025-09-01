using System;
using System.Collections.Generic;

namespace DataAccess.Entities;

public partial class OfertaCarrera 
{
    public int Id { get; set; }

    public int IdOferta { get; set; }

    public int IdCarrera { get; set; }

    public DateTime FechaAlta { get; set; }

    public DateTime FechaModificacion { get; set; }

    public DateTime? FechaBaja { get; set; }

    public virtual Carrera Carrera { get; set; } = null!;

    public virtual Oferta Oferta { get; set; } = null!;
}
