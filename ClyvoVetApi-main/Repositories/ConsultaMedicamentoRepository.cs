using ClyvoVetApi.Data;
using ClyvoVetApi.Models;
using ClyvoVetApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClyvoVetApi.Repositories;

public class ConsultaMedicamentoRepository(AppDbContext context) : IConsultaMedicamentoRepository
{
    private readonly AppDbContext _context = context;

    public async Task<(IEnumerable<ConsultaMedicamento> Items, int Total)> GetAllAsync(int page, int pageSize)
    {
        var total = await _context.ConsultasMedicamentos.CountAsync();
        var items = await _context.ConsultasMedicamentos
            .Include(cm => cm.Consulta)
            .Include(cm => cm.Medicamento)
            .AsNoTracking()
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, total);
    }

    public async Task<ConsultaMedicamento?> GetByIdAsync(int id) =>
        await _context.ConsultasMedicamentos
            .Include(cm => cm.Consulta)
            .Include(cm => cm.Medicamento)
            .FirstOrDefaultAsync(cm => cm.Id == id);

    public async Task<IEnumerable<ConsultaMedicamento>> GetByConsultaIdAsync(int consultaId) =>
        await _context.ConsultasMedicamentos
            .Include(cm => cm.Consulta)
            .Include(cm => cm.Medicamento)
            .AsNoTracking()
            .Where(cm => cm.ConsultaId == consultaId)
            .ToListAsync();

    public async Task<ConsultaMedicamento> CreateAsync(ConsultaMedicamento consultaMedicamento)
    {
        _context.ConsultasMedicamentos.Add(consultaMedicamento);
        await _context.SaveChangesAsync();
        return consultaMedicamento;
    }

    public async Task<ConsultaMedicamento> UpdateAsync(ConsultaMedicamento consultaMedicamento)
    {
        _context.ConsultasMedicamentos.Update(consultaMedicamento);
        await _context.SaveChangesAsync();
        return consultaMedicamento;
    }

    public async Task DeleteAsync(ConsultaMedicamento consultaMedicamento)
    {
        _context.ConsultasMedicamentos.Remove(consultaMedicamento);
        await _context.SaveChangesAsync();
    }
}
