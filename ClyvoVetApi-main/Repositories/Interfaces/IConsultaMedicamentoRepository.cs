using ClyvoVetApi.Models;

namespace ClyvoVetApi.Repositories.Interfaces;

public interface IConsultaMedicamentoRepository
{
    Task<(IEnumerable<ConsultaMedicamento> Items, int Total)> GetAllAsync(int page, int pageSize);
    Task<ConsultaMedicamento?> GetByIdAsync(int id);
    Task<IEnumerable<ConsultaMedicamento>> GetByConsultaIdAsync(int consultaId);
    Task<ConsultaMedicamento> CreateAsync(ConsultaMedicamento consultaMedicamento);
    Task<ConsultaMedicamento> UpdateAsync(ConsultaMedicamento consultaMedicamento);
    Task DeleteAsync(ConsultaMedicamento consultaMedicamento);
}
