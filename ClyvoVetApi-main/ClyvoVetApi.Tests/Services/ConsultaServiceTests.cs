using ClyvoVetApi.DTOs.Request;
using ClyvoVetApi.Exceptions;
using ClyvoVetApi.Models;
using ClyvoVetApi.Repositories.Interfaces;
using ClyvoVetApi.Services;
using Moq;
using Xunit;

namespace ClyvoVetApi.Tests.Services;

public class ConsultaServiceTests
{
    private readonly Mock<IConsultaRepository> _repositoryMock;
    private readonly Mock<IPetRepository> _petRepositoryMock;
    private readonly Mock<IFuncionarioRepository> _funcionarioRepositoryMock;
    private readonly ConsultaService _service;

    public ConsultaServiceTests()
    {
        _repositoryMock = new Mock<IConsultaRepository>();
        _petRepositoryMock = new Mock<IPetRepository>();
        _funcionarioRepositoryMock = new Mock<IFuncionarioRepository>();
        _service = new ConsultaService(_repositoryMock.Object, _petRepositoryMock.Object, _funcionarioRepositoryMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPagedResponse()
    {
        // Arrange
        var page = 1;
        var pageSize = 10;
        var consultas = new List<Consulta>
        {
            new() { Id = 1, Tipo = "Geral", Valor = 150.00m, Descricao = "Rotina", Status = "R", Data = DateTime.Now, PetId = 1, FuncionarioId = 1 },
            new() { Id = 2, Tipo = "Retorno", Valor = 100.00m, Descricao = "Checkup", Status = "A", Data = DateTime.Now, PetId = 1, FuncionarioId = 1 }
        };
        _repositoryMock.Setup(r => r.GetAllAsync(page, pageSize))
            .ReturnsAsync((consultas, 2));

        // Act
        var result = await _service.GetAllAsync(page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(page, result.Page);
        Assert.Equal(2, result.TotalItems);
        Assert.Equal(2, result.Data.Count());
    }

    [Fact]
    public async Task GetByIdAsync_WhenConsultaExists_ReturnsResponseDto()
    {
        // Arrange
        var id = 1;
        var consulta = new Consulta 
        { 
            Id = id, 
            Tipo = "Geral", 
            Valor = 150.00m, 
            Descricao = "Rotina", 
            Status = "R", 
            Data = DateTime.Now,
            Pet = new Pet { Id = 1, Nome = "Rex" } 
        };
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(consulta);

        // Act
        var result = await _service.GetByIdAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("Rex", result.PetNome);
    }

    [Fact]
    public async Task GetByIdAsync_WhenConsultaDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var id = 99;
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Consulta?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetByIdAsync(id));
    }

    [Fact]
    public async Task GetByPeriodoAsync_WhenStartAfterEnd_ThrowsBusinessException()
    {
        // Arrange
        var inicio = DateTime.Now.AddDays(1);
        var fim = DateTime.Now;

        // Act & Assert
        await Assert.ThrowsAsync<BusinessException>(() => _service.GetByPeriodoAsync(inicio, fim));
    }

    [Fact]
    public async Task CreateAsync_WhenPetDoesNotExist_ThrowsBusinessException()
    {
        // Arrange
        var dto = new ConsultaRequestDto { Tipo = "Geral", Valor = 100m, Descricao = "Rotina", Status = "A", Data = DateTime.Now, PetId = 99, FuncionarioId = 1 };
        _petRepositoryMock.Setup(p => p.GetByIdAsync(dto.PetId)).ReturnsAsync((Pet?)null);

        // Act & Assert
        await Assert.ThrowsAsync<BusinessException>(() => _service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_WhenFuncionarioDoesNotExist_ThrowsBusinessException()
    {
        // Arrange
        var dto = new ConsultaRequestDto { Tipo = "Geral", Valor = 100m, Descricao = "Rotina", Status = "A", Data = DateTime.Now, PetId = 1, FuncionarioId = 99 };
        _petRepositoryMock.Setup(p => p.GetByIdAsync(dto.PetId)).ReturnsAsync(new Pet { Id = 1 });
        _funcionarioRepositoryMock.Setup(f => f.GetByIdAsync(dto.FuncionarioId)).ReturnsAsync((Funcionario?)null);

        // Act & Assert
        await Assert.ThrowsAsync<BusinessException>(() => _service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_WhenSuccessful_ReturnsResponseDto()
    {
        // Arrange
        var dto = new ConsultaRequestDto { Tipo = "Geral", Valor = 150m, Descricao = "Rotina", Status = "A", Data = DateTime.Now, PetId = 1, FuncionarioId = 1 };
        var pet = new Pet { Id = 1, Nome = "Rex" };
        var funcionario = new Funcionario { Id = 1, Nome = "Dr. Silva" };

        _petRepositoryMock.Setup(p => p.GetByIdAsync(dto.PetId)).ReturnsAsync(pet);
        _funcionarioRepositoryMock.Setup(f => f.GetByIdAsync(dto.FuncionarioId)).ReturnsAsync(funcionario);
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<Consulta>()))
            .ReturnsAsync((Consulta c) => { c.Id = 10; return c; });

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.Id);
        Assert.Equal("Rex", result.PetNome);
    }

    [Fact]
    public async Task UpdateAsync_WhenConsultaDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var id = 99;
        var dto = new ConsultaRequestDto { Tipo = "Geral", Valor = 150m, Descricao = "Rotina", Status = "A", Data = DateTime.Now, PetId = 1, FuncionarioId = 1 };
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Consulta?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateAsync(id, dto));
    }

