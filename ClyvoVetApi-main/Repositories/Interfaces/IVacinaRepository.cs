using ClyvoVetApi.Models;

namespace ClyvoVetApi.Repositories.Interfaces;

public interface IVacinaRepository
{
    Task<(IEnumerable<Vacina> Items, int Total)> GetAllAsync(int page, int pageSize);
    Task<Vacina?> GetByIdAsync(int id);
    Task<IEnumerable<Vacina>> GetPendentesAsync();
    Task<IEnumerable<Vacina>> GetByNomeAsync(string nome);
    Task<IEnumerable<Vacina>> GetProximasAsync(DateTime limite);
    Task<Vacina> CreateAsync(Vacina vacina);
    Task<Vacina> UpdateAsync(Vacina vacina);
    Task DeleteAsync(Vacina vacina);
}
