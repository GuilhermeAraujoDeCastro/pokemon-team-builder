using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamBuilderPokemon.Data;
using TeamBuilderPokemon.Models;
using TeamBuilderPokemon.Services;

namespace TeamBuilderPokemon.Controllers;

/// <summary>
/// CRUD dos times do usuario logado, mais a analise de tipos de cada time
/// (calculada na hora em Details, nunca guardada no banco).
/// </summary>
[Authorize]
public class TeamsController : Controller
{
    private const int MaxSlots = 6;

    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public TeamsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: /Teams
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);

        var teams = await _context.Teams
            .Where(t => t.UserId == userId)
            .Include(t => t.Slots)
                .ThenInclude(s => s.Pokemon)
            .OrderByDescending(t => t.CreatedAtUtc)
            .ToListAsync();

        return View(teams);
    }

    // GET: /Teams/Create
    public async Task<IActionResult> Create()
    {
        ViewBag.Roster = await _context.Pokemons.OrderBy(p => p.Name).ToListAsync();
        return View();
    }

    // POST: /Teams/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name, List<int> pokemonIds)
    {
        pokemonIds ??= new List<int>();
        pokemonIds = pokemonIds.Where(id => id > 0).Distinct().Take(MaxSlots).ToList();

        if (string.IsNullOrWhiteSpace(name))
        {
            ModelState.AddModelError(nameof(name), "De um nome para o seu time.");
        }

        if (pokemonIds.Count == 0)
        {
            ModelState.AddModelError(nameof(pokemonIds), "Escolha pelo menos 1 Pokemon para o time.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Roster = await _context.Pokemons.OrderBy(p => p.Name).ToListAsync();
            return View();
        }

        var userId = _userManager.GetUserId(User)!;
        var team = new Team
        {
            Name = name.Trim(),
            UserId = userId,
            CreatedAtUtc = DateTime.UtcNow,
        };

        for (var i = 0; i < pokemonIds.Count; i++)
        {
            team.Slots.Add(new TeamSlot { PokemonId = pokemonIds[i], SlotNumber = i + 1 });
        }

        _context.Teams.Add(team);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = team.Id });
    }

    // GET: /Teams/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var userId = _userManager.GetUserId(User);

        var team = await _context.Teams
            .Include(t => t.Slots)
                .ThenInclude(s => s.Pokemon)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (team is null)
        {
            return NotFound();
        }

        var fullRoster = await _context.Pokemons.ToListAsync();
        var teamPokemon = team.Slots
            .Where(s => s.Pokemon is not null)
            .OrderBy(s => s.SlotNumber)
            .Select(s => s.Pokemon!)
            .ToList();

        ViewBag.Analysis = TeamAnalyzer.Analyze(teamPokemon, fullRoster);

        return View(team);
    }

    // POST: /Teams/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = _userManager.GetUserId(User);
        var team = await _context.Teams.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (team is null)
        {
            return NotFound();
        }

        _context.Teams.Remove(team);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