    [Fact]
    public async Task UpdateAsync_WhenPetDoesNotExist_ThrowsBusinessException()
    {
        // Arrange
        var id = 1;
        var consulta = new Consulta { Id = id, Tipo = "Geral", Valor = 150m, Descricao = "Rotina", Status = "A", Data = DateTime.Now, PetId = 1, FuncionarioId = 1 };
        var dto = new ConsultaRequestDto { Tipo = "Geral", Valor = 150m, Descricao = "Rotina", Status = "A", Data = DateTime.Now, PetId = 99, FuncionarioId = 1 };

        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(consulta);
        _petRepositoryMock.Setup(p => p.GetByIdAsync(dto.PetId)).ReturnsAsync((Pet?)null);

        // Act & Assert
        await Assert.ThrowsAsync<BusinessException>(() => _service.UpdateAsync(id, dto));
    }

    [Fact]
    public async Task UpdateAsync_WhenFuncionarioDoesNotExist_ThrowsBusinessException()
    {
        // Arrange
        var id = 1;
        var consulta = new Consulta { Id = id, Tipo = "Geral", Valor = 150m, Descricao = "Rotina", Status = "A", Data = DateTime.Now, PetId = 1, FuncionarioId = 1 };
        var dto = new ConsultaRequestDto { Tipo = "Geral", Valor = 150m, Descricao = "Rotina", Status = "A", Data = DateTime.Now, PetId = 1, FuncionarioId = 99 };

        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(consulta);
        _petRepositoryMock.Setup(p => p.GetByIdAsync(dto.PetId)).ReturnsAsync(new Pet { Id = 1 });
        _funcionarioRepositoryMock.Setup(f => f.GetByIdAsync(dto.FuncionarioId)).ReturnsAsync((Funcionario?)null);

        // Act & Assert
        await Assert.ThrowsAsync<BusinessException>(() => _service.UpdateAsync(id, dto));
    }

    [Fact]
    public async Task UpdateAsync_WhenSuccessful_ReturnsResponseDto()
    {
        // Arrange
        var id = 1;
        var consulta = new Consulta { Id = id, Tipo = "Geral", Valor = 150m, Descricao = "Rotina", Status = "A", Data = DateTime.Now, PetId = 1, FuncionarioId = 1 };
        var pet = new Pet { Id = 1, Nome = "Rex" };
        var funcionario = new Funcionario { Id = 1, Nome = "Dr. Silva" };
        var dto = new ConsultaRequestDto { Tipo = "Geral", Valor = 200m, Descricao = "Rotina Alterada", Status = "R", Data = DateTime.Now, PetId = 1, FuncionarioId = 1 };

        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(consulta);
        _petRepositoryMock.Setup(p => p.GetByIdAsync(1)).ReturnsAsync(pet);
        _funcionarioRepositoryMock.Setup(f => f.GetByIdAsync(1)).ReturnsAsync(funcionario);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Consulta>())).ReturnsAsync((Consulta c) => c);

        // Act
        var result = await _service.UpdateAsync(id, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Rotina Alterada", result.Descricao);
        Assert.Equal("Rex", result.PetNome);
    }

    [Fact]
    public async Task DeleteAsync_WhenConsultaDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var id = 99;
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Consulta?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteAsync(id));
    }

    [Fact]
    public async Task DeleteAsync_WhenSuccessful_CallsRepositoryDelete()
    {
        // Arrange
        var id = 1;
        var consulta = new Consulta { Id = id, Descricao = "Rotina" };
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(consulta);

        // Act
        await _service.DeleteAsync(id);

        // Assert
        _repositoryMock.Verify(r => r.DeleteAsync(consulta), Times.Once);
    }
}
