using ClyvoVetApi.Data;
using ClyvoVetApi.Models;
using ClyvoVetApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClyvoVetApi.Repositories;

public class DonoRepository(AppDbContext context) : IDonoRepository
{
    private readonly AppDbContext _context = context;

    public async Task<(IEnumerable<Dono> Items, int Total)> GetAllAsync(int page, int pageSize)
    {
        var total = await _context.Donos.CountAsync();
        var items = await _context.Donos
            .Include(d => d.Pets)
            .AsNoTracking()
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, total);
    }

    public async Task<Dono?> GetByIdAsync(int id) =>
        await _context.Donos
            .Include(d => d.Pets)
            .FirstOrDefaultAsync(d => d.Id == id);

    public async Task<Dono?> GetByEmailAsync(string email) =>
        await _context.Donos
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Email == email);

    public async Task<Dono> CreateAsync(Dono dono)
    {
        _context.Donos.Add(dono);
        await _context.SaveChangesAsync();
        return dono;
    }

    public async Task<Dono> UpdateAsync(Dono dono)
    {
        _context.Donos.Update(dono);
        await _context.SaveChangesAsync();
        return dono;
    }

    public async Task DeleteAsync(Dono dono)
    {
        _context.Donos.Remove(dono);
        await _context.SaveChangesAsync();
    }
}
