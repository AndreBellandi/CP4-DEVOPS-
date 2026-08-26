using ClyvoVetApi.DTOs.Request;
using ClyvoVetApi.DTOs.Response;
using ClyvoVetApi.Exceptions;
using ClyvoVetApi.Models;
using ClyvoVetApi.Repositories.Interfaces;
using ClyvoVetApi.Services.Interfaces;

namespace ClyvoVetApi.Services;

public class ConsultaService(
    IConsultaRepository repository,
    IPetRepository petRepository,
    IFuncionarioRepository funcionarioRepository) : IConsultaService
{
    private readonly IConsultaRepository _repository = repository;
    private readonly IPetRepository _petRepository = petRepository;
    private readonly IFuncionarioRepository _funcionarioRepository = funcionarioRepository;

    public async Task<PagedResponseDto<ConsultaResponseDto>> GetAllAsync(int page, int pageSize)
    {
        var (items, total) = await _repository.GetAllAsync(page, pageSize);
        return new PagedResponseDto<ConsultaResponseDto>
        {
            Data = items.Select(ToResponseDto),
            Page = page,
            PageSize = pageSize,
            TotalItems = total
        };
    }

    public async Task<ConsultaResponseDto> GetByIdAsync(int id)
    {
        var consulta = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Consulta com ID {id} não encontrada.");
        return ToResponseDto(consulta);
    }

    public async Task<IEnumerable<ConsultaResponseDto>> GetByFuncionarioIdAsync(int funcionarioId)
    {
        var items = await _repository.GetByFuncionarioIdAsync(funcionarioId);
        return items.Select(ToResponseDto);
    }

    public async Task<IEnumerable<ConsultaResponseDto>> GetByPeriodoAsync(DateTime inicio, DateTime fim)
    {
        if (inicio > fim)
            throw new BusinessException("A data de início do período não pode ser posterior à data de fim.");

        var items = await _repository.GetByPeriodoAsync(inicio, fim);
        return items.Select(ToResponseDto);
    }

    public async Task<ConsultaResponseDto> CreateAsync(ConsultaRequestDto dto)
    {
        var pet = await _petRepository.GetByIdAsync(dto.PetId)
            ?? throw new BusinessException($"Pet associado com ID {dto.PetId} não encontrado.");

        var funcionario = await _funcionarioRepository.GetByIdAsync(dto.FuncionarioId)
            ?? throw new BusinessException($"Funcionário associado com ID {dto.FuncionarioId} não encontrado.");

        var consulta = new Consulta
        {
            Tipo = dto.Tipo,
            Valor = dto.Valor,
            Descricao = dto.Descricao,
            Status = dto.Status,
            Data = dto.Data,
            PetId = dto.PetId,
            FuncionarioId = dto.FuncionarioId
        };

        var created = await _repository.CreateAsync(consulta);
        created.Pet = pet;
        created.Funcionario = funcionario;

        return ToResponseDto(created);
    }

    public async Task<ConsultaResponseDto> UpdateAsync(int id, ConsultaRequestDto dto)
    {
        var consulta = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Consulta com ID {id} não encontrada.");

        var pet = await _petRepository.GetByIdAsync(dto.PetId)
            ?? throw new BusinessException($"Pet associado com ID {dto.PetId} não encontrado.");

        var funcionario = await _funcionarioRepository.GetByIdAsync(dto.FuncionarioId)
            ?? throw new BusinessException($"Funcionário associado com ID {dto.FuncionarioId} não encontrado.");

        consulta.Tipo = dto.Tipo;
        consulta.Valor = dto.Valor;
        consulta.Descricao = dto.Descricao;
        consulta.Status = dto.Status;
        consulta.Data = dto.Data;
        consulta.PetId = dto.PetId;
        consulta.FuncionarioId = dto.FuncionarioId;

        var updated = await _repository.UpdateAsync(consulta);
        updated.Pet = pet;
        updated.Funcionario = funcionario;

        return ToResponseDto(updated);
    }

    public async Task DeleteAsync(int id)
    {
        var consulta = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Consulta com ID {id} não encontrada.");
        await _repository.DeleteAsync(consulta);
    }

    private static ConsultaResponseDto ToResponseDto(Consulta c) => new()
    {
        Id = c.Id,
        Tipo = c.Tipo,
        Valor = c.Valor,
        Descricao = c.Descricao,
        Status = c.Status,
        Data = c.Data,
        PetId = c.PetId,
        PetNome = c.Pet?.Nome ?? string.Empty,
        FuncionarioId = c.FuncionarioId,
        FuncionarioNome = c.Funcionario?.Nome ?? string.Empty
    };
}
