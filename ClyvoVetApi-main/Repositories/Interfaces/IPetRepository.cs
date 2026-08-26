using ClyvoVetApi.Models;

namespace ClyvoVetApi.Repositories.Interfaces;

public interface IPetRepository
{
    Task<(IEnumerable<Pet> Items, int Total)> GetAllAsync(int page, int pageSize);
    Task<Pet?> GetByIdAsync(int id);
    Task<IEnumerable<Pet>> GetByEspecieAsync(string especie);
    Task<IEnumerable<Pet>> GetByRacaAsync(string raca);
    Task<Pet> CreateAsync(Pet pet);
    Task<Pet> UpdateAsync(Pet pet);
    Task DeleteAsync(Pet pet);
}
