namespace ClyvoVetApi.Models;

public class Medicamento
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public ICollection<ConsultaMedicamento> ConsultasMedicamentos { get; set; } = new List<ConsultaMedicamento>();
}
