using ClyvoVetApi.Data;
using ClyvoVetApi.Models;
using ClyvoVetApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClyvoVetApi.Repositories;

public class FuncionarioRepository(AppDbContext context) : IFuncionarioRepository
{
    private readonly AppDbContext _context = context;

    public async Task<(IEnumerable<Funcionario> Items, int Total)> GetAllAsync(int page, int pageSize)
    {
        var total = await _context.Funcionarios.CountAsync();
        var items = await _context.Funcionarios
            .AsNoTracking()
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, total);
    }

    public async Task<Funcionario?> GetByIdAsync(int id) =>
        await _context.Funcionarios
            .FirstOrDefaultAsync(f => f.Id == id);

    public async Task<Funcionario?> GetByEmailAsync(string email) =>
        await _context.Funcionarios
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Email == email);

    public async Task<IEnumerable<Funcionario>> GetBySetorAsync(string setor) =>
        await _context.Funcionarios
            .AsNoTracking()
            .Where(f => f.Setor.ToLower() == setor.ToLower())
            .ToListAsync();

    public async Task<IEnumerable<Funcionario>> GetByCargoAsync(string cargo) =>
        await _context.Funcionarios
            .AsNoTracking()
            .Where(f => f.Cargo.ToLower() == cargo.ToLower())
            .ToListAsync();

    public async Task<Funcionario> CreateAsync(Funcionario funcionario)
    {
        _context.Funcionarios.Add(funcionario);
        await _context.SaveChangesAsync();
        return funcionario;
    }

    public async Task<Funcionario> UpdateAsync(Funcionario funcionario)
    {
        _context.Funcionarios.Update(funcionario);
        await _context.SaveChangesAsync();
        return funcionario;
    }

    public async Task DeleteAsync(Funcionario funcionario)
    {
        _context.Funcionarios.Remove(funcionario);
        await _context.SaveChangesAsync();
    }
}
