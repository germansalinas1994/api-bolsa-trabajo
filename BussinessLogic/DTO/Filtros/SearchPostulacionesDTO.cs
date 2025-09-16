public sealed class SearchPostulacionesDTO
{
    public int IdPerfilCandidato { get; set; }
    public List<string>? EstadosCodigo { get; set; } // ej: ["EN_REVISION", "ENTREVISTA"]
}
