using ClyvoVetApi.Models;

namespace ClyvoVetApi.Repositories.Interfaces;

public interface IFuncionarioRepository
{
    Task<(IEnumerable<Funcionario> Items, int Total)> GetAllAsync(int page, int pageSize);
    Task<Funcionario?> GetByIdAsync(int id);
    Task<Funcionario?> GetByEmailAsync(string email);
    Task<IEnumerable<Funcionario>> GetBySetorAsync(string setor);
    Task<IEnumerable<Funcionario>> GetByCargoAsync(string cargo);
    Task<Funcionario> CreateAsync(Funcionario funcionario);
    Task<Funcionario> UpdateAsync(Funcionario funcionario);
    Task DeleteAsync(Funcionario funcionario);
}
