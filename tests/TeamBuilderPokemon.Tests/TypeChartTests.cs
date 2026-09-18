using TeamBuilderPokemon.Services;
using Xunit;

namespace TeamBuilderPokemon.Tests;

public class TypeChartTests
{
    [Fact]
    public void Effectiveness_SuperEffective_ReturnsDoubleMultiplier()
    {
        var multiplier = TypeChart.Effectiveness("Water", new[] { "Fire" });
        Assert.Equal(2.0, multiplier);
    }

    [Fact]
    public void Effectiveness_NotVeryEffective_ReturnsHalfMultiplier()
    {
        var multiplier = TypeChart.Effectiveness("Fire", new[] { "Water" });
        Assert.Equal(0.5, multiplier);
    }

    [Fact]
    public void Effectiveness_Immunity_ReturnsZero()
    {
        var multiplier = TypeChart.Effectiveness("Electric", new[] { "Ground" });
        Assert.Equal(0.0, multiplier);
    }

    [Fact]
    public void Effectiveness_DualType_MultipliesBothTypes()
    {
        // Gelo e 2x contra Grama e 2x contra Dragao: um defensor com os dois
        // tipos toma o produto das duas vezes (4x), nao so uma delas.
        var multiplier = TypeChart.Effectiveness("Ice", new[] { "Grass", "Dragon" });
        Assert.Equal(4.0, multiplier);
    }

    [Fact]
    public void Effectiveness_IsCaseInsensitive()
    {
        var lower = TypeChart.Effectiveness("fire", new[] { "water" });
        var mixed = TypeChart.Effectiveness("Fire", new[] { "Water" });
        Assert.Equal(mixed, lower);
    }
}
