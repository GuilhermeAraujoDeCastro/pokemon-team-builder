using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamBuilderPokemon.Data;
using TeamBuilderPokemon.Models;
using TeamBuilderPokemon.Services;

namespace TeamBuilderPokemon.Controllers;

/// <summary>
/// API JSON pra um front-end separado no futuro. Usa o mesmo login por cookie e o mesmo TeamAnalyzer das telas.
/// </summary>
[ApiController]
[Route("api")]
public class ApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public ApiController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET /api/pokemon
    [HttpGet("pokemon")]
    public async Task<IActionResult> Pokemon()
    {
        var roster = await _context.Pokemons.OrderBy(p => p.Name).ToListAsync();
        return Ok(roster.Select(PokemonJson));
    }

    // GET /api/teams  (times do usuario logado)
    [Authorize]
    [HttpGet("teams")]
    public async Task<IActionResult> Teams()
    {
        var userId = _userManager.GetUserId(User);
        var teams = await _context.Teams
            .Where(t => t.UserId == userId && t.DeletedAtUtc == null)
            .Include(t => t.Slots).ThenInclude(s => s.Pokemon)
            .OrderByDescending(t => t.CreatedAtUtc)
            .ToListAsync();
        return Ok(teams.Select(TeamJson));
    }

    // GET /api/teams/5/analysis  (dono do time, ou qualquer um se o time for publico)
    [HttpGet("teams/{id:int}/analysis")]
    public async Task<IActionResult> Analysis(int id)
    {
        var userId = _userManager.GetUserId(User);
        var team = await _context.Teams
            .Include(t => t.Slots).ThenInclude(s => s.Pokemon)
            .FirstOrDefaultAsync(t => t.Id == id && t.DeletedAtUtc == null && (t.IsPublic || t.UserId == userId));
        if (team is null)
        {
            return NotFound();
        }

        var members = team.Slots.Where(s => s.Pokemon is not null).OrderBy(s => s.SlotNumber).Select(s => s.Pokemon!).ToList();
        var analysis = TeamAnalyzer.Analyze(members, await _context.Pokemons.ToListAsync());
        return Ok(new { team = TeamJson(team), analysis });
    }

    private static object PokemonJson(Pokemon p) => new
    {
        p.Id,
        p.Name,
        types = p.GetTypes(),
        p.DexNumber,
        sprite = p.GetSpriteUrl(),
    };

    private static object TeamJson(Team t) => new
    {
        t.Id,
        t.Name,
        t.IsPublic,
        t.CreatedAtUtc,
        slots = t.Slots.OrderBy(s => s.SlotNumber).Select(s => new
        {
            s.SlotNumber,
            s.Nickname,
            s.Level,
            pokemon = s.Pokemon is null ? null : PokemonJson(s.Pokemon),
        }),
    };
}
