using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Aggregations;
using Elastic.Clients.Elasticsearch.Nodes;
using Elastic.Clients.Elasticsearch.Security;
using Elastic.Transport;
using FiapCloudGames.Jogos.API.DTOs.Request;
using FiapCloudGames.Jogos.API.DTOs.Response;
using FiapCloudGames.Jogos.API.Properties;
using FiapCloudGames.Jogos.Core.Entities;
using FiapCloudGames.Jogos.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiapCloudGames.Jogos.API.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class JogoController : ControllerBase
{
    private readonly IJogoService _jogoService;
    private readonly ElasticsearchClient _client;

    public JogoController(IJogoService jogoService, IConfiguration configuration)
    {
        _jogoService = jogoService;
        var x = ElasticSettings.CloudId;
        _client = new ElasticsearchClient(ElasticSettings.CloudId, new Elastic.Transport.ApiKey(ElasticSettings.ApiKey));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CadastrarJogo([FromBody] CadastrarJogoDto dto)
    {
        var jogo = new Jogo(dto.Titulo, dto.Descricao, dto.Genero, dto.Preco);
        await _jogoService.AdicionarAsync(jogo);

        var gameIndex = new GameIndex
        {
            Id = jogo.Id,
            Titulo = jogo.Titulo,
            Descricao = jogo.Descricao,
            Genero = jogo.Genero.ToString(),
            Preco = jogo.Preco,
            Visualizacoes = 0,
            CriadoEm = DateTime.UtcNow
        };

        await _client.IndexAsync(gameIndex, i => i
            .Index("games")
            .Id(jogo.Id)
        );

        return CreatedAtAction(nameof(BuscarPorId), new { id = jogo.Id }, jogo);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> BuscarPorId(Guid id)
    {
        var jogo = await _jogoService.BuscarPorIdAsync(id);
        if (jogo == null) return NotFound();
        return Ok(new JogoDTO(jogo));
    }

    [HttpGet]
    public async Task<IActionResult> BuscarTodos()
    {
        var jogos = await _jogoService.BuscarTodosAsync();

        if(jogos == null) return NotFound();

        var listaJogosDto = new List<JogoDTO>();
        jogos.ToList().ForEach(j => listaJogosDto.Add(new JogoDTO(j)));

        return Ok(listaJogosDto);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> RemoverJogo(Guid id)
    {
        var removidoComSucesso = await _jogoService.RemoverAsync(id);

        if (!removidoComSucesso) return NotFound();

        return NoContent();
    }

    //--------------------------------------------------ELASTICSEARCH--------------------------------------------------
    [HttpGet("buscar")]
    public async Task<IActionResult> Buscar(string termo)
    {
        var response = await _client.SearchAsync<GameIndex>(s => s
            .Indices("games")
            .Query(q => q
                .MultiMatch(mm => mm
                    .Query(termo)
                    .Fields(new[] { "titulo^2", "descricao" })
                    .Fuzziness("AUTO")
                )
            )
        );

        return Ok(response.Documents);
    }

    [HttpGet("sugestoes")]
    public async Task<IActionResult> SugerirJogos([FromQuery] string genero)
    {
        var response = await _client.SearchAsync<GameIndex>(s => s
            .Indices("games")
            .Size(5)
            .Query(q => q
                .Term(t => t
                    .Field("genero.keyword")
                    .Value(genero)
                )
            )
        );

        return Ok(response.Documents);
    }

    [HttpGet("mais-populares")]
    public async Task<IActionResult> JogosMaisPopulares()
    {
        var response = await _client.SearchAsync<GameIndex>(s => s
            .Indices("games")
            .Size(0)
            .Aggregations(a => a
                .Add("top_jogos", agg => agg
                    .TopHits(th => th
                        .Size(5)
                        .Sort(s => s
                            .Field("visualizacoes", SortOrder.Desc)
                        )
                    )
                )
            )
        );

        if (response.Aggregations.TryGetValue("top_jogos", out var aggregate)
            && aggregate is TopHitsAggregate topHits)
        {
            var jogos = topHits.Hits.Hits
                .Select(h => h.Source)
                .Where(s => s != null)
                .ToList();

            return Ok(jogos);
        }

        return Ok(Array.Empty<GameIndex>());
    }
}