namespace FiapCloudGames.Jogos.Core.Entities;

public class GameIndex
{
    public Guid Id { get; set; }
    public string Titulo { get; set; }
    public string Descricao { get; set; }
    public string Genero { get; set; }
    public decimal Preco { get; set; }
    public int Visualizacoes { get; set; }
    public DateTime CriadoEm { get; set; }
}
