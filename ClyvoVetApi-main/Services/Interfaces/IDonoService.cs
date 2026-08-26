using ClyvoVetApi.DTOs.Request;
using ClyvoVetApi.DTOs.Response;

namespace ClyvoVetApi.Services.Interfaces;

public interface IDonoService
{
    Task<PagedResponseDto<DonoResponseDto>> GetAllAsync(int page, int pageSize);
    Task<DonoDetailsResponseDto> GetByIdAsync(int id);
    Task<DonoDetailsResponseDto> GetByEmailAsync(string email);
    Task<DonoDetailsResponseDto> CreateAsync(DonoRequestDto dto);
    Task<DonoDetailsResponseDto> UpdateAsync(int id, DonoRequestDto dto);
    Task DeleteAsync(int id);
    Task<IEnumerable<PetDto>> GetPetsByDonoIdAsync(int donoId);
}
