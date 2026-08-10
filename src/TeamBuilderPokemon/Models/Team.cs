namespace TeamBuilderPokemon.Models;

public class Team
{
    public const int MaxSlots = 6;

    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>Id do IdentityUser dono do time (AspNetUsers.Id).</summary>
    public string? UserId { get; set; }
    public Microsoft.AspNetCore.Identity.IdentityUser? Owner { get; set; }

    /// <summary>Dono legado preservado quando a conta já não existe. Nunca concede acesso.</summary>
    public string? LegacyOwnerId { get; set; }
    public bool? LegacyIsPublic { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>Quando verdadeiro, qualquer pessoa ve o time pelo link publico, sem login.</summary>
    public bool IsPublic { get; set; }

    /// <summary>Preenchido = time na lixeira (exclusao reversivel).</summary>
    public DateTime? DeletedAtUtc { get; set; }

    public List<TeamSlot> Slots { get; set; } = new();

    public List<TeamRevision> Revisions { get; set; } = new();
}
