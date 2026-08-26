using TeamBuilderPokemon.Models;
namespace TeamBuilderPokemon.Services;

public static class RevisionComparer
{
    public static List<string> Compare(Team team, TeamSnapshot before, IReadOnlyDictionary<int,string> catalog)
    {
        var changes = new List<string>();
        if (team.Name != before.Name) changes.Add($"Nome: {team.Name} → {before.Name}");
        if (team.IsPublic != before.IsPublic) changes.Add($"Visibilidade: {(team.IsPublic ? "público" : "privado")} → {(before.IsPublic ? "público" : "privado")}");
        var current = team.Slots.OrderBy(s => s.SlotNumber).ToList();
        string Description(int id,string? nick,int level) => $"{catalog.GetValueOrDefault(id,"espécie indisponível")} | apelido: {nick ?? "nenhum"} | nível {level}";
        for (var i=0;i<Math.Max(current.Count,before.Slots.Count);i++)
        {
            var a=i<current.Count ? current[i] : null;
            var b=i<before.Slots.Count ? before.Slots[i] : null;
            if (a?.PokemonId == b?.PokemonId && a?.Nickname == b?.Nickname && a?.Level == b?.Level) continue;
            changes.Add($"Slot {i+1}: {(a is null ? "vazio" : Description(a.PokemonId,a.Nickname,a.Level))} → {(b is null ? "vazio" : Description(b.PokemonId,b.Nickname,b.Level))}");
        }
        return changes;
    }
}
