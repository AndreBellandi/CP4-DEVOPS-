using ClyvoVetApi.DTOs.Request;
using ClyvoVetApi.DTOs.Response;
using ClyvoVetApi.Exceptions;
using ClyvoVetApi.Models;
using ClyvoVetApi.Repositories.Interfaces;
using ClyvoVetApi.Services.Interfaces;

namespace ClyvoVetApi.Services;

public class PetService(IPetRepository repository, IDonoRepository donoRepository) : IPetService
{
    private readonly IPetRepository _repository = repository;
    private readonly IDonoRepository _donoRepository = donoRepository;

    public async Task<PagedResponseDto<PetResponseDto>> GetAllAsync(int page, int pageSize)
    {
        var (items, total) = await _repository.GetAllAsync(page, pageSize);
        return new PagedResponseDto<PetResponseDto>
        {
            Data = items.Select(ToResponseDto),
            Page = page,
            PageSize = pageSize,
            TotalItems = total
        };
    }

    public async Task<PetDetailsResponseDto> GetByIdAsync(int id)
    {
        var pet = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Pet com ID {id} não encontrado.");
        return ToDetailsResponseDto(pet);
    }

    public async Task<IEnumerable<PetResponseDto>> GetByEspecieAsync(string especie)
    {
        var items = await _repository.GetByEspecieAsync(especie);
        return items.Select(ToResponseDto);
    }

    public async Task<IEnumerable<PetResponseDto>> GetByRacaAsync(string raca)
    {
        var items = await _repository.GetByRacaAsync(raca);
        return items.Select(ToResponseDto);
    }

    public async Task<PetDetailsResponseDto> CreateAsync(PetRequestDto dto)
    {
        var dono = await _donoRepository.GetByIdAsync(dto.DonoId)
            ?? throw new BusinessException($"Dono associado com ID {dto.DonoId} não encontrado.");

        var pet = new Pet
        {
            Nome = dto.Nome,
            Especie = dto.Especie,
            Raca = dto.Raca,
            DataNascimento = dto.DataNascimento,
            Peso = dto.Peso,
            DonoId = dto.DonoId
        };

        var created = await _repository.CreateAsync(pet);
        
        // Atribuir o dono para o mapeamento correto do DTO
        created.Dono = dono;

        return ToDetailsResponseDto(created);
    }

    public async Task<PetDetailsResponseDto> UpdateAsync(int id, PetRequestDto dto)
    {
        var pet = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Pet com ID {id} não encontrado.");

        var dono = await _donoRepository.GetByIdAsync(dto.DonoId)
            ?? throw new BusinessException($"Dono associado com ID {dto.DonoId} não encontrado.");

        pet.Nome = dto.Nome;
        pet.Especie = dto.Especie;
        pet.Raca = dto.Raca;
        pet.DataNascimento = dto.DataNascimento;
        pet.Peso = dto.Peso;
        pet.DonoId = dto.DonoId;

        var updated = await _repository.UpdateAsync(pet);
        updated.Dono = dono;

        return ToDetailsResponseDto(updated);
    }

    public async Task DeleteAsync(int id)
    {
        var pet = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Pet com ID {id} não encontrado.");
        await _repository.DeleteAsync(pet);
    }

    private static PetResponseDto ToResponseDto(Pet p) => new()
    {
        Id = p.Id,
        Nome = p.Nome,
        Especie = p.Especie,
        Raca = p.Raca,
        DataNascimento = p.DataNascimento,
        Peso = p.Peso,
        DonoId = p.DonoId,
        DonoNome = p.Dono?.Nome ?? string.Empty
    };

    private static PetDetailsResponseDto ToDetailsResponseDto(Pet p) => new()
    {
        Id = p.Id,
        Nome = p.Nome,
        Especie = p.Especie,
        Raca = p.Raca,
        DataNascimento = p.DataNascimento,
        Peso = p.Peso,
        DonoId = p.DonoId,
        DonoNome = p.Dono?.Nome ?? string.Empty,
        Dono = p.Dono != null ? new DonoResponseDto
        {
            Id = p.Dono.Id,
            Nome = p.Dono.Nome,
            Email = p.Dono.Email,
            Telefone = p.Dono.Telefone,
            TotalPets = p.Dono.Pets?.Count ?? 0
        } : null,
        Consultas = p.Consultas?.Select(c => new ConsultaResponseDto
        {
            Id = c.Id,
            Tipo = c.Tipo,
            Valor = c.Valor,
            Descricao = c.Descricao,
            Status = c.Status,
            Data = c.Data,
            PetId = c.PetId,
            PetNome = p.Nome,
            FuncionarioId = c.FuncionarioId,
            FuncionarioNome = c.Funcionario?.Nome ?? string.Empty
        }) ?? [],
        Vacinas = p.Vacinas?.Select(v => new VacinaResponseDto
        {
            Id = v.Id,
            Nome = v.Nome,
            Data = v.Data,
            Status = v.Status,
            PetId = v.PetId,
            PetNome = p.Nome
        }) ?? []
    };
}
