using System.Text.Json;
using System.Text.Json.Serialization;
using TeamBuilderPokemon.Models;

namespace TeamBuilderPokemon.Services;

public class PortableTeam
{
    public int SchemaVersion { get; set; }
    public string Name { get; set; } = "";
    public bool? IsPublic { get; set; }
    public List<PortableSlot> Slots { get; set; } = new();
}

public class PortableSlot
{
    public string Species { get; set; } = "";
    public string? Nickname { get; set; }
    public int Level { get; set; }
}

public static class TeamCodec
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    public static string Export(Team team) => JsonSerializer.Serialize(new PortableTeam
    {
        SchemaVersion = 1, Name = team.Name, IsPublic = team.IsPublic,
        Slots = team.Slots.OrderBy(s => s.SlotNumber).Select(s => new PortableSlot
        { Species = s.Pokemon?.Name ?? throw new InvalidOperationException("Espécie não carregada."), Nickname = s.Nickname, Level = s.Level }).ToList()
    }, Options);

    public static (PortableTeam? Team, List<string> Errors) Parse(string? json, IReadOnlyCollection<Pokemon> catalog)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(json) || json.Length > 65536)
            return (null, new() { "JSON: informe até 64 KiB de texto." });
        PortableTeam? team;
        try
        {
            using var document = JsonDocument.Parse(json, new JsonDocumentOptions { MaxDepth = 16 });
            CheckDuplicates(document.RootElement);
            team = JsonSerializer.Deserialize<PortableTeam>(json, Options);
        }
        catch (JsonException ex) { return (null, new() { $"JSON inválido no campo {ex.Path ?? "$"}." }); }
        if (team is null) return (null, new() { "JSON: esperado objeto de time." });
        if (team.SchemaVersion != 1) errors.Add("schemaVersion: versão suportada é 1.");
        errors.AddRange(TeamRules.ValidateName(team.Name).Select(e => $"name: {e}"));
        if (team.IsPublic is null) errors.Add("isPublic: informe true ou false.");
        if (team.Slots is null || team.Slots.Count is < 1 or > Team.MaxSlots)
            errors.Add("slots: informe de um a seis membros.");
        else
        {
            var seen = new HashSet<int>();
            for (var i = 0; i < team.Slots.Count; i++)
            {
                var slot = team.Slots[i];
                if (slot is null) { errors.Add($"slots[{i}]: membro ausente."); continue; }
                var pokemon = catalog.FirstOrDefault(p => string.Equals(p.Name, slot.Species, StringComparison.OrdinalIgnoreCase));
                if (pokemon is null) errors.Add($"slots[{i}].species: espécie ausente do catálogo. Importe-a antes de confirmar o time.");
                else if (!seen.Add(pokemon.Id)) errors.Add($"slots[{i}].species: espécie repetida.");
                errors.AddRange(TeamRules.ValidateSlot(slot.Nickname, slot.Level).Select(e => $"slots[{i}]: {e}"));
            }
        }
        return (team, errors);
    }

    private static void CheckDuplicates(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>();
            foreach (var property in element.EnumerateObject())
            {
                if (!names.Add(property.Name)) throw new JsonException("Campo repetido.");
                CheckDuplicates(property.Value);
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
            foreach (var child in element.EnumerateArray()) CheckDuplicates(child);
    }
}
