namespace ClyvoVetApi.DTOs.Response;

public class VacinaResponseDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public DateTime Data { get; set; }
    public string Status { get; set; } = string.Empty;
    public int PetId { get; set; }
    public string PetNome { get; set; } = string.Empty;
}
