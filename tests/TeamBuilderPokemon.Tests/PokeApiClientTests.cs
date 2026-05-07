using System.Net;
using System.Text;
using TeamBuilderPokemon.Services;
using Xunit;

namespace TeamBuilderPokemon.Tests;

public class PokeApiClientTests
{
    // Responde qualquer requisicao com o status/corpo dados, e guarda a URL pedida.
    private sealed class FakeHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _status;
        private readonly string _body;

        public FakeHandler(HttpStatusCode status, string body)
        {
            _status = status;
            _body = body;
        }

        public Uri? LastUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastUri = request.RequestUri;
            return Task.FromResult(new HttpResponseMessage(_status) { Content = new StringContent(_body, Encoding.UTF8, "application/json") });
        }
    }

    private static PokeApiClient Client(FakeHandler handler) =>
        new(new HttpClient(handler) { BaseAddress = new Uri("https://pokeapi.co/api/v2/") });

    [Fact]
    public async Task ParsesNameTypesInSlotOrderAndDexNumber()
    {
        const string json = """
            {"id": 6, "name": "charizard",
             "types": [{"slot": 2, "type": {"name": "flying"}}, {"slot": 1, "type": {"name": "fire"}}]}
            """;
        var handler = new FakeHandler(HttpStatusCode.OK, json);

        var pokemon = await Client(handler).FetchAsync("Charizard");

        Assert.NotNull(pokemon);
        Assert.Equal("Charizard", pokemon!.Name);
        Assert.Equal("Fire", pokemon.Type1);
        Assert.Equal("Flying", pokemon.Type2);
        Assert.Equal(6, pokemon.DexNumber);
        Assert.EndsWith("/pokemon/charizard", handler.LastUri!.AbsolutePath);
    }

    [Fact]
    public async Task ReturnsNullWhenThePokemonDoesNotExist()
    {
        var pokemon = await Client(new FakeHandler(HttpStatusCode.NotFound, "Not Found")).FetchAsync("naoexiste");
        Assert.Null(pokemon);
    }

    [Theory]
    [InlineData("Mr. Mime", "mr-mime")]
    [InlineData("  Farfetch'd ", "farfetchd")]
    [InlineData("LUCARIO", "lucario")]
    public void NormalizesNamesTheWayPokeApiExpects(string input, string expected)
    {
        Assert.Equal(expected, PokeApiClient.NormalizeName(input));
    }
}
