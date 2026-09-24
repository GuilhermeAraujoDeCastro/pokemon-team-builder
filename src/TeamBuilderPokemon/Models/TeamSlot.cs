using System.ComponentModel.DataAnnotations;

namespace TeamBuilderPokemon.Models;

/// <summary>
/// Uma posicao do time (ate 6). Entidade propria pra guardar a ordem, o apelido e o nivel.
/// </summary>
public class TeamSlot
{
    public const int MinLevel = 1;
    public const int MaxLevel = 100;
    public const int DefaultLevel = 50;
    public const int NicknameMaxLength = 20;

    public int Id { get; set; }

    public int TeamId { get; set; }
    public Team? Team { get; set; }

    public int PokemonId { get; set; }
    public Pokemon? Pokemon { get; set; }

    public int SlotNumber { get; set; }

    [StringLength(NicknameMaxLength)]
    public string? Nickname { get; set; }

    [Range(MinLevel, MaxLevel)]
    public int Level { get; set; } = DefaultLevel;

    /// <summary>Apelido quando tiver, senao o nome da especie.</summary>
    public string DisplayName => string.IsNullOrWhiteSpace(Nickname) ? Pokemon?.Name ?? "?" : Nickname!;
}
