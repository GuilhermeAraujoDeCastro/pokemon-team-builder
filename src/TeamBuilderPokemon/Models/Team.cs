namespace TeamBuilderPokemon.Models;

public class Team
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>Id do IdentityUser dono do time (AspNetUsers.Id).</summary>
    public string UserId { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public List<TeamSlot> Slots { get; set; } = new();
}
