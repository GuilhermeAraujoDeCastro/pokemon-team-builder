namespace TeamBuilderPokemon.Models;

/// <summary>Formulario de edicao: nome, visibilidade e os slots na ordem.</summary>
public class EditTeamInput
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsPublic { get; set; }

    public List<EditSlotInput> Slots { get; set; } = new();
}

public class EditSlotInput
{
    public int PokemonId { get; set; }

    public string? Nickname { get; set; }

    public int Level { get; set; } = TeamSlot.DefaultLevel;
}

/// <summary>Resultado da tela de batalha: os dois times e o relatorio.</summary>
public class BattleViewModel
{
    public Team Team { get; set; } = new();

    public string OpponentName { get; set; } = string.Empty;

    public Services.BattleReport? Report { get; set; }

    public List<Team> OtherTeams { get; set; } = new();
}
