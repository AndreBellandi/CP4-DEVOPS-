using ClyvoVetApi.DTOs.Request;
using ClyvoVetApi.DTOs.Response;
using ClyvoVetApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClyvoVetApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class VacinasController(IVacinaService service) : ControllerBase
{
    private readonly IVacinaService _service = service;

    /// <summary>
    /// Lista todas as vacinas com paginação
    /// </summary>
    /// <param name="page">Número da página (inicia em 1)</param>
    /// <param name="pageSize">Tamanho da página (padrão 10)</param>
    /// <returns>Retorna uma lista paginada de vacinas</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResponseDto<VacinaResponseDto>))]
    public async Task<IActionResult> GetVacinas(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetAllAsync(page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Busca uma vacina pelo seu ID
    /// </summary>
    /// <param name="id">ID da vacina</param>
    /// <returns>Retorna os detalhes da vacina</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VacinaResponseDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> GetVacina(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Lista vacinas pendentes (não aplicadas)
    /// </summary>
    /// <returns>Lista de vacinas que ainda não foram aplicadas</returns>
    [HttpGet("pendentes")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<VacinaResponseDto>))]
    public async Task<IActionResult> GetVacinasPendentes()
    {
        var result = await _service.GetPendentesAsync();
        return Ok(result);
    }

    /// <summary>
    /// Busca vacinas pelo nome (marca/tipo)
    /// </summary>
    /// <param name="nome">Nome da vacina (Ex: Anti-rábica, V10)</param>
    /// <returns>Lista de vacinas correspondentes ao nome informado</returns>
    [HttpGet("nome/{nome}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<VacinaResponseDto>))]
    public async Task<IActionResult> GetVacinasByNome(string nome)
    {
        var result = await _service.GetByNomeAsync(nome);
        return Ok(result);
    }

    /// <summary>
    /// Lista as próximas vacinas programadas para os próximos dias
    /// </summary>
    /// <param name="dias">Quantidade de dias para frente no calendário (padrão 30)</param>
    /// <returns>Lista de vacinas pendentes que devem ser aplicadas no período informado</returns>
    [HttpGet("proximas")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<VacinaResponseDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> GetProximasVacinas([FromQuery] int dias = 30)
    {
        var result = await _service.GetProximasAsync(dias);
        return Ok(result);
    }

    /// <summary>
    /// Cadastra uma nova vacina
    /// </summary>
    /// <param name="dto">Dados da nova vacina</param>
    /// <returns>Retorna a vacina criada</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(VacinaResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> PostVacina([FromBody] VacinaRequestDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetVacina), new { id = result.Id }, result);
    }

    /// <summary>
    /// Atualiza as informações de uma vacina existente
    /// </summary>
    /// <param name="id">ID da vacina</param>
    /// <param name="dto">Novos dados da vacina</param>
    /// <returns>Retorna a vacina atualizada</returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VacinaResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> PutVacina(int id, [FromBody] VacinaRequestDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        return Ok(result);
    }

    /// <summary>
    /// Remove uma vacina pelo seu ID
    /// </summary>
    /// <param name="id">ID da vacina a ser removida</param>
    /// <returns>Retorna 204 NoContent</returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> DeleteVacina(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}