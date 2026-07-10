using System.Globalization;
using System.Net;
using System.Text.Json;
using TeamBuilderPokemon.Models;

namespace TeamBuilderPokemon.Services;

/// <summary>
/// Busca um Pokemon por nome na PokeAPI (so nome, tipos e numero da Pokedex) pra ampliar o catalogo.
/// </summary>
public class PokeApiClient
{
    private readonly HttpClient _http;

    public PokeApiClient(HttpClient http)
    {
        _http = http;
    }

    /// <summary>Devolve null quando o Pokemon nao existe; erro de rede sobe como excecao.</summary>
    public async Task<Pokemon?> FetchAsync(string name, CancellationToken cancellationToken = default)
    {
        var slug = NormalizeName(name);
        if (slug.Length == 0)
        {
            return null;
        }

        using var response = await _http.GetAsync($"pokemon/{Uri.EscapeDataString(slug)}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        return Parse(json.RootElement);
    }

    /// <summary>"Mr. Mime" vira "mr-mime", do jeito que a PokeAPI espera.</summary>
    public static string NormalizeName(string name)
    {
        // Nidoran♀ e Nidoran♂ viram nidoran-f e nidoran-m; Flabébé vira flabebe; Type: Null vira type-null.
        var semAcento = string.Concat(name.Trim().ToLowerInvariant()
            .Replace("♀", "-f").Replace("♂", "-m")
            .Normalize(System.Text.NormalizationForm.FormD)
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark));
        return string.Join("-", semAcento
            .Replace(".", " ").Replace("'", "").Replace(":", "")
            .Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    public static Pokemon Parse(JsonElement root)
    {
        var types = root.GetProperty("types").EnumerateArray()
            .OrderBy(t => t.GetProperty("slot").GetInt32())
            .Select(t => Capitalize(t.GetProperty("type").GetProperty("name").GetString() ?? string.Empty))
            .ToList();

        return new Pokemon
        {
            Name = Capitalize(root.GetProperty("name").GetString() ?? string.Empty),
            Type1 = types.ElementAtOrDefault(0) ?? "Normal",
            Type2 = types.ElementAtOrDefault(1),
            DexNumber = root.GetProperty("id").GetInt32(),
        };
    }

    // "mr-mime" -> "Mr-Mime", "fire" -> "Fire" (mesmo formato do TypeChart).
    private static string Capitalize(string value)
    {
        return string.Join("-", value.Split('-').Select(part =>
            part.Length == 0 ? part : char.ToUpper(part[0], CultureInfo.InvariantCulture) + part[1..]));
    }
}
