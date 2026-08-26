using ClyvoVetApi.DTOs.Request;
using ClyvoVetApi.DTOs.Response;

namespace ClyvoVetApi.Services.Interfaces;

public interface IFuncionarioService
{
    Task<PagedResponseDto<FuncionarioResponseDto>> GetAllAsync(int page, int pageSize);
    Task<FuncionarioResponseDto> GetByIdAsync(int id);
    Task<FuncionarioResponseDto> GetByEmailAsync(string email);
    Task<IEnumerable<FuncionarioResponseDto>> GetBySetorAsync(string setor);
    Task<IEnumerable<FuncionarioResponseDto>> GetByCargoAsync(string cargo);
    Task<FuncionarioResponseDto> CreateAsync(FuncionarioRequestDto dto);
    Task<FuncionarioResponseDto> UpdateAsync(int id, FuncionarioRequestDto dto);
    Task DeleteAsync(int id);
}
