using FiapCloudGames.Jogos.Core.Entities;

namespace FiapCloudGames.Jogos.Core.Interfaces.Services;

public interface IJogoService
{
    Task<IEnumerable<Jogo>> BuscarTodosAsync();
    Task<Jogo?> BuscarPorIdAsync(Guid id);
    Task<Jogo> AdicionarAsync(Jogo jogo);
    Task AtualizarAsync(Jogo jogo);
    Task<bool> RemoverAsync(Guid id);
    Task<IEnumerable<Jogo>> BuscarJogosComPrecoDeDescontoAsync(DateTime dataReferencia);
}