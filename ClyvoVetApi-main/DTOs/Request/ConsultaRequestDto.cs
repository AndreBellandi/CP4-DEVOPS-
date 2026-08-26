using System.ComponentModel.DataAnnotations;

namespace ClyvoVetApi.DTOs.Request;

public class ConsultaRequestDto
{
    [Required(ErrorMessage = "Tipo da consulta é obrigatório.")]
    [StringLength(50, ErrorMessage = "Tipo da consulta deve ter no máximo 50 caracteres.")]
    public string Tipo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Valor da consulta é obrigatório.")]
    [Range(0.00, 99999999.99, ErrorMessage = "Valor da consulta deve ser entre 0.00 e 99999999.99.")]
    public decimal Valor { get; set; }

    [Required(ErrorMessage = "Descrição da consulta é obrigatória.")]
    [StringLength(500, ErrorMessage = "Descrição deve ter no máximo 500 caracteres.")]
    public string Descricao { get; set; } = string.Empty;

    [Required(ErrorMessage = "Status da consulta é obrigatório.")]
    [RegularExpression("^[ACR]$", ErrorMessage = "Status deve ser 'A' (Agendada), 'C' (Cancelada) ou 'R' (Realizada).")]
    public string Status { get; set; } = "A";

    [Required(ErrorMessage = "Data da consulta é obrigatória.")]
    public DateTime Data { get; set; }

    [Required(ErrorMessage = "ID do pet é obrigatório.")]
    public int PetId { get; set; }

    [Required(ErrorMessage = "ID do funcionário é obrigatório.")]
    public int FuncionarioId { get; set; }
}
