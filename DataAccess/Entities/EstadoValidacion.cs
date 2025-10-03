using System;
using System.Collections.Generic;

namespace DataAccess.Entities;

public partial class EstadoValidacion 
{
    public static int IdEstadoAprobada = 1;
        public static int IdEstadoPendiente = 2;

    public static int IdEstadoRechazada = 3;
        public static int IdEstadoIniciada = 4;

    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string Codigo { get; set; } = null!;
}
