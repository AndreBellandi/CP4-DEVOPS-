using ClyvoVetApi.DTOs.Request;
using ClyvoVetApi.Exceptions;
using ClyvoVetApi.Models;
using ClyvoVetApi.Repositories.Interfaces;
using ClyvoVetApi.Services;
using Moq;
using Xunit;

namespace ClyvoVetApi.Tests.Services;

public class PetServiceTests
{
    private readonly Mock<IPetRepository> _repositoryMock;
    private readonly Mock<IDonoRepository> _donoRepositoryMock;
    private readonly PetService _service;

    public PetServiceTests()
    {
        _repositoryMock = new Mock<IPetRepository>();
        _donoRepositoryMock = new Mock<IDonoRepository>();
        _service = new PetService(_repositoryMock.Object, _donoRepositoryMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPagedResponse()
    {
        // Arrange
        var page = 1;
        var pageSize = 10;
        var pets = new List<Pet>
        {
            new() { Id = 1, Nome = "Rex", Especie = "Cachorro", Raca = "Vira-lata", DonoId = 1, Peso = 10.5m },
            new() { Id = 2, Nome = "Mingau", Especie = "Gato", Raca = "Persa", DonoId = 1, Peso = 4.2m }
        };
        _repositoryMock.Setup(r => r.GetAllAsync(page, pageSize))
            .ReturnsAsync((pets, 2));

        // Act
        var result = await _service.GetAllAsync(page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(page, result.Page);
        Assert.Equal(2, result.TotalItems);
        Assert.Equal(2, result.Data.Count());
    }

    [Fact]
    public async Task GetByIdAsync_WhenPetExists_ReturnsPetDetails()
    {
        // Arrange
        var id = 1;
        var pet = new Pet { Id = id, Nome = "Rex", Especie = "Cachorro", Peso = 10.5m, Dono = new Dono { Id = 1, Nome = "Dono 1" } };
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(pet);

        // Act
        var result = await _service.GetByIdAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("Rex", result.Nome);
        Assert.Equal("Dono 1", result.DonoNome);
    }

    [Fact]
    public async Task GetByIdAsync_WhenPetDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var id = 99;
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Pet?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetByIdAsync(id));
    }

    [Fact]
    public async Task CreateAsync_WhenDonoDoesNotExist_ThrowsBusinessException()
    {
        // Arrange
        var dto = new PetRequestDto { Nome = "Rex", Especie = "Cachorro", Raca = "Labrador", DonoId = 99, Peso = 10.5m };
        _donoRepositoryMock.Setup(t => t.GetByIdAsync(dto.DonoId)).ReturnsAsync((Dono?)null);

        // Act & Assert
        await Assert.ThrowsAsync<BusinessException>(() => _service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_WhenSuccessful_ReturnsDetails()
    {
        // Arrange
        var dto = new PetRequestDto { Nome = "Rex", Especie = "Cachorro", Raca = "Labrador", DonoId = 1, Peso = 10.5m };
        var dono = new Dono { Id = 1, Nome = "Dono 1" };
        
        _donoRepositoryMock.Setup(t => t.GetByIdAsync(dto.DonoId)).ReturnsAsync(dono);
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<Pet>()))
            .ReturnsAsync((Pet p) => { p.Id = 5; return p; });

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Id);
        Assert.Equal("Rex", result.Nome);
        Assert.Equal("Dono 1", result.DonoNome);
    }

    [Fact]
    public async Task UpdateAsync_WhenPetDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var id = 99;
        var dto = new PetRequestDto { Nome = "Rex", DonoId = 1, Peso = 10.5m };
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Pet?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateAsync(id, dto));
    }

    [Fact]
    public async Task UpdateAsync_WhenDonoDoesNotExist_ThrowsBusinessException()
    {
        // Arrange
        var id = 1;
        var pet = new Pet { Id = id, Nome = "Rex", DonoId = 1, Peso = 10.5m };
        var dto = new PetRequestDto { Nome = "Rex Novo", DonoId = 99, Peso = 12.0m };

        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(pet);
        _donoRepositoryMock.Setup(t => t.GetByIdAsync(dto.DonoId)).ReturnsAsync((Dono?)null);

        // Act & Assert
        await Assert.ThrowsAsync<BusinessException>(() => _service.UpdateAsync(id, dto));
    }

    [Fact]
    public async Task UpdateAsync_WhenSuccessful_ReturnsDetails()
    {
        // Arrange
        var id = 1;
        var pet = new Pet { Id = id, Nome = "Rex", DonoId = 1, Peso = 10.5m };
        var dono = new Dono { Id = 1, Nome = "Dono 1" };
        var dto = new PetRequestDto { Nome = "Rex Alterado", Especie = "Cachorro", Raca = "Golden", DonoId = 1, Peso = 11.5m };

        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(pet);
        _donoRepositoryMock.Setup(t => t.GetByIdAsync(1)).ReturnsAsync(dono);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Pet>())).ReturnsAsync((Pet p) => p);

        // Act
        var result = await _service.UpdateAsync(id, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Rex Alterado", result.Nome);
    }

    [Fact]
    public async Task DeleteAsync_WhenPetDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var id = 99;
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Pet?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteAsync(id));
    }

    [Fact]
    public async Task DeleteAsync_WhenSuccessful_CallsRepositoryDelete()
    {
        // Arrange
        var id = 1;
        var pet = new Pet { Id = id, Nome = "Rex" };
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(pet);

        // Act
        await _service.DeleteAsync(id);

        // Assert
        _repositoryMock.Verify(r => r.DeleteAsync(pet), Times.Once);
    }
}
