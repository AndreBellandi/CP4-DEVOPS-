namespace ClyvoVetApi.DTOs.Response;

public class DonoDetailsResponseDto : DonoResponseDto
{
    public IEnumerable<PetDto> Pets { get; set; } = [];
}
