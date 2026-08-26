using System.Text.Json.Serialization;

namespace ClyvoVetApi.DTOs.Response;

public class PetDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Especie { get; set; } = string.Empty;
    public string Raca { get; set; } = string.Empty;
    public DateTime DataNascimento { get; set; }
    public decimal Peso { get; set; }
    [JsonPropertyName("tutorId")]
    public int DonoId { get; set; }
}
