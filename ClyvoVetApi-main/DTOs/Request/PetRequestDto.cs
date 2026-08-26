using System.ComponentModel.DataAnnotations;

using System.Text.Json.Serialization;

namespace ClyvoVetApi.DTOs.Request;

public class PetRequestDto
{
    [Required(ErrorMessage = "Nome do pet é obrigatório.")]
    [StringLength(50, ErrorMessage = "Nome do pet deve ter no máximo 50 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Espécie do pet é obrigatória.")]
    [StringLength(50, ErrorMessage = "Espécie deve ter no máximo 50 caracteres.")]
    public string Especie { get; set; } = string.Empty;

    [Required(ErrorMessage = "Raça do pet é obrigatória.")]
    [StringLength(50, ErrorMessage = "Raça deve ter no máximo 50 caracteres.")]
    public string Raca { get; set; } = string.Empty;

    [Required(ErrorMessage = "Data de nascimento do pet é obrigatória.")]
    public DateTime DataNascimento { get; set; }

    [Required(ErrorMessage = "Peso é obrigatório.")]
    [Range(0.01, 999.99, ErrorMessage = "Peso deve ser entre 0.01 e 999.99.")]
    public decimal Peso { get; set; }

    [Required(ErrorMessage = "ID do dono é obrigatório.")]
    [JsonPropertyName("tutorId")]
    public int DonoId { get; set; }
}
