namespace TeamBuilderPokemon.Models;

/// <summary>
/// Catalogo fixo de 36 Pokemon usado pra popular o banco (via HasData, nas
/// migrations) e escolher os times. Os 36 juntos cobrem os 18 tipos que
/// existem, entao qualquer time formado a partir daqui tem uma analise de
/// tipos que faz sentido.
/// </summary>
public static class PokemonSeedData
{
    public static readonly Pokemon[] All =
    {
        new() { Id = 1, Name = "Bulbasaur", Type1 = "Grass", Type2 = "Poison" },
        new() { Id = 2, Name = "Charmander", Type1 = "Fire" },
        new() { Id = 3, Name = "Squirtle", Type1 = "Water" },
        new() { Id = 4, Name = "Pikachu", Type1 = "Electric" },
        new() { Id = 5, Name = "Eevee", Type1 = "Normal" },
        new() { Id = 6, Name = "Machop", Type1 = "Fighting" },
        new() { Id = 7, Name = "Gastly", Type1 = "Ghost", Type2 = "Poison" },
        new() { Id = 8, Name = "Onix", Type1 = "Rock", Type2 = "Ground" },
        new() { Id = 9, Name = "Gyarados", Type1 = "Water", Type2 = "Flying" },
        new() { Id = 10, Name = "Alakazam", Type1 = "Psychic" },
        new() { Id = 11, Name = "Scyther", Type1 = "Bug", Type2 = "Flying" },
        new() { Id = 12, Name = "Dragonite", Type1 = "Dragon", Type2 = "Flying" },
        new() { Id = 13, Name = "Umbreon", Type1 = "Dark" },
        new() { Id = 14, Name = "Steelix", Type1 = "Steel", Type2 = "Ground" },
        new() { Id = 15, Name = "Togepi", Type1 = "Fairy" },
        new() { Id = 16, Name = "Snorlax", Type1 = "Normal" },
        new() { Id = 17, Name = "Articuno", Type1 = "Ice", Type2 = "Flying" },
        new() { Id = 18, Name = "Vulpix", Type1 = "Fire" },
        new() { Id = 19, Name = "Poliwag", Type1 = "Water" },
        new() { Id = 20, Name = "Magnemite", Type1 = "Electric", Type2 = "Steel" },
        new() { Id = 21, Name = "Oddish", Type1 = "Grass", Type2 = "Poison" },
        new() { Id = 22, Name = "Cubone", Type1 = "Ground" },
        new() { Id = 23, Name = "Zubat", Type1 = "Poison", Type2 = "Flying" },
        new() { Id = 24, Name = "Drowzee", Type1 = "Psychic" },
        new() { Id = 25, Name = "Growlithe", Type1 = "Fire" },
        new() { Id = 26, Name = "Geodude", Type1 = "Rock", Type2 = "Ground" },
        new() { Id = 27, Name = "Krabby", Type1 = "Water" },
        new() { Id = 28, Name = "Abra", Type1 = "Psychic" },
        new() { Id = 29, Name = "Sandshrew", Type1 = "Ground" },
        new() { Id = 30, Name = "Jigglypuff", Type1 = "Normal", Type2 = "Fairy" },
        new() { Id = 31, Name = "Mareep", Type1 = "Electric" },
        new() { Id = 32, Name = "Larvitar", Type1 = "Rock", Type2 = "Ground" },
        new() { Id = 33, Name = "Absol", Type1 = "Dark" },
        new() { Id = 34, Name = "Riolu", Type1 = "Fighting" },
        new() { Id = 35, Name = "Snorunt", Type1 = "Ice" },
        new() { Id = 36, Name = "Gible", Type1 = "Dragon", Type2 = "Ground" },
    };
}
