namespace ClyvoVetApi.Models;

public class Consulta
{
    public int Id { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public string Status { get; set; } = "A"; // 'A' (Agendada), 'C' (Cancelada), 'R' (Realizada)
    public DateTime Data { get; set; }

    public int PetId { get; set; }
    public Pet? Pet { get; set; }

    public int FuncionarioId { get; set; }
    public Funcionario? Funcionario { get; set; }

    public ICollection<ConsultaMedicamento> ConsultasMedicamentos { get; set; } = new List<ConsultaMedicamento>();
}