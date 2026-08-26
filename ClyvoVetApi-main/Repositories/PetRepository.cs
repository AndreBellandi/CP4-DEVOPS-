using ClyvoVetApi.Data;
using ClyvoVetApi.Models;
using ClyvoVetApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClyvoVetApi.Repositories;

public class PetRepository(AppDbContext context) : IPetRepository
{
    private readonly AppDbContext _context = context;

    public async Task<(IEnumerable<Pet> Items, int Total)> GetAllAsync(int page, int pageSize)
    {
        var total = await _context.Pets.CountAsync();
        var items = await _context.Pets
            .Include(p => p.Dono)
            .AsNoTracking()
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, total);
    }

    public async Task<Pet?> GetByIdAsync(int id) =>
        await _context.Pets
            .Include(p => p.Dono)
            .Include(p => p.Consultas)
            .Include(p => p.Vacinas)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<IEnumerable<Pet>> GetByEspecieAsync(string especie) =>
        await _context.Pets
            .Include(p => p.Dono)
            .AsNoTracking()
            .Where(p => p.Especie.ToLower() == especie.ToLower())
            .ToListAsync();

    public async Task<IEnumerable<Pet>> GetByRacaAsync(string raca) =>
        await _context.Pets
            .Include(p => p.Dono)
            .AsNoTracking()
            .Where(p => p.Raca.ToLower() == raca.ToLower())
            .ToListAsync();

    public async Task<Pet> CreateAsync(Pet pet)
    {
        _context.Pets.Add(pet);
        await _context.SaveChangesAsync();
        return pet;
    }

    public async Task<Pet> UpdateAsync(Pet pet)
    {
        _context.Pets.Update(pet);
        await _context.SaveChangesAsync();
        return pet;
    }

    public async Task DeleteAsync(Pet pet)
    {
        _context.Pets.Remove(pet);
        await _context.SaveChangesAsync();
    }
}
