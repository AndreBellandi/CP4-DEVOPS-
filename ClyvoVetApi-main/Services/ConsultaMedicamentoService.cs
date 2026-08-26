using ClyvoVetApi.DTOs.Request;
using ClyvoVetApi.DTOs.Response;
using ClyvoVetApi.Exceptions;
using ClyvoVetApi.Models;
using ClyvoVetApi.Repositories.Interfaces;
using ClyvoVetApi.Services.Interfaces;

namespace ClyvoVetApi.Services;

public class ConsultaMedicamentoService(
    IConsultaMedicamentoRepository repository,
    IConsultaRepository consultaRepository,
    IMedicamentoRepository medicamentoRepository) : IConsultaMedicamentoService
{
    private readonly IConsultaMedicamentoRepository _repository = repository;
    private readonly IConsultaRepository _consultaRepository = consultaRepository;
    private readonly IMedicamentoRepository _medicamentoRepository = medicamentoRepository;

    public async Task<PagedResponseDto<ConsultaMedicamentoResponseDto>> GetAllAsync(int page, int pageSize)
    {
        var (items, total) = await _repository.GetAllAsync(page, pageSize);
        return new PagedResponseDto<ConsultaMedicamentoResponseDto>
        {
            Data = items.Select(ToResponseDto),
            Page = page,
            PageSize = pageSize,
            TotalItems = total
        };
    }

    public async Task<ConsultaMedicamentoResponseDto> GetByIdAsync(int id)
    {
        var cm = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Prescrição com ID {id} não encontrada.");
        return ToResponseDto(cm);
    }

    public async Task<IEnumerable<ConsultaMedicamentoResponseDto>> GetByConsultaIdAsync(int consultaId)
    {
        var items = await _repository.GetByConsultaIdAsync(consultaId);
        return items.Select(ToResponseDto);
    }

    public async Task<ConsultaMedicamentoResponseDto> CreateAsync(ConsultaMedicamentoRequestDto dto)
    {
        var consulta = await _consultaRepository.GetByIdAsync(dto.ConsultaId)
            ?? throw new BusinessException($"Consulta associada com ID {dto.ConsultaId} não encontrada.");

        var medicamento = await _medicamentoRepository.GetByIdAsync(dto.MedicamentoId)
            ?? throw new BusinessException($"Medicamento associado com ID {dto.MedicamentoId} não encontrado.");

        var cm = new ConsultaMedicamento
        {
            ConsultaId = dto.ConsultaId,
            MedicamentoId = dto.MedicamentoId,
            Dosagem = dto.Dosagem
        };

        var created = await _repository.CreateAsync(cm);
        created.Consulta = consulta;
        created.Medicamento = medicamento;

        return ToResponseDto(created);
    }

    public async Task<ConsultaMedicamentoResponseDto> UpdateAsync(int id, ConsultaMedicamentoRequestDto dto)
    {
        var cm = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Prescrição com ID {id} não encontrada.");

        var consulta = await _consultaRepository.GetByIdAsync(dto.ConsultaId)
            ?? throw new BusinessException($"Consulta associada com ID {dto.ConsultaId} não encontrada.");

        var medicamento = await _medicamentoRepository.GetByIdAsync(dto.MedicamentoId)
            ?? throw new BusinessException($"Medicamento associado com ID {dto.MedicamentoId} não encontrado.");

        cm.ConsultaId = dto.ConsultaId;
        cm.MedicamentoId = dto.MedicamentoId;
        cm.Dosagem = dto.Dosagem;

        var updated = await _repository.UpdateAsync(cm);
        updated.Consulta = consulta;
        updated.Medicamento = medicamento;

        return ToResponseDto(updated);
    }

    public async Task DeleteAsync(int id)
    {
        var cm = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Prescrição com ID {id} não encontrada.");
        await _repository.DeleteAsync(cm);
    }

    private static ConsultaMedicamentoResponseDto ToResponseDto(ConsultaMedicamento cm) => new()
    {
        Id = cm.Id,
        ConsultaId = cm.ConsultaId,
        MedicamentoId = cm.MedicamentoId,
        MedicamentoNome = cm.Medicamento?.Nome ?? string.Empty,
        Dosagem = cm.Dosagem
    };
}
