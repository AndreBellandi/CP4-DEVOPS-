namespace ClyvoVetApi.DTOs.Response;

public class ConsultaResponseDto
{
    public int Id { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime Data { get; set; }
    public int PetId { get; set; }
    public string PetNome { get; set; } = string.Empty;
    public int FuncionarioId { get; set; }
    public string FuncionarioNome { get; set; } = string.Empty;
}
