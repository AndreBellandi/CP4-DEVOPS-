using ClyvoVetApi.DTOs.Request;
using ClyvoVetApi.DTOs.Response;
using ClyvoVetApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClyvoVetApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PetsController(IPetService service, IIntelligenceService intelligenceService) : ControllerBase
{
    private readonly IPetService _service = service;
    private readonly IIntelligenceService _intelligenceService = intelligenceService;

    /// <summary>
    /// Lista todos os pets com paginação
    /// </summary>
    /// <param name="page">Número da página (inicia em 1)</param>
    /// <param name="pageSize">Tamanho da página (padrão 10)</param>
    /// <returns>Retorna uma lista paginada de pets</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResponseDto<PetResponseDto>))]
    public async Task<IActionResult> GetPets(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetAllAsync(page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Busca um pet pelo seu ID
    /// </summary>
    /// <param name="id">ID do pet</param>
    /// <returns>Retorna os detalhes do pet, seu tutor e seu histórico de vacinas e consultas</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PetDetailsResponseDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> GetPet(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Busca pets por espécie
    /// </summary>
    /// <param name="especie">Espécie do pet (Ex: Cachorro, Gato)</param>
    /// <returns>Retorna uma lista de pets da espécie informada</returns>
    [HttpGet("especie/{especie}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PetResponseDto>))]
    public async Task<IActionResult> GetPetsByEspecie(string especie)
    {
        var result = await _service.GetByEspecieAsync(especie);
        return Ok(result);
    }

    /// <summary>
    /// Busca pets por raça
    /// </summary>
    /// <param name="raca">Raça do pet (Ex: Golden Retriever, Persa)</param>
    /// <returns>Retorna uma lista de pets da raça informada</returns>
    [HttpGet("raca/{raca}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PetResponseDto>))]
    public async Task<IActionResult> GetPetsByRaca(string raca)
    {
        var result = await _service.GetByRacaAsync(raca);
        return Ok(result);
    }

    /// <summary>
    /// Lista as vacinas de um pet específico
    /// </summary>
    /// <param name="id">ID do pet</param>
    /// <returns>Lista de vacinas aplicadas ou agendadas do pet</returns>
    [HttpGet("{id:int}/vacinas")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<VacinaResponseDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> GetVacinasDoPet(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(result.Vacinas);
    }

    /// <summary>
    /// Lista as consultas de um pet específico
    /// </summary>
    /// <param name="id">ID do pet</param>
    /// <returns>Lista de consultas realizadas ou agendadas do pet</returns>
    [HttpGet("{id:int}/consultas")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ConsultaResponseDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> GetConsultasDoPet(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(result.Consultas);
    }

    /// <summary>
    /// Cadastra um novo pet
    /// </summary>
    /// <param name="dto">Dados para cadastro do pet</param>
    /// <returns>Retorna o pet recém-criado</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(PetDetailsResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> PostPet([FromBody] PetRequestDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetPet), new { id = result.Id }, result);
    }

    /// <summary>
    /// Atualiza as informações de um pet existente
    /// </summary>
    /// <param name="id">ID do pet</param>
    /// <param name="dto">Novos dados do pet</param>
    /// <returns>Retorna o pet atualizado</returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PetDetailsResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> PutPet(int id, [FromBody] PetRequestDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        return Ok(result);
    }

    /// <summary>
    /// Remove um pet pelo seu ID
    /// </summary>
    /// <param name="id">ID do pet</param>
    /// <returns>Retorna 204 NoContent</returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> DeletePet(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Calcula o score de saúde preventiva do pet e gera recomendações inteligentes
    /// </summary>
    /// <param name="id">ID do pet</param>
    /// <returns>Dashboard de saúde preventiva do pet</returns>
    [HttpGet("{id:int}/inteligencia-preventiva")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(HealthDashboardResponseDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> GetIntelligencePreventiva(int id)
    {
        var result = await _intelligenceService.GetIntelligencePreventivaAsync(id);
        return Ok(result);
    }
}