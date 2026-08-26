using ClyvoVetApi.DTOs.Request;
using ClyvoVetApi.DTOs.Response;
using ClyvoVetApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClyvoVetApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ConsultaMedicamentosController(IConsultaMedicamentoService service) : ControllerBase
{
    private readonly IConsultaMedicamentoService _service = service;

    /// <summary>
    /// Lista todas as associações de consulta e medicamento com paginação
    /// </summary>
    /// <param name="page">Número da página (inicia em 1)</param>
    /// <param name="pageSize">Tamanho da página (padrão 10)</param>
    /// <returns>Retorna uma lista paginada das associações</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResponseDto<ConsultaMedicamentoResponseDto>))]
    public async Task<IActionResult> GetConsultaMedicamentos(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetAllAsync(page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Busca uma associação de consulta e medicamento pelo seu ID
    /// </summary>
    /// <param name="id">ID da associação</param>
    /// <returns>Retorna os detalhes da associação</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ConsultaMedicamentoResponseDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> GetConsultaMedicamento(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Busca as prescrições de uma consulta específica
    /// </summary>
    /// <param name="consultaId">ID da consulta</param>
    /// <returns>Lista de medicamentos associados à consulta</returns>
    [HttpGet("consulta/{consultaId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ConsultaMedicamentoResponseDto>))]
    public async Task<IActionResult> GetConsultaMedicamentosByConsulta(int consultaId)
    {
        var result = await _service.GetByConsultaIdAsync(consultaId);
        return Ok(result);
    }

    /// <summary>
    /// Associa um medicamento a uma consulta (prescrição)
    /// </summary>
    /// <param name="dto">Dados da prescrição</param>
    /// <returns>Retorna a associação criada</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ConsultaMedicamentoResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> PostConsultaMedicamento([FromBody] ConsultaMedicamentoRequestDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetConsultaMedicamento), new { id = result.Id }, result);
    }

    /// <summary>
    /// Atualiza as informações (dosagem) de uma prescrição existente
    /// </summary>
    /// <param name="id">ID da prescrição</param>
    /// <param name="dto">Novos dados da prescrição</param>
    /// <returns>Retorna a prescrição atualizada</returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ConsultaMedicamentoResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> PutConsultaMedicamento(int id, [FromBody] ConsultaMedicamentoRequestDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        return Ok(result);
    }

    /// <summary>
    /// Remove a prescrição de um medicamento de uma consulta
    /// </summary>
    /// <param name="id">ID da prescrição a ser removida</param>
    /// <returns>Retorna 204 NoContent</returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> DeleteConsultaMedicamento(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
