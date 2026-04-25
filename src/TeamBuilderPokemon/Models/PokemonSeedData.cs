namespace TeamBuilderPokemon.Models;

/// <summary>
/// Catalogo inicial de 36 Pokemon (cobre os 18 tipos). Entra no banco pela migration;
/// outros podem ser importados da PokeAPI pela tela de criar time.
/// </summary>
public static class PokemonSeedData
{
    public static readonly Pokemon[] All =
    {
        new() { Id = 1, Name = "Bulbasaur", Type1 = "Grass", Type2 = "Poison", DexNumber = 1 },
        new() { Id = 2, Name = "Charmander", Type1 = "Fire", DexNumber = 4 },
        new() { Id = 3, Name = "Squirtle", Type1 = "Water", DexNumber = 7 },
        new() { Id = 4, Name = "Pikachu", Type1 = "Electric", DexNumber = 25 },
        new() { Id = 5, Name = "Eevee", Type1 = "Normal", DexNumber = 133 },
        new() { Id = 6, Name = "Machop", Type1 = "Fighting", DexNumber = 66 },
        new() { Id = 7, Name = "Gastly", Type1 = "Ghost", Type2 = "Poison", DexNumber = 92 },
        new() { Id = 8, Name = "Onix", Type1 = "Rock", Type2 = "Ground", DexNumber = 95 },
        new() { Id = 9, Name = "Gyarados", Type1 = "Water", Type2 = "Flying", DexNumber = 130 },
        new() { Id = 10, Name = "Alakazam", Type1 = "Psychic", DexNumber = 65 },
        new() { Id = 11, Name = "Scyther", Type1 = "Bug", Type2 = "Flying", DexNumber = 123 },
        new() { Id = 12, Name = "Dragonite", Type1 = "Dragon", Type2 = "Flying", DexNumber = 149 },
        new() { Id = 13, Name = "Umbreon", Type1 = "Dark", DexNumber = 197 },
        new() { Id = 14, Name = "Steelix", Type1 = "Steel", Type2 = "Ground", DexNumber = 208 },
        new() { Id = 15, Name = "Togepi", Type1 = "Fairy", DexNumber = 175 },
        new() { Id = 16, Name = "Snorlax", Type1 = "Normal", DexNumber = 143 },
        new() { Id = 17, Name = "Articuno", Type1 = "Ice", Type2 = "Flying", DexNumber = 144 },
        new() { Id = 18, Name = "Vulpix", Type1 = "Fire", DexNumber = 37 },
        new() { Id = 19, Name = "Poliwag", Type1 = "Water", DexNumber = 60 },
        new() { Id = 20, Name = "Magnemite", Type1 = "Electric", Type2 = "Steel", DexNumber = 81 },
        new() { Id = 21, Name = "Oddish", Type1 = "Grass", Type2 = "Poison", DexNumber = 43 },
        new() { Id = 22, Name = "Cubone", Type1 = "Ground", DexNumber = 104 },
        new() { Id = 23, Name = "Zubat", Type1 = "Poison", Type2 = "Flying", DexNumber = 41 },
        new() { Id = 24, Name = "Drowzee", Type1 = "Psychic", DexNumber = 96 },
        new() { Id = 25, Name = "Growlithe", Type1 = "Fire", DexNumber = 58 },
        new() { Id = 26, Name = "Geodude", Type1 = "Rock", Type2 = "Ground", DexNumber = 74 },
        new() { Id = 27, Name = "Krabby", Type1 = "Water", DexNumber = 98 },
        new() { Id = 28, Name = "Abra", Type1 = "Psychic", DexNumber = 63 },
        new() { Id = 29, Name = "Sandshrew", Type1 = "Ground", DexNumber = 27 },
        new() { Id = 30, Name = "Jigglypuff", Type1 = "Normal", Type2 = "Fairy", DexNumber = 39 },
        new() { Id = 31, Name = "Mareep", Type1 = "Electric", DexNumber = 179 },
        new() { Id = 32, Name = "Larvitar", Type1 = "Rock", Type2 = "Ground", DexNumber = 246 },
        new() { Id = 33, Name = "Absol", Type1 = "Dark", DexNumber = 359 },
        new() { Id = 34, Name = "Riolu", Type1 = "Fighting", DexNumber = 447 },
        new() { Id = 35, Name = "Snorunt", Type1 = "Ice", DexNumber = 361 },
        new() { Id = 36, Name = "Gible", Type1 = "Dragon", Type2 = "Ground", DexNumber = 443 },
    };
}
