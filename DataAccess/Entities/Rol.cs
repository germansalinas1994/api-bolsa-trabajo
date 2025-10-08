using System;
using System.Collections.Generic;

namespace DataAccess.Entities;

public partial class Rol
{
    public static int IdRolAdmin = 1;
    public static int IdRolEmpresa = 2;
    public static int IdRolCandidato = 3;
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string Codigo { get; set; } = null!;

}
