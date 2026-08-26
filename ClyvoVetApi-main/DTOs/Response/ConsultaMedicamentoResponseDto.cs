namespace ClyvoVetApi.DTOs.Response;

public class ConsultaMedicamentoResponseDto
{
    public int Id { get; set; }
    public int ConsultaId { get; set; }
    public int MedicamentoId { get; set; }
    public string MedicamentoNome { get; set; } = string.Empty;
    public string Dosagem { get; set; } = string.Empty;
}
