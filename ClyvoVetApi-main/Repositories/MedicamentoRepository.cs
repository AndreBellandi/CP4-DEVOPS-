using ClyvoVetApi.Data;
using ClyvoVetApi.Models;
using ClyvoVetApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClyvoVetApi.Repositories;

public class MedicamentoRepository(AppDbContext context) : IMedicamentoRepository
{
    private readonly AppDbContext _context = context;

    public async Task<(IEnumerable<Medicamento> Items, int Total)> GetAllAsync(int page, int pageSize)
    {
        var total = await _context.Medicamentos.CountAsync();
        var items = await _context.Medicamentos
            .AsNoTracking()
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, total);
    }

    public async Task<Medicamento?> GetByIdAsync(int id) =>
        await _context.Medicamentos
            .FirstOrDefaultAsync(m => m.Id == id);

    public async Task<IEnumerable<Medicamento>> GetByNomeAsync(string nome) =>
        await _context.Medicamentos
            .AsNoTracking()
            .Where(m => m.Nome.ToLower().Contains(nome.ToLower()))
            .ToListAsync();

    public async Task<Medicamento> CreateAsync(Medicamento medicamento)
    {
        _context.Medicamentos.Add(medicamento);
        await _context.SaveChangesAsync();
        return medicamento;
    }

    public async Task<Medicamento> UpdateAsync(Medicamento medicamento)
    {
        _context.Medicamentos.Update(medicamento);
        await _context.SaveChangesAsync();
        return medicamento;
    }

    public async Task DeleteAsync(Medicamento medicamento)
    {
        _context.Medicamentos.Remove(medicamento);
        await _context.SaveChangesAsync();
    }
}
