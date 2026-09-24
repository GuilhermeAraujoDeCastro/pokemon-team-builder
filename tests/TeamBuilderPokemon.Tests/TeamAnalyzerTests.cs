using System.Linq;
using TeamBuilderPokemon.Models;
using TeamBuilderPokemon.Services;
using Xunit;

namespace TeamBuilderPokemon.Tests;

public class TeamAnalyzerTests
{
    private static Pokemon MakeMon(int id, string name, string type1, string? type2 = null)
        => new() { Id = id, Name = name, Type1 = type1, Type2 = type2 };

    [Fact]
    public void FlagsCriticalWeakness_WhenMajorityWeakToAType()
    {
        // 4 de Grama + 2 de Fogo: contra Gelo, os 4 de Grama sao fracos (2x)
        // e os 2 de Fogo sao resistentes (0.5x). 4 de 6 = 2/3, bate exatamente
        // o limite de fraqueza critica.
        var team = new List<Pokemon>
        {
            MakeMon(1, "Grama A", "Grass"),
            MakeMon(2, "Grama B", "Grass"),
            MakeMon(3, "Grama C", "Grass"),
            MakeMon(4, "Grama D", "Grass"),
            MakeMon(5, "Fogo A", "Fire"),
            MakeMon(6, "Fogo B", "Fire"),
        };

        var result = TeamAnalyzer.Analyze(team, team);
        var iceMatchup = result.Matchups.Single(m => m.Type == "Ice");

        Assert.Equal(4, iceMatchup.WeakCount);
        Assert.Equal(2, iceMatchup.ResistantCount);
        Assert.True(iceMatchup.IsWeakness);
        Assert.True(iceMatchup.IsCriticalWeakness);
    }

    [Fact]
    public void SuggestsRosterPokemonResistantToWeakness()
    {
        var team = new List<Pokemon>
        {
            MakeMon(1, "Grama A", "Grass"),
            MakeMon(2, "Grama B", "Grass"),
            MakeMon(3, "Grama C", "Grass"),
            MakeMon(4, "Grama D", "Grass"),
            MakeMon(5, "Fogo A", "Fire"),
            MakeMon(6, "Fogo B", "Fire"),
        };

        var roster = new List<Pokemon>(team) { MakeMon(7, "Growlithe", "Fire") };

        var result = TeamAnalyzer.Analyze(team, roster);
        var iceMatchup = result.Matchups.Single(m => m.Type == "Ice");

        Assert.Contains("Growlithe", iceMatchup.SuggestedPokemon);
    }

    [Fact]
    public void EmptyTeam_ReturnsEmptyResult()
    {
        var result = TeamAnalyzer.Analyze(new List<Pokemon>(), new List<Pokemon>());

        Assert.All(result.Matchups, m =>
        {
            Assert.Equal(0, m.WeakCount);
            Assert.Equal(0, m.ResistantCount);
            Assert.False(m.IsWeakness);
        });
    }

    [Fact]
    public void Analyze_ProducesOneMatchupPerType()
    {
        var team = new List<Pokemon> { MakeMon(1, "Pikachu", "Electric") };

        var result = TeamAnalyzer.Analyze(team, team);

        Assert.Equal(TypeChart.Types.Length, result.Matchups.Count);
    }

    [Fact]
    public void SuggestsPokemonThatCoverTwoWeaknessesAtOnce()
    {
        // Tres de Planta: fracos a Fogo, Gelo, Voador, Veneno e Inseto.
        var team = new List<Pokemon>
        {
            MakeMon(1, "Grama A", "Grass"),
            MakeMon(2, "Grama B", "Grass"),
            MakeMon(3, "Grama C", "Grass"),
        };
        // Magnemite (Eletrico/Aco) resiste a Gelo, Voador, Veneno e Inseto; Squirtle so a Fogo e Gelo.
        var roster = new List<Pokemon>(team)
        {
            MakeMon(4, "Magnemite", "Electric", "Steel"),
            MakeMon(5, "Squirtle", "Water"),
            MakeMon(6, "Eevee", "Normal"),
        };

        var result = TeamAnalyzer.Analyze(team, roster);

        Assert.Equal("Magnemite", result.CoverageSuggestions.First().Name);
        Assert.True(result.CoverageSuggestions.First().Covers.Count >= 3);
        Assert.DoesNotContain(result.CoverageSuggestions, s => s.Name == "Eevee"); // nao cobre nada
        Assert.DoesNotContain(result.CoverageSuggestions, s => s.Name.StartsWith("Grama")); // ja esta no time
    }
}
