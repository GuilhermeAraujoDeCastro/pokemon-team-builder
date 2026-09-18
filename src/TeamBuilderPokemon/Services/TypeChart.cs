namespace TeamBuilderPokemon.Services;

/// <summary>
/// Tabela de efetividade de tipos (18 tipos, geracao 6 em diante, com Fairy).
/// E' a mesma tabela do meu Simulador de Batalha Pokemon em Python, portada
/// pra C# pra manter os dois projetos consistentes entre si. Guarda so as
/// excecoes a regra padrao (multiplicador 1x, "dano normal").
/// </summary>
public static class TypeChart
{
    public static readonly string[] Types =
    {
        "Normal", "Fire", "Water", "Electric", "Grass", "Ice", "Fighting",
        "Poison", "Ground", "Flying", "Psychic", "Bug", "Rock", "Ghost",
        "Dragon", "Dark", "Steel", "Fairy",
    };

    private static readonly Dictionary<string, Dictionary<string, double>> Chart =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Normal"] = new(StringComparer.OrdinalIgnoreCase) { ["Rock"] = 0.5, ["Ghost"] = 0.0, ["Steel"] = 0.5 },
            ["Fire"] = new(StringComparer.OrdinalIgnoreCase)
            {
                ["Fire"] = 0.5, ["Water"] = 0.5, ["Grass"] = 2.0, ["Ice"] = 2.0, ["Bug"] = 2.0,
                ["Rock"] = 0.5, ["Dragon"] = 0.5, ["Steel"] = 2.0,
            },
            ["Water"] = new(StringComparer.OrdinalIgnoreCase)
            {
                ["Fire"] = 2.0, ["Water"] = 0.5, ["Grass"] = 0.5, ["Ground"] = 2.0,
                ["Rock"] = 2.0, ["Dragon"] = 0.5,
            },
            ["Electric"] = new(StringComparer.OrdinalIgnoreCase)
            {
                ["Water"] = 2.0, ["Electric"] = 0.5, ["Grass"] = 0.5, ["Ground"] = 0.0,
                ["Flying"] = 2.0, ["Dragon"] = 0.5,
            },
            ["Grass"] = new(StringComparer.OrdinalIgnoreCase)
            {
                ["Fire"] = 0.5, ["Water"] = 2.0, ["Grass"] = 0.5, ["Poison"] = 0.5,
                ["Ground"] = 2.0, ["Flying"] = 0.5, ["Bug"] = 0.5, ["Rock"] = 2.0,
                ["Dragon"] = 0.5, ["Steel"] = 0.5,
            },
            ["Ice"] = new(StringComparer.OrdinalIgnoreCase)
            {
                ["Fire"] = 0.5, ["Water"] = 0.5, ["Grass"] = 2.0, ["Ice"] = 0.5, ["Ground"] = 2.0,
                ["Flying"] = 2.0, ["Dragon"] = 2.0, ["Steel"] = 0.5,
            },
            ["Fighting"] = new(StringComparer.OrdinalIgnoreCase)
            {
                ["Normal"] = 2.0, ["Ice"] = 2.0, ["Poison"] = 0.5, ["Flying"] = 0.5,
                ["Psychic"] = 0.5, ["Bug"] = 0.5, ["Rock"] = 2.0, ["Ghost"] = 0.0,
                ["Dark"] = 2.0, ["Steel"] = 2.0, ["Fairy"] = 0.5,
            },
            ["Poison"] = new(StringComparer.OrdinalIgnoreCase)
            {
                ["Grass"] = 2.0, ["Poison"] = 0.5, ["Ground"] = 0.5, ["Rock"] = 0.5,
                ["Ghost"] = 0.5, ["Steel"] = 0.0, ["Fairy"] = 2.0,
            },
            ["Ground"] = new(StringComparer.OrdinalIgnoreCase)
            {
                ["Fire"] = 2.0, ["Electric"] = 2.0, ["Grass"] = 0.5, ["Poison"] = 2.0,
                ["Flying"] = 0.0, ["Bug"] = 0.5, ["Rock"] = 2.0, ["Steel"] = 2.0,
            },
            ["Flying"] = new(StringComparer.OrdinalIgnoreCase)
            {
                ["Electric"] = 0.5, ["Grass"] = 2.0, ["Fighting"] = 2.0, ["Bug"] = 2.0,
                ["Rock"] = 0.5, ["Steel"] = 0.5,
            },
            ["Psychic"] = new(StringComparer.OrdinalIgnoreCase)
            {
                ["Fighting"] = 2.0, ["Poison"] = 2.0, ["Psychic"] = 0.5, ["Dark"] = 0.0,
                ["Steel"] = 0.5,
            },
            ["Bug"] = new(StringComparer.OrdinalIgnoreCase)
            {
                ["Fire"] = 0.5, ["Grass"] = 2.0, ["Fighting"] = 0.5, ["Poison"] = 0.5,
                ["Flying"] = 0.5, ["Psychic"] = 2.0, ["Ghost"] = 0.5, ["Dark"] = 2.0,
                ["Steel"] = 0.5, ["Fairy"] = 0.5,
            },
            ["Rock"] = new(StringComparer.OrdinalIgnoreCase)
            {
                ["Fire"] = 2.0, ["Ice"] = 2.0, ["Fighting"] = 0.5, ["Ground"] = 0.5,
                ["Flying"] = 2.0, ["Bug"] = 2.0, ["Steel"] = 0.5,
            },
            ["Ghost"] = new(StringComparer.OrdinalIgnoreCase)
            {
                ["Normal"] = 0.0, ["Psychic"] = 2.0, ["Ghost"] = 2.0, ["Dark"] = 0.5,
            },
            ["Dragon"] = new(StringComparer.OrdinalIgnoreCase)
            {
                ["Dragon"] = 2.0, ["Steel"] = 0.5, ["Fairy"] = 0.0,
            },
            ["Dark"] = new(StringComparer.OrdinalIgnoreCase)
            {
                ["Fighting"] = 0.5, ["Psychic"] = 2.0, ["Ghost"] = 2.0, ["Dark"] = 0.5,
                ["Fairy"] = 0.5,
            },
            ["Steel"] = new(StringComparer.OrdinalIgnoreCase)
            {
                ["Fire"] = 0.5, ["Water"] = 0.5, ["Electric"] = 0.5, ["Ice"] = 2.0,
                ["Rock"] = 2.0, ["Steel"] = 0.5, ["Fairy"] = 2.0,
            },
            ["Fairy"] = new(StringComparer.OrdinalIgnoreCase)
            {
                ["Fire"] = 0.5, ["Fighting"] = 2.0, ["Poison"] = 0.5, ["Dragon"] = 2.0,
                ["Dark"] = 2.0, ["Steel"] = 0.5,
            },
        };

    /// <summary>
    /// Multiplica o efeito de um tipo de ataque contra 1 ou 2 tipos de
    /// defensor. Exemplo: um golpe de Gelo contra um Pokemon Grama/Dragao da
    /// 4x (2x contra Grama vezes 2x contra Dragao).
    /// </summary>
    public static double Effectiveness(string attackType, IEnumerable<string> defenderTypes)
    {
        double multiplier = 1.0;

        if (!Chart.TryGetValue(attackType, out var row))
        {
            return multiplier;
        }

        foreach (var defendType in defenderTypes)
        {
            multiplier *= row.TryGetValue(defendType, out var value) ? value : 1.0;
        }

        return multiplier;
    }
}
