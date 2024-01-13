using System;
using System.Collections.Generic;

namespace DataAccess.Entities;

public partial class Estadoenvio
{

    public static int Ingresado = 1;
    public static int Enviado = 2;
    public static int Entregado = 3;
    public int IdEstadoEnvio { get; set; }

    public string? Descripcion { get; set; }

    public virtual ICollection<Envio> Envios { get; set; } = null!;

}
