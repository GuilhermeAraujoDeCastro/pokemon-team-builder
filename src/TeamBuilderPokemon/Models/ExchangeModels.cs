using TeamBuilderPokemon.Services;
namespace TeamBuilderPokemon.Models;

public class ImportTeamViewModel
{
    public string Json { get; set; } = "";
    public PortableTeam? Preview { get; set; }
    public List<string> Errors { get; set; } = new();
}

public record RevisionComparison(Team Team, TeamRevision Revision, List<string> Changes);
