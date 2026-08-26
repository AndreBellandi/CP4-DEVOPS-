namespace ClyvoVetApi.DTOs.Response;

public class PetResponseDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Especie { get; set; } = string.Empty;
    public string Raca { get; set; } = string.Empty;
    public DateTime DataNascimento { get; set; }
    public decimal Peso { get; set; }
    public int DonoId { get; set; }
    public string DonoNome { get; set; } = string.Empty;
}
