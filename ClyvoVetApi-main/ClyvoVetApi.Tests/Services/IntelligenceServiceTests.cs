using ClyvoVetApi.Exceptions;
using ClyvoVetApi.Models;
using ClyvoVetApi.Repositories.Interfaces;
using ClyvoVetApi.Services;
using Moq;
using Xunit;

namespace ClyvoVetApi.Tests.Services;

public class IntelligenceServiceTests
{
    private readonly Mock<IPetRepository> _petRepositoryMock;
    private readonly IntelligenceService _service;

    public IntelligenceServiceTests()
    {
        _petRepositoryMock = new Mock<IPetRepository>();
        _service = new IntelligenceService(_petRepositoryMock.Object);
    }

    [Fact]
    public async Task GetIntelligencePreventivaAsync_WhenPetDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var petId = 99;
        _petRepositoryMock.Setup(r => r.GetByIdAsync(petId)).ReturnsAsync((Pet?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _service.GetIntelligencePreventivaAsync(petId));
        Assert.Contains($"Pet com ID {petId} não foi encontrado", exception.Message);
    }

    [Fact]
    public async Task GetIntelligencePreventivaAsync_WhenPetHasPerfectHealth_ReturnsScore100AndExcellent()
    {
        // Arrange
        var petId = 1;
        var pet = new Pet
        {
            Id = petId,
            Nome = "Bidu",
            Consultas = new List<Consulta> { new() { Status = "R", Data = DateTime.Today } },
            Vacinas = new List<Vacina> { new() { Status = "A", Data = DateTime.Today.AddMonths(-6) } }
        };
        _petRepositoryMock.Setup(r => r.GetByIdAsync(petId)).ReturnsAsync(pet);

        // Act
        var result = await _service.GetIntelligencePreventivaAsync(petId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(100, result.Score);
        Assert.Equal("EXCELENTE", result.Status);
        Assert.Contains("Excelente!", result.Recomendacoes.First());
    }

    [Fact]
    public async Task GetIntelligencePreventivaAsync_WhenPetHasOverdueVaccine_DecreasesScoreCorrectly()
    {
        // Arrange
        var petId = 1;
        var pet = new Pet
        {
            Id = petId,
            Nome = "Bidu",
            Consultas = new List<Consulta> { new() { Status = "R", Data = DateTime.Today } },
            Vacinas = new List<Vacina> 
            { 
                new() { Status = "P", Data = DateTime.Today.AddDays(-5) } // Overdue: -15
            }
        };
        _petRepositoryMock.Setup(r => r.GetByIdAsync(petId)).ReturnsAsync(pet);

        // Act
        var result = await _service.GetIntelligencePreventivaAsync(petId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(85, result.Score); // 100 - 15 = 85
        Assert.Equal("EXCELENTE", result.Status); // >= 80 is EXCELENTE
        Assert.Contains("vacina(s) em atraso", result.Recomendacoes.Any(r => r.Contains("atraso")) ? result.Recomendacoes.First(r => r.Contains("atraso")) : "");
    }

    [Fact]
    public async Task GetIntelligencePreventivaAsync_WhenPetHasPendingVaccineIn7Days_DecreasesScoreCorrectly()
    {
        // Arrange
        var petId = 1;
        var pet = new Pet
        {
            Id = petId,
            Nome = "Bidu",
            Consultas = new List<Consulta> { new() { Status = "R", Data = DateTime.Today } },
            Vacinas = new List<Vacina> 
            { 
                new() { Status = "P", Data = DateTime.Today.AddDays(3) } // Pending in next 7 days: -5
            }
        };
        _petRepositoryMock.Setup(r => r.GetByIdAsync(petId)).ReturnsAsync(pet);

        // Act
        var result = await _service.GetIntelligencePreventivaAsync(petId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(95, result.Score); // 100 - 5 = 95
        Assert.Equal("EXCELENTE", result.Status);
        Assert.Contains("agendada(s) para os próximos dias", result.Recomendacoes.Any(r => r.Contains("próximos dias")) ? result.Recomendacoes.First(r => r.Contains("próximos dias")) : "");
    }

    [Fact]
    public async Task GetIntelligencePreventivaAsync_WhenPetHasNoConsultations_DecreasesScoreCorrectlyAndAddsRecommendation()
    {
        // Arrange
        var petId = 1;
        var pet = new Pet
        {
            Id = petId,
            Nome = "Bidu",
            Consultas = new List<Consulta>(), // No consultations: -20
            Vacinas = new List<Vacina> { new() { Status = "A", Data = DateTime.Today.AddMonths(-6) } }
        };
        _petRepositoryMock.Setup(r => r.GetByIdAsync(petId)).ReturnsAsync(pet);

        // Act
        var result = await _service.GetIntelligencePreventivaAsync(petId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(80, result.Score); // 100 - 20 = 80
        Assert.Equal("EXCELENTE", result.Status);
        Assert.Contains("Nenhuma consulta preventiva foi registrada ainda", result.Recomendacoes.First(r => r.Contains("Nenhuma consulta")));
    }

    [Fact]
    public async Task GetIntelligencePreventivaAsync_WhenPetHasOldConsultation_DecreasesScoreCorrectlyAndAddsRecommendation()
    {
        // Arrange
        var petId = 1;
        var pet = new Pet
        {
            Id = petId,
            Nome = "Bidu",
            Consultas = new List<Consulta> { new() { Status = "R", Data = DateTime.Today.AddMonths(-7) } }, // Old consultation (>6 months): -10
            Vacinas = new List<Vacina> { new() { Status = "A", Data = DateTime.Today.AddMonths(-6) } }
        };
        _petRepositoryMock.Setup(r => r.GetByIdAsync(petId)).ReturnsAsync(pet);

        // Act
        var result = await _service.GetIntelligencePreventivaAsync(petId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(90, result.Score); // 100 - 10 = 90
        Assert.Equal("EXCELENTE", result.Status);
        Assert.Contains("Já faz mais de 6 meses desde a última consulta", result.Recomendacoes.First(r => r.Contains("Já faz mais de 6 meses")));
    }

    [Fact]
    public async Task GetIntelligencePreventivaAsync_WhenPetHasMultipleIssues_ClampsScoreToZeroAndReturnsCritical()
    {
        // Arrange
        var petId = 1;
        var pet = new Pet
        {
            Id = petId,
            Nome = "Bidu",
            Consultas = new List<Consulta>(), // No consultations: -20
            Vacinas = new List<Vacina> 
            { 
                new() { Status = "P", Data = DateTime.Today.AddDays(-10) }, // Overdue: -15
                new() { Status = "P", Data = DateTime.Today.AddDays(-20) }, // Overdue: -15
                new() { Status = "P", Data = DateTime.Today.AddDays(-30) }, // Overdue: -15
                new() { Status = "P", Data = DateTime.Today.AddDays(-40) }, // Overdue: -15
                new() { Status = "P", Data = DateTime.Today.AddDays(-50) }, // Overdue: -15
                new() { Status = "P", Data = DateTime.Today.AddDays(-60) }  // Overdue: -15
            } // Total penalty = 20 + 6*15 = 110. Score should clamp to 0.
        };
        _petRepositoryMock.Setup(r => r.GetByIdAsync(petId)).ReturnsAsync(pet);

        // Act
        var result = await _service.GetIntelligencePreventivaAsync(petId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.Score);
        Assert.Equal("CRÍTICO", result.Status);
        Assert.Contains("O status de saúde do seu pet está CRÍTICO", result.Recomendacoes.First(r => r.Contains("CRÍTICO")));
    }
}
