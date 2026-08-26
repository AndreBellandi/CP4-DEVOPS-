using System.ComponentModel.DataAnnotations;

namespace ClyvoVetApi.DTOs.Request;

public class MedicamentoRequestDto
{
    [Required(ErrorMessage = "Nome do medicamento é obrigatório.")]
    [StringLength(100, ErrorMessage = "Nome do medicamento deve ter no máximo 100 caracteres.")]
    public string Nome { get; set; } = string.Empty;
}
