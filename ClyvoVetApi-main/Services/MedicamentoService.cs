using ClyvoVetApi.DTOs.Request;
using ClyvoVetApi.DTOs.Response;
using ClyvoVetApi.Exceptions;
using ClyvoVetApi.Models;
using ClyvoVetApi.Repositories.Interfaces;
using ClyvoVetApi.Services.Interfaces;

namespace ClyvoVetApi.Services;

public class MedicamentoService(IMedicamentoRepository repository) : IMedicamentoService
{
    private readonly IMedicamentoRepository _repository = repository;

    public async Task<PagedResponseDto<MedicamentoResponseDto>> GetAllAsync(int page, int pageSize)
    {
        var (items, total) = await _repository.GetAllAsync(page, pageSize);
        return new PagedResponseDto<MedicamentoResponseDto>
        {
            Data = items.Select(ToResponseDto),
            Page = page,
            PageSize = pageSize,
            TotalItems = total
        };
    }

    public async Task<MedicamentoResponseDto> GetByIdAsync(int id)
    {
        var medicamento = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Medicamento com ID {id} não encontrado.");
        return ToResponseDto(medicamento);
    }

    public async Task<IEnumerable<MedicamentoResponseDto>> GetByNomeAsync(string nome)
    {
        var items = await _repository.GetByNomeAsync(nome);
        return items.Select(ToResponseDto);
    }

    public async Task<MedicamentoResponseDto> CreateAsync(MedicamentoRequestDto dto)
    {
        var medicamento = new Medicamento
        {
            Nome = dto.Nome
        };

        var created = await _repository.CreateAsync(medicamento);
        return ToResponseDto(created);
    }

    public async Task<MedicamentoResponseDto> UpdateAsync(int id, MedicamentoRequestDto dto)
    {
        var medicamento = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Medicamento com ID {id} não encontrado.");

        medicamento.Nome = dto.Nome;

        var updated = await _repository.UpdateAsync(medicamento);
        return ToResponseDto(updated);
    }

    public async Task DeleteAsync(int id)
    {
        var medicamento = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Medicamento com ID {id} não encontrado.");
        await _repository.DeleteAsync(medicamento);
    }

    private static MedicamentoResponseDto ToResponseDto(Medicamento m) => new()
    {
        Id = m.Id,
        Nome = m.Nome
    };
}
