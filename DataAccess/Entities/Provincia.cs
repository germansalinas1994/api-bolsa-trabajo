using System;
using System.Collections.Generic;

namespace DataAccess.Entities;

public partial class Provincia
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public int IdPais { get; set; }


    public virtual Pais Pais { get; set; } = null!;
}
