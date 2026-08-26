using ClyvoVetApi.DTOs.Request;
using ClyvoVetApi.DTOs.Response;
using ClyvoVetApi.Exceptions;
using ClyvoVetApi.Models;
using ClyvoVetApi.Repositories.Interfaces;
using ClyvoVetApi.Services.Interfaces;

namespace ClyvoVetApi.Services;

public class FuncionarioService(IFuncionarioRepository repository) : IFuncionarioService
{
    private readonly IFuncionarioRepository _repository = repository;

    public async Task<PagedResponseDto<FuncionarioResponseDto>> GetAllAsync(int page, int pageSize)
    {
        var (items, total) = await _repository.GetAllAsync(page, pageSize);
        return new PagedResponseDto<FuncionarioResponseDto>
        {
            Data = items.Select(ToResponseDto),
            Page = page,
            PageSize = pageSize,
            TotalItems = total
        };
    }

    public async Task<FuncionarioResponseDto> GetByIdAsync(int id)
    {
        var funcionario = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Funcionário com ID {id} não encontrado.");
        return ToResponseDto(funcionario);
    }

    public async Task<FuncionarioResponseDto> GetByEmailAsync(string email)
    {
        var funcionario = await _repository.GetByEmailAsync(email)
            ?? throw new NotFoundException($"Funcionário com e-mail '{email}' não encontrado.");
        return ToResponseDto(funcionario);
    }

    public async Task<IEnumerable<FuncionarioResponseDto>> GetBySetorAsync(string setor)
    {
        var items = await _repository.GetBySetorAsync(setor);
        return items.Select(ToResponseDto);
    }

    public async Task<IEnumerable<FuncionarioResponseDto>> GetByCargoAsync(string cargo)
    {
        var items = await _repository.GetByCargoAsync(cargo);
        return items.Select(ToResponseDto);
    }

    public async Task<FuncionarioResponseDto> CreateAsync(FuncionarioRequestDto dto)
    {
        var existing = await _repository.GetByEmailAsync(dto.Email);
        if (existing != null)
            throw new BusinessException("Já existe um funcionário cadastrado com este e-mail.");

        var funcionario = new Funcionario
        {
            Nome = dto.Nome,
            Setor = dto.Setor,
            Cargo = dto.Cargo,
            Email = dto.Email,
            Telefone = dto.Telefone
        };

        var created = await _repository.CreateAsync(funcionario);
        return ToResponseDto(created);
    }

    public async Task<FuncionarioResponseDto> UpdateAsync(int id, FuncionarioRequestDto dto)
    {
        var funcionario = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Funcionário com ID {id} não encontrado.");

        if (funcionario.Email != dto.Email)
        {
            var existing = await _repository.GetByEmailAsync(dto.Email);
            if (existing != null)
                throw new BusinessException("Já existe outro funcionário cadastrado com este e-mail.");
        }

        funcionario.Nome = dto.Nome;
        funcionario.Setor = dto.Setor;
        funcionario.Cargo = dto.Cargo;
        funcionario.Email = dto.Email;
        funcionario.Telefone = dto.Telefone;

        var updated = await _repository.UpdateAsync(funcionario);
        return ToResponseDto(updated);
    }

    public async Task DeleteAsync(int id)
    {
        var funcionario = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Funcionário com ID {id} não encontrado.");
        await _repository.DeleteAsync(funcionario);
    }

    private static FuncionarioResponseDto ToResponseDto(Funcionario f) => new()
    {
        Id = f.Id,
        Nome = f.Nome,
        Setor = f.Setor,
        Cargo = f.Cargo,
        Email = f.Email,
        Telefone = f.Telefone
    };
}
