using System;
using System.Collections.Generic;

namespace DataAccess.Entities;

public partial class EstadoValidacion 
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string Codigo { get; set; } = null!;
}
