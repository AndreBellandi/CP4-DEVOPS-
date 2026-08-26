using ClyvoVetApi.DTOs.Request;
using ClyvoVetApi.Exceptions;
using ClyvoVetApi.Models;
using ClyvoVetApi.Repositories.Interfaces;
using ClyvoVetApi.Services;
using Moq;
using Xunit;

namespace ClyvoVetApi.Tests.Services;

public class VacinaServiceTests
{
    private readonly Mock<IVacinaRepository> _repositoryMock;
    private readonly Mock<IPetRepository> _petRepositoryMock;
    private readonly VacinaService _service;

    public VacinaServiceTests()
    {
        _repositoryMock = new Mock<IVacinaRepository>();
        _petRepositoryMock = new Mock<IPetRepository>();
        _service = new VacinaService(_repositoryMock.Object, _petRepositoryMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPagedResponse()
    {
        // Arrange
        var page = 1;
        var pageSize = 10;
        var vacinas = new List<Vacina>
        {
            new() { Id = 1, Nome = "Antirrábica", Status = "A", Data = DateTime.Now, PetId = 1 },
            new() { Id = 2, Nome = "V10", Status = "P", Data = DateTime.Now.AddDays(5), PetId = 1 }
        };
        _repositoryMock.Setup(r => r.GetAllAsync(page, pageSize))
            .ReturnsAsync((vacinas, 2));

        // Act
        var result = await _service.GetAllAsync(page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(page, result.Page);
        Assert.Equal(2, result.TotalItems);
        Assert.Equal(2, result.Data.Count());
    }

    [Fact]
    public async Task GetByIdAsync_WhenVacinaExists_ReturnsResponseDto()
    {
        // Arrange
        var id = 1;
        var vacina = new Vacina { Id = id, Nome = "Antirrábica", Status = "A", Data = DateTime.Now, Pet = new Pet { Id = 1, Nome = "Rex" } };
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(vacina);

        // Act
        var result = await _service.GetByIdAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("Rex", result.PetNome);
    }

    [Fact]
    public async Task GetByIdAsync_WhenVacinaDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var id = 99;
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Vacina?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetByIdAsync(id));
    }

    [Fact]
    public async Task GetProximasAsync_WhenDaysIsZeroOrLess_ThrowsBusinessException()
    {
        // Arrange
        var dias = 0;

        // Act & Assert
        await Assert.ThrowsAsync<BusinessException>(() => _service.GetProximasAsync(dias));
    }

    [Fact]
    public async Task CreateAsync_WhenPetDoesNotExist_ThrowsBusinessException()
    {
        // Arrange
        var dto = new VacinaRequestDto { Nome = "Antirrábica", Status = "P", Data = DateTime.Now, PetId = 99 };
        _petRepositoryMock.Setup(p => p.GetByIdAsync(dto.PetId)).ReturnsAsync((Pet?)null);

        // Act & Assert
        await Assert.ThrowsAsync<BusinessException>(() => _service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_WhenSuccessful_ReturnsResponseDto()
    {
        // Arrange
        var dto = new VacinaRequestDto { Nome = "Antirrábica", Status = "A", Data = DateTime.Now, PetId = 1 };
        var pet = new Pet { Id = 1, Nome = "Rex" };

        _petRepositoryMock.Setup(p => p.GetByIdAsync(dto.PetId)).ReturnsAsync(pet);
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<Vacina>()))
            .ReturnsAsync((Vacina v) => { v.Id = 10; return v; });

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.Id);
        Assert.Equal("Rex", result.PetNome);
    }

    [Fact]
    public async Task UpdateAsync_WhenVacinaDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var id = 99;
        var dto = new VacinaRequestDto { Nome = "Antirrábica", Status = "P", Data = DateTime.Now, PetId = 1 };
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Vacina?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateAsync(id, dto));
    }

    [Fact]
    public async Task UpdateAsync_WhenPetDoesNotExist_ThrowsBusinessException()
    {
        // Arrange
        var id = 1;
        var vacina = new Vacina { Id = id, Nome = "Antirrábica", Status = "P", Data = DateTime.Now, PetId = 1 };
        var dto = new VacinaRequestDto { Nome = "Antirrábica", Status = "A", Data = DateTime.Now, PetId = 99 };

        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(vacina);
        _petRepositoryMock.Setup(p => p.GetByIdAsync(dto.PetId)).ReturnsAsync((Pet?)null);

        // Act & Assert
        await Assert.ThrowsAsync<BusinessException>(() => _service.UpdateAsync(id, dto));
    }

    [Fact]
    public async Task UpdateAsync_WhenSuccessful_ReturnsResponseDto()
    {
        // Arrange
        var id = 1;
        var vacina = new Vacina { Id = id, Nome = "Antirrábica", Status = "P", Data = DateTime.Now, PetId = 1 };
        var pet = new Pet { Id = 1, Nome = "Rex" };
        var dto = new VacinaRequestDto { Nome = "Antirrábica Alterada", Status = "A", Data = DateTime.Now, PetId = 1 };

        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(vacina);
        _petRepositoryMock.Setup(p => p.GetByIdAsync(1)).ReturnsAsync(pet);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Vacina>())).ReturnsAsync((Vacina v) => v);

        // Act
        var result = await _service.UpdateAsync(id, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Antirrábica Alterada", result.Nome);
        Assert.Equal("Rex", result.PetNome);
    }

    [Fact]
    public async Task DeleteAsync_WhenVacinaDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var id = 99;
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Vacina?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteAsync(id));
    }

    [Fact]
    public async Task DeleteAsync_WhenSuccessful_CallsRepositoryDelete()
    {
        // Arrange
        var id = 1;
        var vacina = new Vacina { Id = id, Nome = "Antirrábica" };
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(vacina);

        // Act
        await _service.DeleteAsync(id);

        // Assert
        _repositoryMock.Verify(r => r.DeleteAsync(vacina), Times.Once);
    }
}
