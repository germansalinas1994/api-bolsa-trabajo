using System;
using System.Collections.Generic;

namespace DataAccess.Entities;

public partial class Notificacion
{
    public int Id { get; set; }
    public int IdUsuario { get; set; }

    public string Mensaje { get; set; } = null!;

    public bool Leido { get; set; }

    public DateTime FechaEnvio { get; set; }

    public DateTime FechaAlta { get; set; }

    public DateTime FechaModificacion { get; set; }

    public DateTime? FechaBaja { get; set; }

    public string? Asunto { get; set; }

    public int IdPostulacion { get; set; }

    public virtual Postulacion Postulacion { get; set; } = null!;

    public virtual Usuario Usuario { get; set; } = null!;
}
