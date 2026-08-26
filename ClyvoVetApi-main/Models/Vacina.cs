namespace ClyvoVetApi.Models;

public class Vacina
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public DateTime Data { get; set; }
    public string Status { get; set; } = "P"; // 'P' (Pendente) ou 'A' (Aplicada)

    public int PetId { get; set; }
    public Pet? Pet { get; set; }
}