using System;
using System.Collections.Generic;

namespace DataAccess.Entities;

public partial class EstadoOferta
{
    public static int IdEstadoPublicada = 1;
    public static int IdEstadoBaja = 2;

    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Codigo { get; set; }

}
