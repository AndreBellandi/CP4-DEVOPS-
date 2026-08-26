using ClyvoVetApi.DTOs.Request;
using ClyvoVetApi.DTOs.Response;
using ClyvoVetApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClyvoVetApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ConsultasController(IConsultaService service) : ControllerBase
{
    private readonly IConsultaService _service = service;

    /// <summary>
    /// Lista todas as consultas com paginação
    /// </summary>
    /// <param name="page">Número da página (inicia em 1)</param>
    /// <param name="pageSize">Tamanho da página (padrão 10)</param>
    /// <returns>Retorna uma lista paginada de consultas</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResponseDto<ConsultaResponseDto>))]
    public async Task<IActionResult> GetConsultas(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetAllAsync(page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Busca uma consulta pelo seu ID
    /// </summary>
    /// <param name="id">ID da consulta</param>
    /// <returns>Retorna os detalhes da consulta</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ConsultaResponseDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> GetConsulta(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Lista as consultas de um funcionário específico (veterinário)
    /// </summary>
    /// <param name="funcionarioId">ID do funcionário</param>
    /// <returns>Lista de consultas realizadas ou agendadas pelo funcionário</returns>
    [HttpGet("funcionario/{funcionarioId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ConsultaResponseDto>))]
    public async Task<IActionResult> GetConsultasByFuncionario(int funcionarioId)
    {
        var result = await _service.GetByFuncionarioIdAsync(funcionarioId);
        return Ok(result);
    }

    /// <summary>
    /// Busca consultas por período de datas
    /// </summary>
    /// <param name="inicio">Data de início do período (ISO format)</param>
    /// <param name="fim">Data de fim do período (ISO format)</param>
    /// <returns>Lista de consultas dentro do período</returns>
    [HttpGet("periodo")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ConsultaResponseDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> GetConsultasByPeriodo(
        [FromQuery] DateTime inicio,
        [FromQuery] DateTime fim)
    {
        var result = await _service.GetByPeriodoAsync(inicio, fim);
        return Ok(result);
    }

    /// <summary>
    /// Cadastra uma nova consulta
    /// </summary>
    /// <param name="dto">Dados da nova consulta</param>
    /// <returns>Retorna a consulta criada</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ConsultaResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> PostConsulta([FromBody] ConsultaRequestDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetConsulta), new { id = result.Id }, result);
    }

    /// <summary>
    /// Atualiza as informações de uma consulta existente
    /// </summary>
    /// <param name="id">ID da consulta</param>
    /// <param name="dto">Novos dados da consulta</param>
    /// <returns>Retorna a consulta atualizada</returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ConsultaResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> PutConsulta(int id, [FromBody] ConsultaRequestDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        return Ok(result);
    }

    /// <summary>
    /// Remove uma consulta pelo seu ID
    /// </summary>
    /// <param name="id">ID da consulta</param>
    /// <returns>Retorna 204 NoContent</returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> DeleteConsulta(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}