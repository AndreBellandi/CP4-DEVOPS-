using ClyvoVetApi.DTOs.Request;
using ClyvoVetApi.Exceptions;
using ClyvoVetApi.Models;
using ClyvoVetApi.Repositories.Interfaces;
using ClyvoVetApi.Services;
using Moq;
using Xunit;

namespace ClyvoVetApi.Tests.Services;

public class DonoServiceTests
{
    private readonly Mock<IDonoRepository> _repositoryMock;
    private readonly DonoService _service;

    public DonoServiceTests()
    {
        _repositoryMock = new Mock<IDonoRepository>();
        _service = new DonoService(_repositoryMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPagedResponse()
    {
        // Arrange
        var page = 1;
        var pageSize = 10;
        var donos = new List<Dono>
        {
            new() { Id = 1, Nome = "Dono 1", Email = "dono1@email.com", Telefone = "11999999999" },
            new() { Id = 2, Nome = "Dono 2", Email = "dono2@email.com", Telefone = "11988888888" }
        };
        _repositoryMock.Setup(r => r.GetAllAsync(page, pageSize))
            .ReturnsAsync((donos, 2));

        // Act
        var result = await _service.GetAllAsync(page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(page, result.Page);
        Assert.Equal(pageSize, result.PageSize);
        Assert.Equal(2, result.TotalItems);
        Assert.Equal(2, result.Data.Count());
    }

    [Fact]
    public async Task GetByIdAsync_WhenDonoExists_ReturnsDonoDetails()
    {
        // Arrange
        var id = 1;
        var dono = new Dono { Id = id, Nome = "Dono 1", Email = "dono1@email.com" };
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(dono);

        // Act
        var result = await _service.GetByIdAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("Dono 1", result.Nome);
    }

    [Fact]
    public async Task GetByIdAsync_WhenDonoDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var id = 99;
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Dono?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetByIdAsync(id));
    }

    [Fact]
    public async Task GetByEmailAsync_WhenDonoExists_ReturnsDonoDetails()
    {
        // Arrange
        var email = "dono1@email.com";
        var dono = new Dono { Id = 1, Nome = "Dono 1", Email = email };
        _repositoryMock.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync(dono);

        // Act
        var result = await _service.GetByEmailAsync(email);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(email, result.Email);
    }

    [Fact]
    public async Task GetByEmailAsync_WhenDonoDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var email = "notfound@email.com";
        _repositoryMock.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync((Dono?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetByEmailAsync(email));
    }

    [Fact]
    public async Task CreateAsync_WhenEmailAlreadyExists_ThrowsBusinessException()
    {
        // Arrange
        var dto = new DonoRequestDto { Nome = "Novo", Email = "dono1@email.com", Telefone = "11999999999" };
        _repositoryMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync(new Dono());

        // Act & Assert
        await Assert.ThrowsAsync<BusinessException>(() => _service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_WhenSuccessful_ReturnsDetails()
    {
        // Arrange
        var dto = new DonoRequestDto { Nome = "Novo Dono", Email = "novo@email.com", Telefone = "11999999999" };
        _repositoryMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync((Dono?)null);
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<Dono>()))
            .ReturnsAsync((Dono d) => { d.Id = 10; return d; });

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.Id);
        Assert.Equal("Novo Dono", result.Nome);
    }

    [Fact]
    public async Task UpdateAsync_WhenDonoDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var id = 99;
        var dto = new DonoRequestDto { Nome = "Nome", Email = "email@email.com", Telefone = "123" };
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Dono?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateAsync(id, dto));
    }

    [Fact]
    public async Task UpdateAsync_WhenEmailAlreadyExistsForAnotherDono_ThrowsBusinessException()
    {
        // Arrange
        var id = 1;
        var dono = new Dono { Id = id, Nome = "Dono 1", Email = "dono1@email.com" };
        var dto = new DonoRequestDto { Nome = "Dono Alterado", Email = "outro@email.com" };
        
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(dono);
        _repositoryMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync(new Dono { Id = 2, Email = dto.Email });

        // Act & Assert
        await Assert.ThrowsAsync<BusinessException>(() => _service.UpdateAsync(id, dto));
    }

    [Fact]
    public async Task UpdateAsync_WhenSuccessful_ReturnsDetails()
    {
        // Arrange
        var id = 1;
        var dono = new Dono { Id = id, Nome = "Dono 1", Email = "dono1@email.com" };
        var dto = new DonoRequestDto { Nome = "Dono 1 Alterado", Email = "dono1@email.com", Telefone = "11999999999" };

        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(dono);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Dono>())).ReturnsAsync((Dono d) => d);

        // Act
        var result = await _service.UpdateAsync(id, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Dono 1 Alterado", result.Nome);
    }

    [Fact]
    public async Task DeleteAsync_WhenDonoDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var id = 99;
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Dono?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteAsync(id));
    }

    [Fact]
    public async Task DeleteAsync_WhenSuccessful_CallsRepositoryDelete()
    {
        // Arrange
        var id = 1;
        var dono = new Dono { Id = id, Nome = "Dono 1" };
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(dono);

        // Act
        await _service.DeleteAsync(id);

        // Assert
        _repositoryMock.Verify(r => r.DeleteAsync(dono), Times.Once);
    }
}
