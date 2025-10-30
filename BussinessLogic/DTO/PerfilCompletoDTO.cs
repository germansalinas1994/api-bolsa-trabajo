namespace BussinessLogic.DTO;

public class PerfilCompletoDTO
{
    public int IdUsuario { get; set; }
    public string? Email { get; set; }
    public string? Nombre { get; set; }
    public int IdRol { get; set; }

    public string? Activo { get; set; }

    public string? FechaAlta { get; set; }
    public string? FechaBaja { get; set; }

    public string? RolNombre { get; set; }

    //Datos si es candidato
    public string? DescripcionCandidato { get; set; }
    public string? NombreCarrera { get; set; }
    public string? Legajo { get; set; }
    public string? Genero { get; set; }


    //Datos si es empresa
    public string? DescripcionEmpresa { get; set; }
    public string? RazonSocial { get; set; }
    public string? Cuit { get; set; }
    public string? EstadoValidacion { get; set; }

}

