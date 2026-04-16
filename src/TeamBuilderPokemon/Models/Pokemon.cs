namespace TeamBuilderPokemon.Models;

public class Pokemon
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Type1 { get; set; } = string.Empty;

    public string? Type2 { get; set; }

    /// <summary>
    /// Devolve os 1 ou 2 tipos deste Pokemon. E' um metodo, nao uma
    /// propriedade, de proposito: se fosse uma propriedade do tipo
    /// IReadOnlyList/List, o EF Core tentaria mapear como se fosse um dado
    /// a mais da entidade, em vez de so ler Type1/Type2.
    /// </summary>
    public IReadOnlyList<string> GetTypes()
    {
        return Type2 is null ? new[] { Type1 } : new[] { Type1, Type2 };
    }
}
