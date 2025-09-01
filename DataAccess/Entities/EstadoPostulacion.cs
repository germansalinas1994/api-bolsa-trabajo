using System;
using System.Collections.Generic;

namespace DataAccess.Entities;

public partial class EstadoPostulacion
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Codigo { get; set; }
}
