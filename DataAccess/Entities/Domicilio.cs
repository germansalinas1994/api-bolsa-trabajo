using System;
using System.Collections.Generic;

namespace DataAccess.Entities;

public partial class Domicilio
{
    public int IdDomicilio { get; set; }

    public int? Altura { get; set; }

    public string? Calle { get; set; }

    public string? Aclaracion { get; set; }

    public string? Departamento { get; set; }

    public int IdUsuario { get; set; }

    public string CodigoPostal {get; set;}

    public DateTime FechaDesde {get; set;}

    public DateTime? FechaHasta {get; set;}

    public DateTime FechaActualizacion {get; set;}

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;

    public virtual ICollection<Envio> Envios { get; set; } = null!;
}
