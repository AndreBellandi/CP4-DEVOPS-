using ClyvoVetApi.DTOs.Request;
using ClyvoVetApi.DTOs.Response;
using ClyvoVetApi.Exceptions;
using ClyvoVetApi.Models;
using ClyvoVetApi.Repositories.Interfaces;
using ClyvoVetApi.Services.Interfaces;

namespace ClyvoVetApi.Services;

public class VacinaService(IVacinaRepository repository, IPetRepository petRepository) : IVacinaService
{
    private readonly IVacinaRepository _repository = repository;
    private readonly IPetRepository _petRepository = petRepository;

    public async Task<PagedResponseDto<VacinaResponseDto>> GetAllAsync(int page, int pageSize)
    {
        var (items, total) = await _repository.GetAllAsync(page, pageSize);
        return new PagedResponseDto<VacinaResponseDto>
        {
            Data = items.Select(ToResponseDto),
            Page = page,
            PageSize = pageSize,
            TotalItems = total
        };
    }

    public async Task<VacinaResponseDto> GetByIdAsync(int id)
    {
        var vacina = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Vacina com ID {id} não encontrada.");
        return ToResponseDto(vacina);
    }

    public async Task<IEnumerable<VacinaResponseDto>> GetPendentesAsync()
    {
        var items = await _repository.GetPendentesAsync();
        return items.Select(ToResponseDto);
    }

    public async Task<IEnumerable<VacinaResponseDto>> GetByNomeAsync(string nome)
    {
        var items = await _repository.GetByNomeAsync(nome);
        return items.Select(ToResponseDto);
    }

    public async Task<IEnumerable<VacinaResponseDto>> GetProximasAsync(int dias)
    {
        if (dias <= 0)
            throw new BusinessException("A quantidade de dias deve ser maior que zero.");

        var limite = DateTime.Now.AddDays(dias);
        var items = await _repository.GetProximasAsync(limite);
        return items.Select(ToResponseDto);
    }

    public async Task<VacinaResponseDto> CreateAsync(VacinaRequestDto dto)
    {
        var pet = await _petRepository.GetByIdAsync(dto.PetId)
            ?? throw new BusinessException($"Pet associado com ID {dto.PetId} não encontrado.");

        var vacina = new Vacina
        {
            Nome = dto.Nome,
            Data = dto.Data,
            Status = dto.Status,
            PetId = dto.PetId
        };

        var created = await _repository.CreateAsync(vacina);
        created.Pet = pet;

        return ToResponseDto(created);
    }

    public async Task<VacinaResponseDto> UpdateAsync(int id, VacinaRequestDto dto)
    {
        var vacina = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Vacina com ID {id} não encontrada.");

        var pet = await _petRepository.GetByIdAsync(dto.PetId)
            ?? throw new BusinessException($"Pet associado com ID {dto.PetId} não encontrado.");

        vacina.Nome = dto.Nome;
        vacina.Data = dto.Data;
        vacina.Status = dto.Status;
        vacina.PetId = dto.PetId;

        var updated = await _repository.UpdateAsync(vacina);
        updated.Pet = pet;

        return ToResponseDto(updated);
    }

    public async Task DeleteAsync(int id)
    {
        var vacina = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Vacina com ID {id} não encontrada.");
        await _repository.DeleteAsync(vacina);
    }

    private static VacinaResponseDto ToResponseDto(Vacina v) => new()
    {
        Id = v.Id,
        Nome = v.Nome,
        Data = v.Data,
        Status = v.Status,
        PetId = v.PetId,
        PetNome = v.Pet?.Nome ?? string.Empty
    };
}
