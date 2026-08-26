namespace ClyvoVetApi.Models;

public class ConsultaMedicamento
{
    public int Id { get; set; }
    public int ConsultaId { get; set; }
    public Consulta? Consulta { get; set; }
    public int MedicamentoId { get; set; }
    public Medicamento? Medicamento { get; set; }
    public string Dosagem { get; set; } = string.Empty;
}
