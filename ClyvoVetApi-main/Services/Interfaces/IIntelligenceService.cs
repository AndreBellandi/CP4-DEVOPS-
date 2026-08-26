using ClyvoVetApi.DTOs.Response;

namespace ClyvoVetApi.Services.Interfaces;

public interface IIntelligenceService
{
    Task<HealthDashboardResponseDto> GetIntelligencePreventivaAsync(int petId);
}
