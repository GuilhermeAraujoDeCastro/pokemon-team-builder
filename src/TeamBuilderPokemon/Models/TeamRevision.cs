using System.Text.Json;

namespace TeamBuilderPokemon.Models;

/// <summary>
/// Foto de como o time estava antes de uma edicao, pra dar pra voltar atras.
/// </summary>
public class TeamRevision
{
    public int Id { get; set; }

    public int TeamId { get; set; }
    public Team? Team { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>JSON de <see cref="TeamSnapshot"/>.</summary>
    public string SnapshotJson { get; set; } = "{}";

    public TeamSnapshot ReadSnapshot()
    {
        return JsonSerializer.Deserialize<TeamSnapshot>(SnapshotJson) ?? new TeamSnapshot();
    }

    public static TeamRevision FromTeam(Team team)
    {
        var snapshot = new TeamSnapshot
        {
            Name = team.Name,
            IsPublic = team.IsPublic,
            Slots = team.Slots
                .OrderBy(s => s.SlotNumber)
                .Select(s => new SlotSnapshot { PokemonId = s.PokemonId, Nickname = s.Nickname, Level = s.Level })
                .ToList(),
        };
        return new TeamRevision { TeamId = team.Id, SnapshotJson = JsonSerializer.Serialize(snapshot) };
    }
}

public class TeamSnapshot
{
    public string Name { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public List<SlotSnapshot> Slots { get; set; } = new();
}

public class SlotSnapshot
{
    public int PokemonId { get; set; }
    public string? Nickname { get; set; }
    public int Level { get; set; } = TeamSlot.DefaultLevel;
}
