using System;
using System.Collections.Generic;

namespace DataAccess.Entities;

public partial class EstadoPostulacion
{
    public static int IdEstadoAprobada = 1;
    public static int IdEstadoRechazada = 2;
    public static int IdEstadoEnRevision = 3;
    public static int IdEstadoIniciada = 4;

    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Codigo { get; set; }
}
