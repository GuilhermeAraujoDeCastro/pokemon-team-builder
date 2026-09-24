using TeamBuilderPokemon.Models;

namespace TeamBuilderPokemon.Services;

/// <summary>Regras de um time valido, separadas do controller pra dar pra testar sem HTTP.</summary>
public static class TeamRules
{
    public static List<string> ValidateName(string? name)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(name))
        {
            errors.Add("Dê um nome para o seu time.");
        }
        else if (name.Trim().Length > 100)
        {
            errors.Add("O nome do time pode ter no máximo 100 caracteres.");
        }
        return errors;
    }

    public static List<string> ValidatePokemonIds(IReadOnlyList<int> pokemonIds, ISet<int> existingIds)
    {
        var errors = new List<string>();
        if (pokemonIds.Count == 0)
        {
            errors.Add("Escolha pelo menos 1 Pokémon para o time.");
        }
        if (pokemonIds.Count > Team.MaxSlots)
        {
            errors.Add($"Um time tem no máximo {Team.MaxSlots} Pokémon.");
        }
        if (pokemonIds.Distinct().Count() != pokemonIds.Count)
        {
            errors.Add("Não repita o mesmo Pokémon no time.");
        }
        if (pokemonIds.Any(id => !existingIds.Contains(id)))
        {
            errors.Add("Um dos Pokémon escolhidos não existe no catálogo.");
        }
        return errors;
    }

    public static List<string> ValidateSlot(string? nickname, int level)
    {
        var errors = new List<string>();
        if (nickname is not null && nickname.Trim().Length > TeamSlot.NicknameMaxLength)
        {
            errors.Add($"Apelido pode ter no máximo {TeamSlot.NicknameMaxLength} caracteres.");
        }
        if (level < TeamSlot.MinLevel || level > TeamSlot.MaxLevel)
        {
            errors.Add($"O nível vai de {TeamSlot.MinLevel} a {TeamSlot.MaxLevel}.");
        }
        return errors;
    }

    /// <summary>Apelido vazio vira null (usa o nome da especie).</summary>
    public static string? CleanNickname(string? nickname)
    {
        return string.IsNullOrWhiteSpace(nickname) ? null : nickname.Trim();
    }
}
