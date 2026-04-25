namespace TeamBuilderPokemon.Models;

public class Team
{
    public const int MaxSlots = 6;

    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>Id do IdentityUser dono do time (AspNetUsers.Id).</summary>
    public string UserId { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>Quando verdadeiro, qualquer pessoa ve o time pelo link publico, sem login.</summary>
    public bool IsPublic { get; set; }

    /// <summary>Preenchido = time na lixeira (exclusao reversivel).</summary>
    public DateTime? DeletedAtUtc { get; set; }

    public List<TeamSlot> Slots { get; set; } = new();

    public List<TeamRevision> Revisions { get; set; } = new();
}
