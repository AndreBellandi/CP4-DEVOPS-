using ClyvoVetApi.DTOs.Request;
using ClyvoVetApi.DTOs.Response;
using ClyvoVetApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClyvoVetApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MedicamentosController(IMedicamentoService service) : ControllerBase
{
    private readonly IMedicamentoService _service = service;

    /// <summary>
    /// Lista todos os medicamentos com paginação
    /// </summary>
    /// <param name="page">Número da página (inicia em 1)</param>
    /// <param name="pageSize">Tamanho da página (padrão 10)</param>
    /// <returns>Retorna uma lista paginada de medicamentos</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResponseDto<MedicamentoResponseDto>))]
    public async Task<IActionResult> GetMedicamentos(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetAllAsync(page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Busca um medicamento pelo seu ID
    /// </summary>
    /// <param name="id">ID do medicamento</param>
    /// <returns>Retorna os detalhes do medicamento</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MedicamentoResponseDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> GetMedicamento(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Busca medicamentos pelo nome
    /// </summary>
    /// <param name="nome">Nome do medicamento (ex: Amoxicilina)</param>
    /// <returns>Lista de medicamentos correspondentes</returns>
    [HttpGet("nome/{nome}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<MedicamentoResponseDto>))]
    public async Task<IActionResult> GetMedicamentosByNome(string nome)
    {
        var result = await _service.GetByNomeAsync(nome);
        return Ok(result);
    }

    /// <summary>
    /// Cadastra um novo medicamento
    /// </summary>
    /// <param name="dto">Dados do novo medicamento</param>
    /// <returns>Retorna o medicamento criado</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(MedicamentoResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> PostMedicamento([FromBody] MedicamentoRequestDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetMedicamento), new { id = result.Id }, result);
    }

    /// <summary>
    /// Atualiza as informações de um medicamento existente
    /// </summary>
    /// <param name="id">ID do medicamento</param>
    /// <param name="dto">Novos dados do medicamento</param>
    /// <returns>Retorna o medicamento atualizado</returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MedicamentoResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> PutMedicamento(int id, [FromBody] MedicamentoRequestDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        return Ok(result);
    }

    /// <summary>
    /// Remove um medicamento pelo seu ID
    /// </summary>
    /// <param name="id">ID do medicamento a ser removido</param>
    /// <returns>Retorna 204 NoContent</returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> DeleteMedicamento(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
