using TeamBuilderPokemon.Services;
using Xunit;

namespace TeamBuilderPokemon.Tests;

public class TeamRulesTests
{
    private static readonly HashSet<int> Existing = new() { 1, 2, 3, 4, 5, 6, 7 };

    [Fact]
    public void RejectsDuplicatePokemon()
    {
        var errors = TeamRules.ValidatePokemonIds(new List<int> { 1, 2, 1 }, Existing);
        Assert.Contains(errors, e => e.Contains("repita"));
    }

    [Fact]
    public void RejectsMoreThanSix()
    {
        var errors = TeamRules.ValidatePokemonIds(new List<int> { 1, 2, 3, 4, 5, 6, 7 }, Existing);
        Assert.Contains(errors, e => e.Contains("máximo"));
    }

    [Fact]
    public void RejectsUnknownPokemonId()
    {
        // Antes isso virava erro 500 (chave estrangeira) em vez de mensagem na tela.
        var errors = TeamRules.ValidatePokemonIds(new List<int> { 1, 999 }, Existing);
        Assert.Contains(errors, e => e.Contains("não existe"));
    }

    [Fact]
    public void AcceptsAValidTeam()
    {
        Assert.Empty(TeamRules.ValidatePokemonIds(new List<int> { 3, 1, 2 }, Existing));
        Assert.Empty(TeamRules.ValidateName("Meu time"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void RejectsLevelOutOfRange(int level)
    {
        Assert.NotEmpty(TeamRules.ValidateSlot(null, level));
    }

    [Fact]
    public void BlankNicknameBecomesNull()
    {
        Assert.Null(TeamRules.CleanNickname("   "));
        Assert.Equal("Sparky", TeamRules.CleanNickname("  Sparky "));
    }
}
