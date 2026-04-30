using TeamBuilderPokemon.Models;

namespace TeamBuilderPokemon.Services;

/// <summary>Resultado da analise de um time contra um tipo de ataque especifico.</summary>
public class TypeMatchup
{
    public string Type { get; set; } = string.Empty;

    public int WeakCount { get; set; }

    public int ResistantCount { get; set; }

    public int ImmuneCount { get; set; }

    public bool IsWeakness { get; set; }

    public bool IsCriticalWeakness { get; set; }

    public bool IsResistance { get; set; }

    public List<string> SuggestedPokemon { get; set; } = new();
}

public class TeamAnalysisResult
{
    public List<TypeMatchup> Matchups { get; set; } = new();

    /// <summary>Pokemon de fora do time que resistem a duas ou mais fraquezas de uma vez.</summary>
    public List<CoverageSuggestion> CoverageSuggestions { get; set; } = new();
}

/// <summary>Um Pokemon sugerido e quais fraquezas do time ele cobre.</summary>
public class CoverageSuggestion
{
    public string Name { get; set; } = string.Empty;

    public string? SpriteUrl { get; set; }

    public List<string> Covers { get; set; } = new();
}

/// <summary>
/// Analisa o time inteiro contra os 18 tipos de ataque.
/// Fraqueza: mais da metade toma dano aumentado. Critica: dois tercos ou mais.
/// Resistencia: mais da metade toma dano reduzido.
/// </summary>
public static class TeamAnalyzer
{
    private const double WeaknessThreshold = 0.5;
    private const double CriticalWeaknessThreshold = 2.0 / 3.0;
    private const double ResistanceThreshold = 0.5;
    private const int MaxSuggestions = 5;

    public static TeamAnalysisResult Analyze(IReadOnlyList<Pokemon> team, IReadOnlyList<Pokemon> fullRoster)
    {
        var result = new TeamAnalysisResult();

        if (team.Count == 0)
        {
            foreach (var type in TypeChart.Types)
            {
                result.Matchups.Add(new TypeMatchup { Type = type });
            }

            return result;
        }

        var teamIds = team.Select(p => p.Id).ToHashSet();

        foreach (var attackType in TypeChart.Types)
        {
            int weak = 0;
            int resistant = 0;
            int immune = 0;

            foreach (var pokemon in team)
            {
                var multiplier = TypeChart.Effectiveness(attackType, pokemon.GetTypes());

                if (multiplier == 0.0)
                {
                    immune++;
                }
                else if (multiplier > 1.0)
                {
                    weak++;
                }
                else if (multiplier < 1.0)
                {
                    resistant++;
                }
            }

            var weakRatio = (double)weak / team.Count;
            var resistantRatio = (double)resistant / team.Count;

            var matchup = new TypeMatchup
            {
                Type = attackType,
                WeakCount = weak,
                ResistantCount = resistant,
                ImmuneCount = immune,
                IsWeakness = weakRatio > WeaknessThreshold,
                IsCriticalWeakness = weakRatio >= CriticalWeaknessThreshold,
                IsResistance = resistantRatio > ResistanceThreshold,
            };

            if (matchup.IsWeakness)
            {
                matchup.SuggestedPokemon = fullRoster
                    .Where(p => !teamIds.Contains(p.Id))
                    .Where(p => TypeChart.Effectiveness(attackType, p.GetTypes()) < 1.0)
                    .OrderBy(p => p.Name)
                    .Select(p => p.Name)
                    .Take(MaxSuggestions)
                    .ToList();
            }

            result.Matchups.Add(matchup);
        }

        result.CoverageSuggestions = SuggestCoverage(result.Matchups, teamIds, fullRoster);
        return result;
    }

    /// <summary>
    /// Cruza as fraquezas: quem resiste a mais fraquezas ao mesmo tempo vem primeiro.
    /// So entra quem cobre pelo menos duas (uma so ja aparece na tabela por tipo).
    /// </summary>
    private static List<CoverageSuggestion> SuggestCoverage(
        IEnumerable<TypeMatchup> matchups, HashSet<int> teamIds, IReadOnlyList<Pokemon> fullRoster)
    {
        var weaknesses = matchups.Where(m => m.IsWeakness).Select(m => m.Type).ToList();
        if (weaknesses.Count < 2)
        {
            return new List<CoverageSuggestion>();
        }

        return fullRoster
            .Where(p => !teamIds.Contains(p.Id))
            .Select(p => new CoverageSuggestion
            {
                Name = p.Name,
                SpriteUrl = p.GetSpriteUrl(),
                Covers = weaknesses.Where(type => TypeChart.Effectiveness(type, p.GetTypes()) < 1.0).ToList(),
            })
            .Where(s => s.Covers.Count >= 2)
            .OrderByDescending(s => s.Covers.Count)
            .ThenBy(s => s.Name)
            .Take(MaxSuggestions)
            .ToList();
    }
}
