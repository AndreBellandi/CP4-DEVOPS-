using ClyvoVetApi.DTOs.Request;
using ClyvoVetApi.DTOs.Response;

namespace ClyvoVetApi.Services.Interfaces;

public interface IPetService
{
    Task<PagedResponseDto<PetResponseDto>> GetAllAsync(int page, int pageSize);
    Task<PetDetailsResponseDto> GetByIdAsync(int id);
    Task<IEnumerable<PetResponseDto>> GetByEspecieAsync(string especie);
    Task<IEnumerable<PetResponseDto>> GetByRacaAsync(string raca);
    Task<PetDetailsResponseDto> CreateAsync(PetRequestDto dto);
    Task<PetDetailsResponseDto> UpdateAsync(int id, PetRequestDto dto);
    Task DeleteAsync(int id);
}
