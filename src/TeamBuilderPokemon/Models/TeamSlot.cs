namespace TeamBuilderPokemon.Models;

/// <summary>
/// Uma posicao ocupada por um Pokemon dentro de um time (no maximo 6 por
/// time). Existe como entidade separada, em vez de um List&lt;Pokemon&gt;
/// direto em Team, pra guardar a ordem (SlotNumber) e deixar espaco pra
/// atributos futuros por posicao (nivel, apelido) sem mexer no resto.
/// </summary>
public class TeamSlot
{
    public int Id { get; set; }

    public int TeamId { get; set; }
    public Team? Team { get; set; }

    public int PokemonId { get; set; }
    public Pokemon? Pokemon { get; set; }

    public int SlotNumber { get; set; }
}
