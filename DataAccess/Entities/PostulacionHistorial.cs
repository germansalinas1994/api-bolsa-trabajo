using System;
using System.Collections.Generic;

namespace DataAccess.Entities;

public partial class PostulacionHistorial 
{
    public int Id { get; set; }
    public int IdPostulacion { get; set; }

    public int IdEstadoPostulacion { get; set; }

    public string? Motivo { get; set; }

    public DateTime FechaAlta { get; set; }

    public DateTime FechaModificacion { get; set; }

    public string? FechaBaja { get; set; }

    public virtual EstadoPostulacion EstadoPostulacion { get; set; } = null!;

    public virtual Postulacion Postulacion { get; set; } = null!;
}
