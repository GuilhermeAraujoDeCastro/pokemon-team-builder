namespace TeamBuilderPokemon.Services;

/// <summary>Nome dos tipos em portugues pra exibir na tela (o banco e o TypeChart usam o nome em ingles).</summary>
public static class TypeNames
{
    private static readonly Dictionary<string, string> Pt = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Normal"] = "Normal", ["Fire"] = "Fogo", ["Water"] = "Água", ["Electric"] = "Elétrico",
        ["Grass"] = "Planta", ["Ice"] = "Gelo", ["Fighting"] = "Lutador", ["Poison"] = "Venenoso",
        ["Ground"] = "Terrestre", ["Flying"] = "Voador", ["Psychic"] = "Psíquico", ["Bug"] = "Inseto",
        ["Rock"] = "Pedra", ["Ghost"] = "Fantasma", ["Dragon"] = "Dragão", ["Dark"] = "Sombrio",
        ["Steel"] = "Aço", ["Fairy"] = "Fada",
    };

    public static string ToPt(string type) => Pt.TryGetValue(type, out var name) ? name : type;
}
