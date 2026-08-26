using ClyvoVetApi.DTOs.Request;
using ClyvoVetApi.DTOs.Response;
using ClyvoVetApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClyvoVetApi.Controllers;

[ApiController]
[Route("api/tutores")]
[Produces("application/json")]
public class DonosController(IDonoService service) : ControllerBase
{
    private readonly IDonoService _service = service;

    /// <summary>
    /// Lista todos os donos com paginação
    /// </summary>
    /// <param name="page">Número da página (inicia em 1)</param>
    /// <param name="pageSize">Tamanho da página (padrão 10)</param>
    /// <returns>Retorna uma lista paginada de donos</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResponseDto<DonoResponseDto>))]
    public async Task<IActionResult> GetDonos(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetAllAsync(page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Busca um dono pelo seu ID
    /// </summary>
    /// <param name="id">ID único do dono</param>
    /// <returns>Retorna os detalhes completos do dono e a lista de seus pets</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DonoDetailsResponseDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> GetDono(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Busca um dono pelo seu e-mail
    /// </summary>
    /// <param name="email">E-mail cadastrado do dono</param>
    /// <returns>Retorna os detalhes completos do dono encontrado</returns>
    [HttpGet("email/{email}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DonoDetailsResponseDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> GetDonoByEmail(string email)
    {
        var result = await _service.GetByEmailAsync(email);
        return Ok(result);
    }

    /// <summary>
    /// Lista os pets de um dono específico
    /// </summary>
    /// <param name="id">ID do dono</param>
    /// <returns>Retorna a lista de pets vinculados ao dono</returns>
    [HttpGet("{id:int}/pets")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PetDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> GetPetsDoDono(int id)
    {
        var result = await _service.GetPetsByDonoIdAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Cadastra um novo dono
    /// </summary>
    /// <param name="dto">Dados para cadastro do dono</param>
    /// <returns>Retorna o dono recém-criado</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(DonoDetailsResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> PostDono([FromBody] DonoRequestDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetDono), new { id = result.Id }, result);
    }

    /// <summary>
    /// Atualiza as informações de um dono existente
    /// </summary>
    /// <param name="id">ID do dono a ser atualizado</param>
    /// <param name="dto">Novos dados do dono</param>
    /// <returns>Retorna o dono com os dados atualizados</returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DonoDetailsResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> PutDono(int id, [FromBody] DonoRequestDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        return Ok(result);
    }

    /// <summary>
    /// Remove um dono pelo seu ID
    /// </summary>
    /// <param name="id">ID do dono a ser excluído</param>
    /// <returns>Retorna status 204 NoContent indicando sucesso</returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> DeleteDono(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
