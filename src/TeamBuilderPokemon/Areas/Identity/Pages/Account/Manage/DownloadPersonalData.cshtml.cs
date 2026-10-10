using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TeamBuilderPokemon.Data;
using TeamBuilderPokemon.Services;

namespace TeamBuilderPokemon.Areas.Identity.Pages.Account.Manage;

[Authorize]
public class DownloadPersonalDataModel : PageModel
{
    private readonly UserManager<IdentityUser> _users;
    private readonly ApplicationDbContext _db;
    public DownloadPersonalDataModel(UserManager<IdentityUser> users,ApplicationDbContext db) { _users=users;_db=db; }
    public IActionResult OnGet() => NotFound();
    public async Task<IActionResult> OnPostAsync()
    {
        await using var transaction=await _db.Database.BeginTransactionAsync();
        var user=await _users.GetUserAsync(User);
        if(user is null)return NotFound();
        var bytes=await AccountDataExport.BuildAsync(_db,user);
        await transaction.CommitAsync();
        return File(bytes,"application/json","dados-pessoais-e-times.json");
    }
}
