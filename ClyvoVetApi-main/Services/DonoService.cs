using ClyvoVetApi.DTOs.Request;
using ClyvoVetApi.DTOs.Response;
using ClyvoVetApi.Exceptions;
using ClyvoVetApi.Models;
using ClyvoVetApi.Repositories.Interfaces;
using ClyvoVetApi.Services.Interfaces;

namespace ClyvoVetApi.Services;

public class DonoService(IDonoRepository repository) : IDonoService
{
    private readonly IDonoRepository _repository = repository;

    public async Task<PagedResponseDto<DonoResponseDto>> GetAllAsync(int page, int pageSize)
    {
        var (items, total) = await _repository.GetAllAsync(page, pageSize);
        return new PagedResponseDto<DonoResponseDto>
        {
            Data       = items.Select(ToResponseDto),
            Page       = page,
            PageSize   = pageSize,
            TotalItems = total
        };
    }

    public async Task<DonoDetailsResponseDto> GetByIdAsync(int id)
    {
        var dono = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Dono com ID {id} não encontrado.");
        return ToDetailsResponseDto(dono);
    }

    public async Task<DonoDetailsResponseDto> GetByEmailAsync(string email)
    {
        var dono = await _repository.GetByEmailAsync(email)
            ?? throw new NotFoundException($"Dono com e-mail '{email}' não encontrado.");
        return ToDetailsResponseDto(dono);
    }

    public async Task<DonoDetailsResponseDto> CreateAsync(DonoRequestDto dto)
    {
        var existing = await _repository.GetByEmailAsync(dto.Email);
        if (existing != null)
            throw new BusinessException("Já existe um dono cadastrado com este e-mail.");

        var dono = new Dono
        {
            Nome     = dto.Nome,
            Email    = dto.Email,
            Telefone = dto.Telefone
        };

        var created = await _repository.CreateAsync(dono);
        return ToDetailsResponseDto(created);
    }

    public async Task<DonoDetailsResponseDto> UpdateAsync(int id, DonoRequestDto dto)
    {
        var dono = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Dono com ID {id} não encontrado.");

        if (dono.Email != dto.Email)
        {
            var existing = await _repository.GetByEmailAsync(dto.Email);
            if (existing != null)
                throw new BusinessException("Já existe outro dono cadastrado com este e-mail.");
        }

        dono.Nome     = dto.Nome;
        dono.Email    = dto.Email;
        dono.Telefone = dto.Telefone;

        var updated = await _repository.UpdateAsync(dono);
        return ToDetailsResponseDto(updated);
    }

    public async Task DeleteAsync(int id)
    {
        var dono = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Dono com ID {id} não encontrado.");
        await _repository.DeleteAsync(dono);
    }

    public async Task<IEnumerable<PetDto>> GetPetsByDonoIdAsync(int donoId)
    {
        var dono = await _repository.GetByIdAsync(donoId)
            ?? throw new NotFoundException($"Dono com ID {donoId} não encontrado.");
        
        return dono.Pets?.Select(p => new PetDto
        {
            Id             = p.Id,
            Nome           = p.Nome,
            Especie        = p.Especie,
            Raca           = p.Raca,
            DataNascimento = p.DataNascimento,
            Peso           = p.Peso,
            DonoId         = p.DonoId
        }) ?? [];
    }

    private static DonoResponseDto ToResponseDto(Dono d) => new()
    {
        Id        = d.Id,
        Nome      = d.Nome,
        Email     = d.Email,
        Telefone  = d.Telefone,
        TotalPets = d.Pets?.Count ?? 0
    };

    private static DonoDetailsResponseDto ToDetailsResponseDto(Dono d) => new()
    {
        Id        = d.Id,
        Nome      = d.Nome,
        Email     = d.Email,
        Telefone  = d.Telefone,
        TotalPets = d.Pets?.Count ?? 0,
        Pets      = d.Pets?.Select(p => new PetDto
        {
            Id             = p.Id,
            Nome           = p.Nome,
            Especie        = p.Especie,
            Raca           = p.Raca,
            DataNascimento = p.DataNascimento,
            Peso           = p.Peso,
            DonoId         = p.DonoId
        }) ?? []
    };
}
