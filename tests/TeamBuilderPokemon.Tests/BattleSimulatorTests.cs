using TeamBuilderPokemon.Services;
using Xunit;

namespace TeamBuilderPokemon.Tests;

public class BattleSimulatorTests
{
    [Fact]
    public void HpFollowsTheOfficialFormulaForBase80()
    {
        // Nivel 50, base 80, sem IV/EV: (2*80*50/100) + 50 + 10 = 140.
        Assert.Equal(140, BattleSimulator.HpForLevel(50));
    }

    [Fact]
    public void PicksTheMostEffectiveTypeOfADualTypePokemon()
    {
        var attacker = new Fighter("Gyarados", new[] { "Water", "Flying" }, 50);
        var defender = new Fighter("Charmander", new[] { "Fire" }, 50);
        var (type, effectiveness) = BattleSimulator.BestAttack(attacker, defender);
        Assert.Equal("Water", type);
        Assert.Equal(2.0, effectiveness);
    }

    [Fact]
    public void ImmunityDealsNoDamage()
    {
        var attacker = new Fighter("Pikachu", new[] { "Electric" }, 50);
        var defender = new Fighter("Sandshrew", new[] { "Ground" }, 50);
        Assert.Equal(0, BattleSimulator.Damage(attacker, defender, new Random(1)));
    }

    [Fact]
    public void TypeAdvantageWinsAMirrorLevelBattle()
    {
        // Agua contra Fogo no mesmo nivel: com qualquer semente o time de Agua ganha.
        for (var seed = 0; seed < 20; seed++)
        {
            var water = new List<Fighter> { new("Squirtle", new[] { "Water" }, 50) };
            var fire = new List<Fighter> { new("Charmander", new[] { "Fire" }, 50) };
            var report = BattleSimulator.Simulate(water, fire, new Random(seed));
            Assert.Equal("A", report.Winner);
        }
    }

    [Fact]
    public void ImmuneVersusImmuneEndsInADrawInsteadOfLoopingForever()
    {
        // Normal nao acerta Fantasma e vice-versa: ninguem causa dano.
        var normal = new List<Fighter> { new("Eevee", new[] { "Normal" }, 50) };
        var ghost = new List<Fighter> { new("Gastly", new[] { "Ghost" }, 50) };
        var report = BattleSimulator.Simulate(normal, ghost, new Random(3));
        Assert.Equal("Empate", report.Winner);
        Assert.Equal(BattleSimulator.MaxTurns, report.Turns);
    }

    [Fact]
    public void NextPokemonEntersAfterAFaint()
    {
        var teamA = new List<Fighter> { new("Charmander", new[] { "Fire" }, 5), new("Squirtle", new[] { "Water" }, 100) };
        var teamB = new List<Fighter> { new("Geodude", new[] { "Rock" }, 100) };
        var report = BattleSimulator.Simulate(teamA, teamB, new Random(7));
        Assert.Contains(report.Log, line => line.Contains("Squirtle") && line.Contains("entra na batalha"));
    }
}
