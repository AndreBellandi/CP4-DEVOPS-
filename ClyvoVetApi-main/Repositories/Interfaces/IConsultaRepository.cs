using ClyvoVetApi.Models;

namespace ClyvoVetApi.Repositories.Interfaces;

public interface IConsultaRepository
{
    Task<(IEnumerable<Consulta> Items, int Total)> GetAllAsync(int page, int pageSize);
    Task<Consulta?> GetByIdAsync(int id);
    Task<IEnumerable<Consulta>> GetByFuncionarioIdAsync(int funcionarioId);
    Task<IEnumerable<Consulta>> GetByPeriodoAsync(DateTime inicio, DateTime fim);
    Task<Consulta> CreateAsync(Consulta consulta);
    Task<Consulta> UpdateAsync(Consulta consulta);
    Task DeleteAsync(Consulta consulta);
}
