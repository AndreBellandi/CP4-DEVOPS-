using ClyvoVetApi.DTOs.Request;
using ClyvoVetApi.DTOs.Response;

namespace ClyvoVetApi.Services.Interfaces;

public interface IMedicamentoService
{
    Task<PagedResponseDto<MedicamentoResponseDto>> GetAllAsync(int page, int pageSize);
    Task<MedicamentoResponseDto> GetByIdAsync(int id);
    Task<IEnumerable<MedicamentoResponseDto>> GetByNomeAsync(string nome);
    Task<MedicamentoResponseDto> CreateAsync(MedicamentoRequestDto dto);
    Task<MedicamentoResponseDto> UpdateAsync(int id, MedicamentoRequestDto dto);
    Task DeleteAsync(int id);
}
