using ClyvoVetApi.DTOs.Request;
using ClyvoVetApi.DTOs.Response;

namespace ClyvoVetApi.Services.Interfaces;

public interface IConsultaMedicamentoService
{
    Task<PagedResponseDto<ConsultaMedicamentoResponseDto>> GetAllAsync(int page, int pageSize);
    Task<ConsultaMedicamentoResponseDto> GetByIdAsync(int id);
    Task<IEnumerable<ConsultaMedicamentoResponseDto>> GetByConsultaIdAsync(int consultaId);
    Task<ConsultaMedicamentoResponseDto> CreateAsync(ConsultaMedicamentoRequestDto dto);
    Task<ConsultaMedicamentoResponseDto> UpdateAsync(int id, ConsultaMedicamentoRequestDto dto);
    Task DeleteAsync(int id);
}
