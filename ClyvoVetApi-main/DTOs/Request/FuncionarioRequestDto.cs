using System.ComponentModel.DataAnnotations;

namespace ClyvoVetApi.DTOs.Request;

public class FuncionarioRequestDto
{
    [Required(ErrorMessage = "Nome é obrigatório.")]
    [StringLength(50, ErrorMessage = "Nome deve ter no máximo 50 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Setor é obrigatório.")]
    [StringLength(50, ErrorMessage = "Setor deve ter no máximo 50 caracteres.")]
    public string Setor { get; set; } = string.Empty;

    [Required(ErrorMessage = "Cargo é obrigatório.")]
    [StringLength(50, ErrorMessage = "Cargo deve ter no máximo 50 caracteres.")]
    public string Cargo { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "Formato de e-mail inválido.")]
    [StringLength(50, ErrorMessage = "E-mail deve ter no máximo 50 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Telefone é obrigatório.")]
    [StringLength(20, ErrorMessage = "Telefone deve ter no máximo 20 caracteres.")]
    public string Telefone { get; set; } = string.Empty;
}
