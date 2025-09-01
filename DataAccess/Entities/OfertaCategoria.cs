using System;
using System.Collections.Generic;

namespace DataAccess.Entities;

public partial class OfertaCategoria
{
    public int Id { get; set; }

    public int IdCategoria { get; set; }

    public int IdOferta { get; set; }

    public DateTime FechaAlta { get; set; }

    public DateTime FechaModificacion { get; set; }

    public DateTime? FechaBaja { get; set; }

    public virtual Categoria Categoria { get; set; } = null!;

    public virtual Oferta Oferta { get; set; } = null!;
}
