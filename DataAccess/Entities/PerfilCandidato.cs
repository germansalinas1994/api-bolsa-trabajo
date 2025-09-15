using System;
using System.Collections.Generic;

namespace DataAccess.Entities;

public partial class PerfilCandidato 
{
    public int Id { get; set; }

    public string Descripcion { get; set; } = null!;

    public int IdUsuario { get; set; }

    public int? IdGenero { get; set; }
    public int? IdCarrera { get; set; }

    public string? Legajo { get; set; }

    public int? AnioEgreso { get; set; }

    public byte[]? Cv { get; set; }

    public DateTime FechaAlta { get; set; }

    public DateTime FechaModificacion { get; set; }

    public DateTime? FechaBaja { get; set; }


    public virtual Genero? Genero { get; set; }

    public virtual Usuario Usuario { get; set; } = null!;
    public virtual Carrera? Carrera { get; set; } = null!;
}