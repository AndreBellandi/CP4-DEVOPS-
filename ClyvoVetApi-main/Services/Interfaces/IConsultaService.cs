using ClyvoVetApi.DTOs.Request;
using ClyvoVetApi.DTOs.Response;

namespace ClyvoVetApi.Services.Interfaces;

public interface IConsultaService
{
    Task<PagedResponseDto<ConsultaResponseDto>> GetAllAsync(int page, int pageSize);
    Task<ConsultaResponseDto> GetByIdAsync(int id);
    Task<IEnumerable<ConsultaResponseDto>> GetByFuncionarioIdAsync(int funcionarioId);
    Task<IEnumerable<ConsultaResponseDto>> GetByPeriodoAsync(DateTime inicio, DateTime fim);
    Task<ConsultaResponseDto> CreateAsync(ConsultaRequestDto dto);
    Task<ConsultaResponseDto> UpdateAsync(int id, ConsultaRequestDto dto);
    Task DeleteAsync(int id);
}
