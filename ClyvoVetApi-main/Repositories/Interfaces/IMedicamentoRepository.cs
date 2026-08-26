using ClyvoVetApi.Models;

namespace ClyvoVetApi.Repositories.Interfaces;

public interface IMedicamentoRepository
{
    Task<(IEnumerable<Medicamento> Items, int Total)> GetAllAsync(int page, int pageSize);
    Task<Medicamento?> GetByIdAsync(int id);
    Task<IEnumerable<Medicamento>> GetByNomeAsync(string nome);
    Task<Medicamento> CreateAsync(Medicamento medicamento);
    Task<Medicamento> UpdateAsync(Medicamento medicamento);
    Task DeleteAsync(Medicamento medicamento);
}
