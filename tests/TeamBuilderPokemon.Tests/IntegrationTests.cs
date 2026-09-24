using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TeamBuilderPokemon.Data;
using TeamBuilderPokemon.Models;
using Xunit;

namespace TeamBuilderPokemon.Tests;

/// <summary>Sobe o app inteiro com SQLite em memoria (banco novo por teste).</summary>
public class AppFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();
        builder.UseEnvironment("Development");
        builder.ConfigureServices(services =>
        {
            var existing = services.Single(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            services.Remove(existing);
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(_connection));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection.Dispose();
    }
}

public class IntegrationTests : IDisposable
{
    private readonly AppFactory _factory = new();

    public void Dispose() => _factory.Dispose();

    private HttpClient NewClient() => _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    [Fact]
    public async Task HomePageLoads()
    {
        var html = await NewClient().GetStringAsync("/");
        Assert.Contains("Team Builder Pokémon", html);
    }

    [Fact]
    public async Task TeamsPageRedirectsToLoginWhenAnonymous()
    {
        var response = await NewClient().GetAsync("/Teams");
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Identity/Account/Login", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task ApiAnswers401InsteadOfRedirectingWhenAnonymous()
    {
        var response = await NewClient().GetAsync("/api/teams");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ApiListsTheSeedCatalog()
    {
        var json = await NewClient().GetStringAsync("/api/pokemon");
        using var doc = JsonDocument.Parse(json);
        Assert.Equal(PokemonSeedData.All.Length, doc.RootElement.GetArrayLength());
    }

    [Fact]
    public async Task PublicPageShowsOnlyPublicTeams()
    {
        int publicId, privateId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var shared = new Team { Name = "Aberto", UserId = "x", IsPublic = true, Slots = { new TeamSlot { PokemonId = 1, SlotNumber = 1 } } };
            var hidden = new Team { Name = "Fechado", UserId = "x", Slots = { new TeamSlot { PokemonId = 2, SlotNumber = 1 } } };
            db.Teams.AddRange(shared, hidden);
            await db.SaveChangesAsync();
            (publicId, privateId) = (shared.Id, hidden.Id);
        }

        var client = NewClient();
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/Teams/Public/{publicId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/Teams/Public/{privateId}")).StatusCode);
    }

    [Fact]
    public async Task RegisterCreateTeamAndSeeTheAnalysis()
    {
        var client = _factory.CreateClient(); // segue redirects e guarda o cookie de login

        var registerPage = await client.GetStringAsync("/Identity/Account/Register");
        var register = await client.PostAsync("/Identity/Account/Register", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.Email"] = "ash@exemplo.com",
            ["Input.Password"] = "Pikachu#25",
            ["Input.ConfirmPassword"] = "Pikachu#25",
            ["__RequestVerificationToken"] = Token(registerPage),
        }));
        Assert.Equal(HttpStatusCode.OK, register.StatusCode);

        var createPage = await client.GetStringAsync("/Teams/Create");
        var form = new List<KeyValuePair<string, string>>
        {
            new("name", "Time do Ash"),
            new("pokemonIds", "4"),  // Pikachu
            new("pokemonIds", "9"),  // Gyarados
            new("__RequestVerificationToken", Token(createPage)),
        };
        var created = await client.PostAsync("/Teams/Create", new FormUrlEncodedContent(form));
        var details = await created.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, created.StatusCode);
        Assert.Contains("Time do Ash", details);
        Assert.Contains("Análise de tipos", details);
    }

    [Fact]
    public async Task CreatingATeamWithADuplicateShowsAnErrorInsteadOfSaving()
    {
        var client = _factory.CreateClient();
        var registerPage = await client.GetStringAsync("/Identity/Account/Register");
        await client.PostAsync("/Identity/Account/Register", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.Email"] = "misty@exemplo.com",
            ["Input.Password"] = "Starmie#121",
            ["Input.ConfirmPassword"] = "Starmie#121",
            ["__RequestVerificationToken"] = Token(registerPage),
        }));

        var createPage = await client.GetStringAsync("/Teams/Create");
        var response = await client.PostAsync("/Teams/Create", new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
        {
            new("name", "Repetido"),
            new("pokemonIds", "3"),
            new("pokemonIds", "3"),
            new("__RequestVerificationToken", Token(createPage)),
        }));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("Não repita o mesmo Pokémon", WebUtility.HtmlDecode(html));
        using var scope = _factory.Services.CreateScope();
        Assert.Equal(0, await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Teams.CountAsync());
    }

    // Token anti-CSRF que o formulario manda escondido.
    private static string Token(string html)
    {
        var match = Regex.Match(html, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"");
        Assert.True(match.Success, "token anti-CSRF nao encontrado na pagina");
        return match.Groups[1].Value;
    }
}
