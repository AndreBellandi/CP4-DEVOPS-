using ClyvoVetApi.DTOs.Request;
using ClyvoVetApi.DTOs.Response;

namespace ClyvoVetApi.Services.Interfaces;

public interface IVacinaService
{
    Task<PagedResponseDto<VacinaResponseDto>> GetAllAsync(int page, int pageSize);
    Task<VacinaResponseDto> GetByIdAsync(int id);
    Task<IEnumerable<VacinaResponseDto>> GetPendentesAsync();
    Task<IEnumerable<VacinaResponseDto>> GetByNomeAsync(string nome);
    Task<IEnumerable<VacinaResponseDto>> GetProximasAsync(int dias);
    Task<VacinaResponseDto> CreateAsync(VacinaRequestDto dto);
    Task<VacinaResponseDto> UpdateAsync(int id, VacinaRequestDto dto);
    Task DeleteAsync(int id);
}
