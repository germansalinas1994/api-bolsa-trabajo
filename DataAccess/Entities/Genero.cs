using System;
using System.Collections.Generic;

namespace DataAccess.Entities;

public partial class Genero
{
    public int Id { get; set; }
    public string Codigo { get; set; } = null!;
    public string Nombre { get; set; } = null!;


}
