namespace TeamBuilderPokemon.Models;

public class Pokemon
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Type1 { get; set; } = string.Empty;

    public string? Type2 { get; set; }

    /// <summary>Numero na Pokedex nacional; monta a URL do sprite oficial da PokeAPI.</summary>
    public int? DexNumber { get; set; }

    /// <summary>
    /// Os 1 ou 2 tipos do Pokemon. Metodo (nao propriedade) pro EF Core nao tentar mapear como coluna.
    /// </summary>
    public IReadOnlyList<string> GetTypes()
    {
        return Type2 is null ? new[] { Type1 } : new[] { Type1, Type2 };
    }

    /// <summary>Sprite do repositorio publico da PokeAPI (null quando nao sabemos o numero).</summary>
    public string? GetSpriteUrl()
    {
        return DexNumber is null
            ? null
            : $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/{DexNumber}.png";
    }
}
