using System;
using System.Collections.Generic;

namespace DataAccess.Entities;

public partial class Localidad
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;
    public int IdProvincia { get; set; }

    public string? CodPostal { get; set; }


    public virtual Provincia Provincia { get; set; } = null!;
}
