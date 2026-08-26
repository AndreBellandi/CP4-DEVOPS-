using ClyvoVetApi.DTOs.Request;
using ClyvoVetApi.DTOs.Response;
using ClyvoVetApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClyvoVetApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class FuncionariosController(IFuncionarioService service) : ControllerBase
{
    private readonly IFuncionarioService _service = service;

    /// <summary>
    /// Lista todos os funcionários com paginação
    /// </summary>
    /// <param name="page">Número da página (inicia em 1)</param>
    /// <param name="pageSize">Tamanho da página (padrão 10)</param>
    /// <returns>Retorna uma lista paginada de funcionários</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResponseDto<FuncionarioResponseDto>))]
    public async Task<IActionResult> GetFuncionarios(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetAllAsync(page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Busca um funcionário pelo seu ID
    /// </summary>
    /// <param name="id">ID do funcionário</param>
    /// <returns>Retorna os detalhes do funcionário</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FuncionarioResponseDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> GetFuncionario(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Busca um funcionário pelo seu e-mail
    /// </summary>
    /// <param name="email">E-mail do funcionário</param>
    /// <returns>Retorna os detalhes do funcionário encontrado</returns>
    [HttpGet("email/{email}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FuncionarioResponseDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> GetFuncionarioByEmail(string email)
    {
        var result = await _service.GetByEmailAsync(email);
        return Ok(result);
    }

    /// <summary>
    /// Busca funcionários de um determinado setor
    /// </summary>
    /// <param name="setor">Nome do setor (Ex: Veterinária, Recepção)</param>
    /// <returns>Lista de funcionários do setor</returns>
    [HttpGet("setor/{setor}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<FuncionarioResponseDto>))]
    public async Task<IActionResult> GetFuncionariosBySetor(string setor)
    {
        var result = await _service.GetBySetorAsync(setor);
        return Ok(result);
    }

    /// <summary>
    /// Busca funcionários de um determinado cargo
    /// </summary>
    /// <param name="cargo">Nome do cargo (Ex: Veterinário Clínico, Atendente)</param>
    /// <returns>Lista de funcionários com o cargo informado</returns>
    [HttpGet("cargo/{cargo}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<FuncionarioResponseDto>))]
    public async Task<IActionResult> GetFuncionariosByCargo(string cargo)
    {
        var result = await _service.GetByCargoAsync(cargo);
        return Ok(result);
    }

    /// <summary>
    /// Cadastra um novo funcionário
    /// </summary>
    /// <param name="dto">Dados para cadastro do funcionário</param>
    /// <returns>Retorna o funcionário recém-criado</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(FuncionarioResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> PostFuncionario([FromBody] FuncionarioRequestDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetFuncionario), new { id = result.Id }, result);
    }

    /// <summary>
    /// Atualiza as informações de um funcionário existente
    /// </summary>
    /// <param name="id">ID do funcionário a ser atualizado</param>
    /// <param name="dto">Novos dados do funcionário</param>
    /// <returns>Retorna o funcionário atualizado</returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FuncionarioResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> PutFuncionario(int id, [FromBody] FuncionarioRequestDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        return Ok(result);
    }

    /// <summary>
    /// Remove um funcionário pelo seu ID
    /// </summary>
    /// <param name="id">ID do funcionário a ser excluído</param>
    /// <returns>Retorna status 204 NoContent indicando sucesso</returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> DeleteFuncionario(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
