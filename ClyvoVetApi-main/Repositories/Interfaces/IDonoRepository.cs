using ClyvoVetApi.Models;

namespace ClyvoVetApi.Repositories.Interfaces;

public interface IDonoRepository
{
    Task<(IEnumerable<Dono> Items, int Total)> GetAllAsync(int page, int pageSize);
    Task<Dono?> GetByIdAsync(int id);
    Task<Dono?> GetByEmailAsync(string email);
    Task<Dono> CreateAsync(Dono dono);
    Task<Dono> UpdateAsync(Dono dono);
    Task DeleteAsync(Dono dono);
}
