using System.ComponentModel.DataAnnotations;

namespace ClyvoVetApi.DTOs.Request;

public class ConsultaMedicamentoRequestDto
{
    [Required(ErrorMessage = "ID da consulta é obrigatório.")]
    public int ConsultaId { get; set; }

    [Required(ErrorMessage = "ID do medicamento é obrigatório.")]
    public int MedicamentoId { get; set; }

    [Required(ErrorMessage = "Dosagem é obrigatória.")]
    [StringLength(100, ErrorMessage = "Dosagem deve ter no máximo 100 caracteres.")]
    public string Dosagem { get; set; } = string.Empty;
}
