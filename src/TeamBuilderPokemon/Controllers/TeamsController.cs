using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamBuilderPokemon.Data;
using TeamBuilderPokemon.Models;
using TeamBuilderPokemon.Services;

namespace TeamBuilderPokemon.Controllers;

/// <summary>
/// Times do usuario logado: criar, editar, lixeira, historico, batalha e pagina publica.
/// A analise de tipos e' calculada na hora, nunca guardada no banco.
/// </summary>
[Authorize]
public class TeamsController : Controller
{
    private const int RevisionsShown = 10;

    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly PokeApiClient _pokeApi;

    public TeamsController(ApplicationDbContext context, UserManager<IdentityUser> userManager, PokeApiClient pokeApi)
    {
        _context = context;
        _userManager = userManager;
        _pokeApi = pokeApi;
    }

    // GET: /Teams
    public IActionResult ImportTeam() => View(new ImportTeamViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(100_000)]
    public async Task<IActionResult> PreviewImport(string json)
    {
        var (team, errors) = TeamCodec.Parse(json, await _context.Pokemons.ToListAsync());
        return View("ImportTeam", new ImportTeamViewModel { Json = json, Preview = errors.Count == 0 ? team : null, Errors = errors });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(100_000)]
    public async Task<IActionResult> ConfirmImport(string json)
    {
        var catalog = await _context.Pokemons.ToListAsync();
        var (portable, errors) = TeamCodec.Parse(json,catalog);
        if (errors.Count != 0 || portable is null)
            return View("ImportTeam",new ImportTeamViewModel { Json = json, Errors = errors });
        var team = new Team { Name = portable.Name.Trim(), IsPublic = portable.IsPublic!.Value, UserId = _userManager.GetUserId(User)! };
        for (var i=0;i<portable.Slots.Count;i++)
        {
            var slot=portable.Slots[i];
            team.Slots.Add(new TeamSlot { SlotNumber=i+1, PokemonId=catalog.Single(p => string.Equals(p.Name,slot.Species,StringComparison.OrdinalIgnoreCase)).Id, Nickname=TeamRules.CleanNickname(slot.Nickname), Level=slot.Level });
        }
        _context.Teams.Add(team);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Details),new { id=team.Id });
    }

    public async Task<IActionResult> ExportTeam(int id)
    {
        var team=await FindOwnTeam(id);
        return team is null ? NotFound() : File(System.Text.Encoding.UTF8.GetBytes(TeamCodec.Export(team)),"application/json",$"time-{team.Id}.json");
    }

    public async Task<IActionResult> CompareRevision(int id,int revisionId)
    {
        var team=await FindOwnTeam(id);
        var revision=await _context.TeamRevisions.SingleOrDefaultAsync(r => r.TeamId == id && r.Id == revisionId);
        if (team is null || revision is null) return NotFound();
        var catalog=await _context.Pokemons.ToDictionaryAsync(p => p.Id,p => p.Name);
        return View(new RevisionComparison(team,revision,RevisionComparer.Compare(team,revision.ReadSnapshot(),catalog)));
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        var teams = await OwnTeams(userId)
            .Where(t => t.DeletedAtUtc == null)
            .OrderByDescending(t => t.CreatedAtUtc)
            .ToListAsync();

        ViewBag.TrashCount = await _context.Teams.CountAsync(t => t.UserId == userId && t.DeletedAtUtc != null);
        return View(teams);
    }

    // GET: /Teams/Create
    public async Task<IActionResult> Create()
    {
        ViewBag.Roster = await Roster();
        return View();
    }

    // POST: /Teams/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name, List<int> pokemonIds)
    {
        pokemonIds ??= new List<int>();
        var existing = (await _context.Pokemons.Select(p => p.Id).ToListAsync()).ToHashSet();
        AddErrors(TeamRules.ValidateName(name));
        AddErrors(TeamRules.ValidatePokemonIds(pokemonIds, existing));

        if (!ModelState.IsValid)
        {
            ViewBag.Roster = await Roster();
            ViewBag.Name = name;
            ViewBag.Selected = pokemonIds;
            return View();
        }

        var team = new Team { Name = name.Trim(), UserId = _userManager.GetUserId(User)!, CreatedAtUtc = DateTime.UtcNow };
        // A ordem da lista (montada pelo JS na ordem dos cliques) vira a ordem dos slots.
        for (var i = 0; i < pokemonIds.Count; i++)
        {
            team.Slots.Add(new TeamSlot { PokemonId = pokemonIds[i], SlotNumber = i + 1 });
        }

        _context.Teams.Add(team);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id = team.Id });
    }

    // POST: /Teams/ImportPokemon  (amplia o catalogo com um Pokemon da PokeAPI)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportPokemon(string name, int? teamId)
    {
        if (teamId is not null && await FindOwnTeam(teamId.Value) is null) return NotFound(new { error = "Time não encontrado." });
        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest(new { error = "Digite o nome do Pokémon para buscar na PokeAPI." });
        }
        if (name.Length > 40) // o maxlength da tela não vale pra POST feito na mão
        {
            return BadRequest(new { error = "O nome deve ter até 40 caracteres." });
        }

        try
        {
            var normalized = PokeApiClient.NormalizeName(name);
            var pokemon = await _context.Pokemons.FirstOrDefaultAsync(p => p.Name.ToLower() == normalized);
            var existing = pokemon is not null;
            pokemon ??= await _pokeApi.FetchAsync(name);
            if (pokemon is null)
            {
                return NotFound(new { error = $"Não achei {name} na PokeAPI. Confira a grafia em inglês." });
            }
            if (!existing)
            {
                var found = await _context.Pokemons.FirstOrDefaultAsync(p => p.Name == pokemon.Name);
                if (found is not null) pokemon = found;
                else { _context.Pokemons.Add(pokemon); await _context.SaveChangesAsync(); }
            }
            return Json(new { message = $"{pokemon.Name} disponível no catálogo. Seu rascunho foi mantido.", pokemon = new { pokemon.Id, pokemon.Name, types = pokemon.GetTypes() } });
        }
        catch (DbUpdateException)
        {
            // Dois pedidos do mesmo Pokémon ao mesmo tempo: o índice único barra o segundo, que não vira erro 500.
            return Conflict(new { error = "O catálogo mudou durante a importação. Tente novamente; seu rascunho está mantido." });
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return StatusCode(503,new { error = "A PokeAPI não respondeu agora. Tente novamente em instantes." });
        }
    }

    // GET: /Teams/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var team = await FindOwnTeam(id);
        if (team is null)
        {
            return NotFound();
        }

        ViewBag.Analysis = await AnalyzeAsync(team);
        ViewBag.Revisions = await _context.TeamRevisions
            .Where(r => r.TeamId == team.Id)
            .OrderByDescending(r => r.CreatedAtUtc)
            .Take(RevisionsShown)
            .ToListAsync();
        ViewBag.ShareUrl = Url.Action(nameof(Public), "Teams", new { id = team.Id }, Request.Scheme);
        return View(team);
    }

    // GET: /Teams/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var team = await FindOwnTeam(id);
        if (team is null)
        {
            return NotFound();
        }

        ViewBag.Roster = await Roster();
        return View(ToInput(team));
    }

    // POST: /Teams/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditTeamInput input)
    {
        var team = await FindOwnTeam(id);
        if (team is null)
        {
            return NotFound();
        }

        input.Slots ??= new List<EditSlotInput>();
        input.Slots = input.Slots.Where(s => s.PokemonId > 0).ToList();
        var existing = (await _context.Pokemons.Select(p => p.Id).ToListAsync()).ToHashSet();
        AddErrors(TeamRules.ValidateName(input.Name));
        AddErrors(TeamRules.ValidatePokemonIds(input.Slots.Select(s => s.PokemonId).ToList(), existing));
        foreach (var slot in input.Slots)
        {
            AddErrors(TeamRules.ValidateSlot(slot.Nickname, slot.Level));
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Roster = await Roster();
            input.Id = id;
            return View(input);
        }

        // Guarda como estava antes, pra dar pra voltar pelo historico.
        _context.TeamRevisions.Add(TeamRevision.FromTeam(team));
        await ReplaceSlotsAsync(team, input.Name, input.IsPublic,
            input.Slots.Select(s => new SlotSnapshot { PokemonId = s.PokemonId, Nickname = s.Nickname, Level = s.Level }).ToList());

        TempData["Message"] = "Time atualizado.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: /Teams/RestoreRevision/5?revisionId=9
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RestoreRevision(int id, int revisionId)
    {
        var team = await FindOwnTeam(id);
        var revision = await _context.TeamRevisions.FirstOrDefaultAsync(r => r.Id == revisionId && r.TeamId == id);
        if (team is null || revision is null)
        {
            return NotFound();
        }

        var snapshot = revision.ReadSnapshot();
        var existing = (await _context.Pokemons.Select(p => p.Id).ToListAsync()).ToHashSet();
        var slots = snapshot.Slots;
        var errors = TeamRules.ValidateName(snapshot.Name);
        errors.AddRange(TeamRules.ValidatePokemonIds(slots.Select(s => s.PokemonId).ToList(), existing));
        foreach (var slot in slots) errors.AddRange(TeamRules.ValidateSlot(slot.Nickname,slot.Level));
        if (errors.Count != 0)
        {
            TempData["Error"] = "Revisão não restaurada: " + string.Join(" ",errors);
            return RedirectToAction(nameof(CompareRevision), new { id,revisionId });
        }

        _context.TeamRevisions.Add(TeamRevision.FromTeam(team));
        await ReplaceSlotsAsync(team, snapshot.Name, snapshot.IsPublic, slots);
        TempData["Message"] = $"Versão de {revision.CreatedAtUtc.ToLocalTime():dd/MM HH:mm} restaurada.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: /Teams/Delete/5  (vai pra lixeira, nao apaga de vez)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var team = await _context.Teams.FirstOrDefaultAsync(t => t.Id == id && t.UserId == _userManager.GetUserId(User));
        if (team is null)
        {
            return NotFound();
        }

        team.DeletedAtUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        TempData["Message"] = $"\"{team.Name}\" foi pra lixeira. Dá pra restaurar em Lixeira.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Teams/Trash
    public async Task<IActionResult> Trash()
    {
        var teams = await OwnTeams(_userManager.GetUserId(User))
            .Where(t => t.DeletedAtUtc != null)
            .OrderByDescending(t => t.DeletedAtUtc)
            .ToListAsync();
        return View(teams);
    }

    // POST: /Teams/Restore/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(int id)
    {
        var team = await _context.Teams.FirstOrDefaultAsync(t => t.Id == id && t.UserId == _userManager.GetUserId(User));
        if (team is null)
        {
            return NotFound();
        }

        team.DeletedAtUtc = null;
        await _context.SaveChangesAsync();
        TempData["Message"] = $"\"{team.Name}\" voltou pros seus times.";
        return RedirectToAction(nameof(Index));
    }

    // POST: /Teams/Purge/5  (apaga de vez, so a partir da lixeira)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Purge(int id)
    {
        var team = await _context.Teams.FirstOrDefaultAsync(t =>
            t.Id == id && t.UserId == _userManager.GetUserId(User) && t.DeletedAtUtc != null);
        if (team is null)
        {
            return NotFound();
        }

        _context.Teams.Remove(team);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Trash));
    }

    // GET: /Teams/Battle/5
    public async Task<IActionResult> Battle(int id)
    {
        var team = await FindOwnTeam(id);
        if (team is null)
        {
            return NotFound();
        }
        return View(new BattleViewModel { Team = team, OtherTeams = await OtherTeams(team) });
    }

    // POST: /Teams/Battle/5  (opponentId vazio = time aleatorio do catalogo)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Battle(int id, int? opponentId)
    {
        var team = await FindOwnTeam(id);
        if (team is null)
        {
            return NotFound();
        }

        var rng = new Random();
        List<Fighter> opponentFighters;
        string opponentName;
        var opponent = opponentId is null ? null : await FindOwnTeam(opponentId.Value);
        if (opponentId is not null && opponent is null)
        {
            return NotFound(); // time escolhido que não existe (ou é de outra pessoa): antes virava batalha aleatória
        }
        if (opponent is not null)
        {
            opponentFighters = BattleSimulator.FromSlots(opponent.Slots);
            opponentName = opponent.Name;
        }
        else
        {
            // Time aleatorio: 6 do catalogo, todos no nivel padrao.
            var roster = await Roster();
            opponentFighters = roster.OrderBy(_ => rng.Next()).Take(Team.MaxSlots)
                .Select(p => new Fighter(p.Name, p.GetTypes(), TeamSlot.DefaultLevel)).ToList();
            opponentName = "Time aleatório";
        }

        var report = BattleSimulator.Simulate(BattleSimulator.FromSlots(team.Slots), opponentFighters, rng);
        return View(new BattleViewModel { Team = team, OpponentName = opponentName, Report = report, OtherTeams = await OtherTeams(team) });
    }

    // GET: /Teams/Public/5  (link pra compartilhar, sem login)
    [AllowAnonymous]
    public async Task<IActionResult> Public(int id)
    {
        var team = await _context.Teams
            .Include(t => t.Slots).ThenInclude(s => s.Pokemon)
            .FirstOrDefaultAsync(t => t.Id == id && t.IsPublic && t.DeletedAtUtc == null && t.UserId != null && _context.Users.Any(u => u.Id == t.UserId));
        if (team is null)
        {
            return NotFound();
        }

        ViewBag.Analysis = await AnalyzeAsync(team);
        return View(team);
    }

    // ---------- apoio ----------

    private IQueryable<Team> OwnTeams(string? userId)
    {
        return _context.Teams
            .Where(t => userId != null && t.UserId != null && t.UserId == userId)
            .Include(t => t.Slots).ThenInclude(s => s.Pokemon);
    }

    // Time do usuario logado que nao esta na lixeira.
    private Task<Team?> FindOwnTeam(int id)
    {
        return OwnTeams(_userManager.GetUserId(User)).FirstOrDefaultAsync(t => t.Id == id && t.DeletedAtUtc == null);
    }

    private Task<List<Pokemon>> Roster()
    {
        return _context.Pokemons.OrderBy(p => p.Name).ToListAsync();
    }

    private async Task<List<Team>> OtherTeams(Team team)
    {
        return await OwnTeams(team.UserId)
            .Where(t => t.Id != team.Id && t.DeletedAtUtc == null)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    private async Task<TeamAnalysisResult> AnalyzeAsync(Team team)
    {
        var teamPokemon = team.Slots.Where(s => s.Pokemon is not null).OrderBy(s => s.SlotNumber).Select(s => s.Pokemon!).ToList();
        return TeamAnalyzer.Analyze(teamPokemon, await _context.Pokemons.ToListAsync());
    }

    private static EditTeamInput ToInput(Team team)
    {
        return new EditTeamInput
        {
            Id = team.Id,
            Name = team.Name,
            IsPublic = team.IsPublic,
            Slots = team.Slots.OrderBy(s => s.SlotNumber)
                .Select(s => new EditSlotInput { PokemonId = s.PokemonId, Nickname = s.Nickname, Level = s.Level })
                .ToList(),
        };
    }

    // Troca os slots em duas etapas: apagar e inserir no mesmo SaveChanges bateria no indice unico (TeamId, SlotNumber).
    private async Task ReplaceSlotsAsync(Team team, string name, bool isPublic, List<SlotSnapshot> slots)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        _context.TeamSlots.RemoveRange(team.Slots);
        team.Name = name.Trim();
        team.IsPublic = isPublic;
        await _context.SaveChangesAsync();

        for (var i = 0; i < slots.Count; i++)
        {
            _context.TeamSlots.Add(new TeamSlot
            {
                TeamId = team.Id,
                PokemonId = slots[i].PokemonId,
                SlotNumber = i + 1,
                Nickname = TeamRules.CleanNickname(slots[i].Nickname),
                Level = Math.Clamp(slots[i].Level, TeamSlot.MinLevel, TeamSlot.MaxLevel),
            });
        }
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    private void AddErrors(IEnumerable<string> errors)
    {
        foreach (var error in errors)
        {
            ModelState.AddModelError(string.Empty, error);
        }
    }
}
